using System.Net;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using MailKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using OperationResults;
using Pixora.Authentication.Entities;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.DataProtectionLayer;
using Pixora.Shared.Models;
using Pixora.Shared.Models.Requests;
using Pixora.Shared.Notifications;
using SimpleAuthentication.JwtBearer;
using SimpleTransit;

namespace Pixora.BusinessLayer.Services;

public class IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, TimeProvider timeProvider, RandomNumberGenerator generator, IDataProtectionService dataProtectionService, ITimeLimitedDataProtectionService timeLimitedDataProtectionService, IJwtBearerService jwtBearerService, INotificationPublisher notificationPublisher) : IIdentityService
{
    public async Task<Result> ConfirmEmailAsync(string secret, string token, CancellationToken cancellationToken)
    {
        ApplicationUser? user;

        var decodedSecret = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(secret));
        var decodedToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));

        try
        {
            var userId = await timeLimitedDataProtectionService.UnprotectAsync(decodedSecret, cancellationToken);
            user = await userManager.FindByIdAsync(userId);
        }
        catch
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid or expired token", "Invalid or expired token");
        }

        if (user is null)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid or expired token", "Invalid or expired token");
        }

        var result = await userManager.ConfirmEmailAsync(user, decodedToken);
        if (result.Succeeded)
        {
            await notificationPublisher.NotifyAsync(new UserVerifiedMessage(user.Email!), cancellationToken);
            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ClientError, "Invalid or expired token", "Invalid or expired token");
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid email or password", "Invalid email or password");
        }

        var signInResult = await signInManager.PasswordSignInAsync(user, request.Password, false, false);
        if (!signInResult.Succeeded)
        {
            if (signInResult.IsLockedOut)
            {
                return Result.Fail(FailureReasons.ClientError, "User locked out", $"Your account is locked until {user.LockoutEnd}");
            }

            await userManager.AccessFailedAsync(user);
            return Result.Fail(FailureReasons.ClientError, "Invalid email or password", "Invalid email or password");
        }

        return await CreateResponseAsync(user, cancellationToken);
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var validationResult = await jwtBearerService.TryValidateTokenAsync(request.AccessToken, false);
        if (validationResult.IsValid && validationResult.Principal is not null)
        {
            var user = await userManager.GetUserAsync(validationResult.Principal);
            if (user?.RefreshToken is null || user?.RefreshTokenExpirationDate < timeProvider.GetUtcNow() || user?.RefreshToken != request.RefreshToken)
            {
                return Result.Fail(FailureReasons.ClientError, "Invalid token", "Login failed");
            }

            return await CreateResponseAsync(user, cancellationToken);
        }

        return Result.Fail(FailureReasons.ClientError, "Invalid token", "Login failed");
    }

    public async Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var user = new ApplicationUser
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.UserName,
                EnableNotifications = request.EnableNotifications
            };

            var result = await userManager.CreateAsync(user, request.Password);
            if (result.Succeeded)
            {
                await notificationPublisher.NotifyAsync(new UserRegistratedMessage(request.Email), cancellationToken);
                return Result.Ok();
            }

            return Result.Fail(FailureReasons.ClientError, "Registration failed", string.Join(",", result.Errors.Select(e => e.Description)));
        }
        catch (SocketException ex)
        {
            return Result.Fail(FailureReasons.ClientError, "Couldn't send email", ex.Message);
        }
    }

    private async Task<AuthResponse> CreateResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        await userManager.UpdateSecurityStampAsync(user);
        var userRoles = await userManager.GetRolesAsync(user);

        var hostName = Dns.GetHostName();
        var addresses = await Dns.GetHostAddressesAsync(hostName, cancellationToken);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email!),
            new Claim(ClaimTypes.GivenName, user.FirstName),
            new Claim(ClaimTypes.Surname, user.LastName),
            new Claim(ClaimTypes.SerialNumber, user.SecurityStamp ?? string.Empty),
            new Claim(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty),
            new Claim(ClaimTypes.Dns, hostName)
        }
        .Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)))
        .Union(addresses.Select(address => new Claim(ClaimTypes.Dns, address.ToString())));

        var accessToken = await jwtBearerService.CreateTokenAsync(user.UserName!, claims.ToList());
        var refreshToken = await SaveRefreshTokenAsync(user, cancellationToken);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = timeProvider.GetUtcNow().AddDays(10);

        await userManager.UpdateAsync(user);
        return new AuthResponse(accessToken, refreshToken);
    }

    private async Task<string> SaveRefreshTokenAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var randomNumber = new byte[512];
        generator.GetBytes(randomNumber);

        var refreshToken = await dataProtectionService.ProtectAsync(Convert.ToBase64String(randomNumber), cancellationToken);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = timeProvider.GetUtcNow().AddDays(10);

        await userManager.UpdateAsync(user);
        return refreshToken;
    }
}
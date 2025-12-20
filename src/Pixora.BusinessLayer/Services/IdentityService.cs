using System.Net;
using System.Net.Mime;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using MailKit;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.JsonWebTokens;
using OperationResults;
using Pixora.Authentication.Entities;
using Pixora.BusinessLayer.Generators.Interfaces;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.DataProtectionLayer;
using Pixora.Shared.Models;
using Pixora.Shared.Models.Requests;
using Pixora.Shared.Notifications;
using SimpleAuthentication.JwtBearer;
using SimpleTransit;
using TinyHelpers.Extensions;

namespace Pixora.BusinessLayer.Services;

public class IdentityService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, TimeProvider timeProvider, RandomNumberGenerator generator, IDataProtectionService dataProtectionService, ITimeLimitedDataProtectionService timeLimitedDataProtectionService, IQrCodeGenerator qrCodeGenerator, IJwtBearerService jwtBearerService, INotificationPublisher notificationPublisher) : IIdentityService
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

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid email address", "Invalid email address");
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        await notificationPublisher.NotifyAsync(new UserForgotPasswordMessage(request.Email, token), cancellationToken);

        return Result.Ok();
    }

    public async Task<Result<StreamFileContent>> GetQrCodeAsync(string token, CancellationToken cancellationToken)
    {
        ApplicationUser? user;

        try
        {
            var userId = await timeLimitedDataProtectionService.UnprotectAsync(token, cancellationToken);
            user = await userManager.FindByIdAsync(userId);
        }
        catch
        {
            return Result.Fail(FailureReasons.ClientError, "Problem occurred while generating the QR Code", "Problem occurred while generating the QR Code");
        }

        var stream = await qrCodeGenerator.GenerateAsync(user, cancellationToken);
        if (stream is null)
        {
            return Result.Fail(FailureReasons.ClientError, "Error occurred while generating the qr code");
        }

        var streamFileContent = new StreamFileContent(stream, MediaTypeNames.Image.Png);
        return streamFileContent;
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
            if (signInResult.RequiresTwoFactor)
            {
                var twoFactorToken = await timeLimitedDataProtectionService.ProtectAsync(user.Id.ToString(), TimeSpan.FromMinutes(15), cancellationToken);
                return new AuthResponse(twoFactorToken);
            }

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

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        ApplicationUser? user;

        try
        {
            var userId = await timeLimitedDataProtectionService.UnprotectAsync(request.Secret, cancellationToken);
            user = await userManager.FindByIdAsync(userId);
        }
        catch
        {
            return Result.Fail(FailureReasons.ClientError);
        }

        if (user is null)
        {
            return Result.Fail(FailureReasons.ClientError, "An error occurred", "An error occurred");
        }

        var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);
        if (result.Succeeded)
        {
            await notificationPublisher.NotifyAsync(new UserResetPasswordMessage(user.Email!), cancellationToken);
            return Result.Ok();
        }

        return Result.Fail(FailureReasons.ClientError, "An error occurred", "An error occurred");
    }

    public async Task<Result<AuthResponse>> ValidateTwoFactorAsync(TwoFactorValidationRequest request, CancellationToken cancellationToken)
    {
        ApplicationUser? user;

        try
        {
            var userId = await timeLimitedDataProtectionService.UnprotectAsync(request.Token, cancellationToken);
            user = await userManager.FindByIdAsync(userId);
        }
        catch
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid code", "Invalid code");
        }

        if (user is null)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid code", "Invalid code");
        }

        var isValidTotpCode = await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, request.Code);
        if (!isValidTotpCode)
        {
            return Result.Fail(FailureReasons.ClientError, "Invalid code", "Invalid code");
        }

        return await CreateResponseAsync(user, cancellationToken);
    }

    private async Task<AuthResponse> CreateResponseAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var claims = await GetOrCreateClaimsAsync(user, cancellationToken);
        var userRoles = await userManager.GetRolesAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        var hostName = Dns.GetHostName();
        var addresses = await Dns.GetHostAddressesAsync(hostName, cancellationToken);

        claims.Add(new Claim(ClaimTypes.Dns, hostName));
        claims.Add(new Claim(ClaimTypes.SerialNumber, user.SecurityStamp ?? string.Empty));

        claims.Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.Union(addresses.Select(address => new Claim(ClaimTypes.Dns, address.ToString())));

        var accessToken = await jwtBearerService.CreateTokenAsync(user.UserName!, claims);
        var refreshToken = await SaveRefreshTokenAsync(user, cancellationToken);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpirationDate = timeProvider.GetUtcNow().AddDays(10);

        await userManager.UpdateAsync(user);
        return new AuthResponse(accessToken, refreshToken);
    }

    private async Task<IList<Claim>> GetOrCreateClaimsAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var claims = await userManager.GetClaimsAsync(user);
        if (claims is null || claims.Count == 0)
        {
            claims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(ClaimTypes.Email, user.Email!)
            ];

            if (user.PhoneNumber.HasValue() && user.PhoneNumberConfirmed)
            {
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            }

            await userManager.AddClaimsAsync(user, claims);
        }

        cancellationToken.ThrowIfCancellationRequested();
        return claims;
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
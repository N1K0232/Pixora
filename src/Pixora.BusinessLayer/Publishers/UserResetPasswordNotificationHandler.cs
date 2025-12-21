using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Pixora.Authentication.Entities;
using Pixora.BusinessLayer.Clients.Interfaces;
using Pixora.BusinessLayer.Resources;
using Pixora.BusinessLayer.Settings;
using Pixora.DataProtectionLayer;
using Pixora.Shared.Models;
using Pixora.Shared.Notifications;
using SimpleTransit;

namespace Pixora.BusinessLayer.Publishers;

public class UserResetPasswordNotificationHandler(UserManager<ApplicationUser> userManager, LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, IEmailClient emailClient, IOptions<AppSettings> appSettingsOptions, ITimeLimitedDataProtectionService dataProtectionService) : INotificationHandler<UserForgotPasswordMessage>, INotificationHandler<UserResetPasswordMessage>
{
    private readonly AppSettings appSettings = appSettingsOptions.Value;

    public async Task HandleAsync(UserForgotPasswordMessage message, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(message.Email);
        if (user is not null && await userManager.IsEmailConfirmedAsync(user))
        {
            var secret = await dataProtectionService.ProtectAsync(user.Id.ToString(), TimeSpan.FromMinutes(15), cancellationToken);
            var token = await userManager.GeneratePasswordResetTokenAsync(user);

            var encodedSecret = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(secret));
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var page = linkGenerator.GetUriByPage(httpContextAccessor.HttpContext!, "/Account/ResetPassword", values: new { secret = encodedSecret, token = encodedToken });

            var emailMessage = new EmailMessage
            {
                To = [message.Email],
                SenderEmail = appSettings.SenderEmail,
                SenderName = appSettings.SenderName,
                Subject = EmailSubjects.ForgotPassword,
                TextContent = string.Format(Messages.ForgotPassword, page)
            };

            await emailClient.SendAsync(emailMessage, cancellationToken);
        }
    }

    public async Task HandleAsync(UserResetPasswordMessage message, CancellationToken cancellationToken)
    {
        var emailMessage = new EmailMessage
        {
            To = [message.Email],
            SenderEmail = appSettings.SenderEmail,
            SenderName = appSettings.SenderName,
            Subject = EmailSubjects.PasswordResetSuccessful,
            TextContent = Messages.PasswordResetSuccessful
        };

        await emailClient.SendAsync(emailMessage, cancellationToken);
    }
}
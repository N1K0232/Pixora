using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Pixora.Authentication;
using Pixora.Authentication.Entities;
using Pixora.BusinessLayer.Clients.Interfaces;
using Pixora.BusinessLayer.Resources;
using Pixora.BusinessLayer.Settings;
using Pixora.DataProtectionLayer;
using Pixora.Shared.Models;
using Pixora.Shared.Notifications;
using SimpleTransit;

namespace Pixora.BusinessLayer.Publishers;

public class UserRegistratedNotificationHandler(UserManager<ApplicationUser> userManager, LinkGenerator linkGenerator, IHttpContextAccessor httpContextAccessor, ITimeLimitedDataProtectionService dataProtectionService, IEmailClient emailClient, IOptions<AppSettings> appSettingsOptions) : INotificationHandler<UserRegistratedMessage>, INotificationHandler<UserVerifiedMessage>
{
    private readonly AppSettings appSettings = appSettingsOptions.Value;

    public async Task HandleAsync(UserRegistratedMessage message, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(message.Email);
        if (user is not null)
        {
            var secret = await dataProtectionService.ProtectAsync(user.Id.ToString(), TimeSpan.FromMinutes(15), cancellationToken);
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);

            var encodedSecret = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(secret));
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var httpContext = httpContextAccessor.HttpContext!;
            var page = linkGenerator.GetUriByPage(httpContext, "/Account/UserConfirmEmail", values: new { secret = encodedSecret, token = encodedToken });

            var emailMessage = new EmailMessage
            {
                SenderEmail = appSettings.SenderEmail,
                SenderName = appSettings.SenderName,
                To = [message.Email],
                Subject = EmailSubjects.ConfirmEmail,
                TextContent = string.Format(Messages.ConfirmEmail, page)
            };

            await emailClient.SendAsync(emailMessage, cancellationToken);
        }
    }

    public async Task HandleAsync(UserVerifiedMessage message, CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(message.Email);
        if (user is not null)
        {
            await userManager.AddToRoleAsync(user, RoleNames.User);

            var emailMessage = new EmailMessage
            {
                SenderEmail = appSettings.SenderEmail,
                SenderName = appSettings.SenderName,
                To = [message.Email],
                Subject = EmailSubjects.EmailConfirmed,
                TextContent = Messages.EmailConfirmed
            };

            await emailClient.SendAsync(emailMessage, cancellationToken);
        }
    }
}
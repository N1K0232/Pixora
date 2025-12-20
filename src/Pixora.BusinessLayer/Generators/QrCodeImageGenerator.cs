using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Pixora.Authentication.Entities;
using Pixora.BusinessLayer.Generators.Interfaces;
using Pixora.BusinessLayer.Resources;
using QRCoder;
using TinyHelpers.Extensions;

namespace Pixora.BusinessLayer.Generators;

public class QrCodeImageGenerator(UserManager<ApplicationUser> userManager, QRCodeGenerator generator, IWebHostEnvironment environment) : IQrCodeGenerator
{
    public async Task<Stream?> GenerateAsync(ApplicationUser? user, CancellationToken cancellationToken = default)
    {
        if (user is null || (await userManager.GetAuthenticatorKeyAsync(user)).HasValue())
        {
            return null;
        }

        await userManager.ResetAuthenticatorKeyAsync(user);
        var secret = await userManager.GetAuthenticatorKeyAsync(user);

        var qrCodeUri = CreateUri(secret, user.Email);
        using var qrCodeData = generator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);

        using var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(3);

        var stream = new MemoryStream(qrCodeBytes);
        return stream;
    }

    private string CreateUri(string? secret, string? email)
    {
        var applicationName = Uri.EscapeDataString(environment.ApplicationName);
        return string.Format(UrlAddresses.QRCodeUri, applicationName, email, secret, applicationName);
    }
}
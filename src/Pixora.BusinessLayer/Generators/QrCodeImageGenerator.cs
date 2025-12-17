using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Pixora.Authentication.Entities;
using Pixora.BusinessLayer.Generators.Interfaces;
using QRCoder;

namespace Pixora.BusinessLayer.Generators;

public class QrCodeImageGenerator(UserManager<ApplicationUser> userManager, IWebHostEnvironment environment) : IQrCodeGenerator
{
    public async Task<Stream> GenerateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        await userManager.ResetAuthenticatorKeyAsync(user);
        var secret = await userManager.GetAuthenticatorKeyAsync(user);

        var qrCodeUri = $"otpauth://totp/{Uri.EscapeDataString(environment.ApplicationName)}:{user.Email}?secret={secret}&issuer={Uri.EscapeDataString(environment.ApplicationName)}";
        using var generator = new QRCodeGenerator();

        using var qrCodeData = generator.CreateQrCode(qrCodeUri, QRCodeGenerator.ECCLevel.Q);
        using var qrCode = new PngByteQRCode(qrCodeData);

        var qrCodeBytes = qrCode.GetGraphic(3);
        return new MemoryStream(qrCodeBytes);
    }
}
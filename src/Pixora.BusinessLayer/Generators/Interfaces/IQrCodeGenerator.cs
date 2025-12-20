using Pixora.Authentication.Entities;

namespace Pixora.BusinessLayer.Generators.Interfaces;

public interface IQrCodeGenerator
{
    Task<Stream?> GenerateAsync(ApplicationUser? user, CancellationToken cancellationToken = default);
}
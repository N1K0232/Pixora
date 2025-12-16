using Microsoft.AspNetCore.DataProtection;

namespace Pixora.DataProtectionLayer;

internal class DataProtectionService(IDataProtector dataProtector) : IDataProtectionService
{
    public Task<string> ProtectAsync(string plaintext, CancellationToken cancellationToken = default)
    {
        var protectedData = dataProtector.Protect(plaintext);
        return Task.FromResult(protectedData);
    }

    public Task<string> UnprotectAsync(string protectedData, CancellationToken cancellationToken = default)
    {
        var plaintext = dataProtector.Unprotect(protectedData);
        return Task.FromResult(plaintext);
    }
}
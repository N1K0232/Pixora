using Microsoft.AspNetCore.DataProtection;

namespace Pixora.DataProtectionLayer;

internal class TimeLimitedDataProtectionService(ITimeLimitedDataProtector dataProtector) : ITimeLimitedDataProtectionService
{
    public Task<string> ProtectAsync(string plaintext, TimeSpan lifetime, CancellationToken cancellationToken = default)
    {
        var protectedData = dataProtector.Protect(plaintext, lifetime);
        return Task.FromResult(protectedData);
    }

    public Task<string> UnprotectAsync(string protectedData, CancellationToken cancellationToken = default)
    {
        var plaintext = dataProtector.Unprotect(protectedData);
        return Task.FromResult(plaintext);
    }
}
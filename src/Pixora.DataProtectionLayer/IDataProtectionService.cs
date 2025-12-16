namespace Pixora.DataProtectionLayer;

public interface IDataProtectionService
{
    Task<string> ProtectAsync(string plaintext, CancellationToken cancellationToken = default);

    Task<string> UnprotectAsync(string protectedData, CancellationToken cancellationToken = default);
}
namespace Pixora.StorageProviders;

public interface IStorageProvider
{
    Task SaveAsync(Stream stream, string path, bool overwrite = false, CancellationToken cancellationToken = default);

    Task<Stream?> ReadAsStreamAsync(string path, CancellationToken cancellationToken = default);

    Task DeleteAsync(string path, CancellationToken cancellationToken = default);
}
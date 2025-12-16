
namespace Pixora.StorageProviders.FileSystem;

internal class FileSystemStorageProvider(FileSystemStorageOptions options) : IStorageProvider
{
    public async Task SaveAsync(Stream stream, string path, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(path);
        var directoryName = Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directoryName) && !Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }

        stream.Position = 0;
        using var fileStream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write);

        await stream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);
        fileStream.Close();
    }

    public Task<Stream?> ReadAsStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(path);

        if (!File.Exists(fullPath))
        {
            return Task.FromResult<Stream?>(null);
        }

        var stream = File.OpenRead(fullPath);
        return Task.FromResult<Stream?>(stream);
    }

    public Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = GetFullPath(path);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private string GetFullPath(string path)
    {
        var fullPath = Path.Combine(options.StorageFolder, path);

        if (!Path.IsPathRooted(fullPath))
        {
            fullPath = Path.Combine(AppContext.BaseDirectory, fullPath);
        }

        return fullPath;
    }
}
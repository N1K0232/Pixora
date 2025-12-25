using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MimeMapping;

namespace Pixora.StorageProviders.Azure;

internal class AzureStorageProvider(AzureStorageOptions options) : IStorageProvider
{
    private readonly BlobServiceClient blobServiceClient = new BlobServiceClient(options.ConnectionString);

    public async Task SaveAsync(Stream stream, string path, bool overwrite = false, CancellationToken cancellationToken = default)
    {
        var blobClient = await GetBlobClientAsync(path, true, cancellationToken).ConfigureAwait(false);

        if (!overwrite)
        {
            var blobExists = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
            if (blobExists)
            {
                throw new IOException($"The file {path} already exists");
            }
        }

        stream.Position = 0;
        await blobClient.UploadAsync(stream, new BlobHttpHeaders { ContentType = MimeUtility.GetMimeMapping(path) }, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<Stream?> ReadAsStreamAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = await GetBlobClientAsync(path, false, cancellationToken).ConfigureAwait(false);
        var blobExists = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);

        if (!blobExists)
        {
            return null;
        }

        var stream = await blobClient.OpenReadAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        return stream;
    }

    public async Task<bool> ExistsAsync(string path, CancellationToken cancellationToken = default)
    {
        var blobClient = await GetBlobClientAsync(path, false, cancellationToken).ConfigureAwait(false);
        var blobExists = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);

        return blobExists;
    }

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        var (containerName, blobName) = ExtractContainerBlobName(path);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        await blobContainerClient.DeleteBlobIfExistsAsync(blobName, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    private async Task<BlobClient> GetBlobClientAsync(string path, bool createIfNotExists = false, CancellationToken cancellationToken = default)
    {
        var (containerName, blobName) = ExtractContainerBlobName(path);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(containerName);

        if (createIfNotExists)
        {
            await blobContainerClient.CreateIfNotExistsAsync(PublicAccessType.None, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        var blobClient = blobContainerClient.GetBlobClient(blobName);
        return blobClient;
    }

    private (string ContainerName, string BlobName) ExtractContainerBlobName(string? path)
    {
        var fixedPath = path?.Replace(@"\", "/") ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(options.ContainerName))
        {
            return (options.ContainerName, fixedPath);
        }

        var root = Path.GetPathRoot(fixedPath) ?? string.Empty;
        var fileName = fixedPath[root.Length..];
        var parts = fileName.Split('/');

        var containerName = parts.First().ToLowerInvariant();
        var blobName = string.Join('/', parts.Skip(1));

        return (containerName, blobName);
    }
}
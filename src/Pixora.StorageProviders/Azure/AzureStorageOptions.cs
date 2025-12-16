namespace Pixora.StorageProviders.Azure;

public class AzureStorageOptions
{
    public string ConnectionString { get; set; } = null!;

    public string? ContainerName { get; set; }
}
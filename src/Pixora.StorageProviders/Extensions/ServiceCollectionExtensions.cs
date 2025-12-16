using Microsoft.Extensions.DependencyInjection;
using Pixora.StorageProviders.Azure;
using Pixora.StorageProviders.FileSystem;

namespace Pixora.StorageProviders.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureStorage(this IServiceCollection services, Action<AzureStorageOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(setupAction, nameof(setupAction));

        var options = new AzureStorageOptions();
        setupAction.Invoke(options);

        services.AddSingleton(options);
        services.AddScoped<IStorageProvider, AzureStorageProvider>();

        return services;
    }

    public static IServiceCollection AddFileSystemStorage(this IServiceCollection services, Action<FileSystemStorageOptions> setupAction)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(setupAction, nameof(setupAction));

        var options = new FileSystemStorageOptions();
        setupAction.Invoke(options);

        services.AddSingleton(options);
        services.AddScoped<IStorageProvider, FileSystemStorageProvider>();

        return services;
    }
}
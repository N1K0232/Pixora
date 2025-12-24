using Microsoft.Extensions.Caching.Memory;
using Pixora.BusinessLayer.Generators.Interfaces;
using Pixora.Shared.Notifications;
using Pixora.StorageProviders;
using SimpleTransit;

namespace Pixora.BusinessLayer.Publishers;

public class ImageNotificationHandler(IMemoryCache cache, IStorageProvider storageProvider) : INotificationHandler<ImageCreated>, INotificationHandler<ImageDeleted>
{
    public async Task HandleAsync(ImageCreated message, CancellationToken cancellationToken)
    {
        await storageProvider.SaveAsync(message.Stream, message.Path, false, cancellationToken);
        cache.Remove("Images");
    }

    public async Task HandleAsync(ImageDeleted message, CancellationToken cancellationToken)
    {
        await storageProvider.DeleteAsync(message.Path, cancellationToken);

        cache.Remove("Images");
        cache.Remove($"Images-{message.Id}");
    }
}
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using OperationResults;
using Pixora.Authentication.Extensions;
using Pixora.BusinessLayer.Generators.Interfaces;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.DataAccessLayer;
using Pixora.Shared.Models;
using Pixora.Shared.Notifications;
using Pixora.StorageProviders;
using SimpleTransit;
using Entities = Pixora.DataAccessLayer.Entities;

namespace Pixora.BusinessLayer.Services;

public class ImageService(IApplicationDbContext dbContext, IStorageProvider storageProvider, IMemoryCache cache, IPathGenerator pathGenerator, IHttpContextAccessor httpContextAccessor, INotificationPublisher notificationPublisher) : IImageService
{
    public async Task<Result<Image>> SaveAsync(IFormFile file, string? description, string[]? tags, CancellationToken cancellationToken)
    {
        var httpContext = httpContextAccessor.HttpContext!;

        try
        {
            var path = pathGenerator.CreatePath(file.FileName);
            var image = new Entities.Image
            {
                UserId = httpContext.User.GetId(),
                FileName = file.FileName,
                Path = path,
                Length = file.Length,
                ContentType = file.ContentType,
                Description = description,
                Tags = tags!
            };

            await dbContext.CreateAsync(image, cancellationToken);
            await dbContext.SaveAsync(cancellationToken);

            using var stream = file.OpenReadStream();
            await notificationPublisher.NotifyAsync(new ImageCreated(stream, path), cancellationToken);

            var createdImage = new Image
            {
                Id = image.Id,
                UserName = httpContext.User.Identity?.Name,
                FileName = file.FileName,
                Path = path,
                Length = image.Length,
                ContentType = image.ContentType,
                Description = description,
                Tags = tags!
            };

            return createdImage;
        }
        catch (DbUpdateException ex)
        {
            return Result.Fail(FailureReasons.ClientError, "Upload failed", ex.Message);
        }
        catch (SqlException ex)
        {
            return Result.Fail(FailureReasons.ClientError, "Upload failed", ex.Message);
        }
        catch (IOException ex)
        {
            return Result.Fail(FailureReasons.ClientError, "Upload failed", ex.Message);
        }
    }

    public async Task<Result<Image>> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var image = cache.Get<Image>($"Images-{id}");
        if (image is null)
        {
            var dbImage = await dbContext.GetData<Entities.Image>()
                .Include(i => i.User)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

            if (dbImage is null)
            {
                return Result.Fail(FailureReasons.ItemNotFound, "No image found", $"No image found with id {id}");
            }

            if (!dbImage.IsPublished)
            {
                return Result.Fail(FailureReasons.Forbidden, "Content not available", "Content not available");
            }

            image = new Image
            {
                Id = id,
                UserName = dbImage.User.UserName,
                FileName = dbImage.FileName,
                Path = dbImage.Path,
                Length = dbImage.Length,
                ContentType = dbImage.ContentType
            };
        }

        cache.Set($"Images-{id}", image, TimeSpan.FromHours(1));
        return image;
    }

    public async Task<Result<IEnumerable<Image>>> GetListAsync(CancellationToken cancellationToken)
    {
        var images = await cache.GetOrCreateAsync("images", async (entry) =>
        {
            var images = await dbContext.GetData<Entities.Image>()
                .Include(i => i.User)
                .Where(i => i.IsPublished)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new Image
                {
                    Id = i.Id,
                    UserName = i.User.UserName,
                    FileName = i.FileName,
                    Path = i.Path,
                    Length = i.Length,
                    ContentType = i.ContentType,
                    Description = i.Description,
                    Tags = i.Tags
                })
                .ToListAsync(cancellationToken);

            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return images;
        });

        return images ?? [];
    }

    public async Task<Result<StreamFileContent>> DownloadAsync(Guid id, CancellationToken cancellationToken)
    {
        var image = await dbContext.GetAsync<Entities.Image>(id, cancellationToken);
        if (image is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No image found", $"No image found with id {id}");
        }

        if (!image.IsPublished)
        {
            return Result.Fail(FailureReasons.Forbidden, "Content not available", "Content not available");
        }

        var stream = await storageProvider.ReadAsStreamAsync(image.Path, cancellationToken);
        if (stream is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No image found", "No image found");
        }

        var streamFileContent = new StreamFileContent(stream, image.ContentType);
        return streamFileContent;
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var image = await dbContext.GetAsync<Entities.Image>(id, cancellationToken);
        if (image is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No image found", $"No image found with id {id}");
        }

        await dbContext.DeleteAsync(image, cancellationToken);
        await dbContext.SaveAsync(cancellationToken);

        await notificationPublisher.NotifyAsync(new ImageDeleted(id, image.Path), cancellationToken);
        return Result.Ok();
    }
}
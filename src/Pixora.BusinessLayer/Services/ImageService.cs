using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OperationResults;
using Pixora.Authentication.Extensions;
using Pixora.BusinessLayer.Internal;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.DataAccessLayer;
using Pixora.Shared.Models;
using Pixora.StorageProviders;
using Entities = Pixora.DataAccessLayer.Entities;

namespace Pixora.BusinessLayer.Services;

public class ImageService(IApplicationDbContext dbContext, IStorageProvider storageProvider, IHttpContextAccessor httpContextAccessor) : IImageService
{
    public async Task<Result<Image>> SaveAsync(IFormFile file, string? description, string? tags, CancellationToken cancellationToken)
    {
        try
        {
            using var stream = file.OpenReadStream();
            var path = PathGenerator.CreatePath(file.FileName);

            await storageProvider.SaveAsync(stream, path, false, cancellationToken);

            var image = new Entities.Image
            {
                UserId = httpContextAccessor.HttpContext!.User.GetId(),
                FileName = file.FileName,
                Path = path,
                Length = stream.Length,
                ContentType = file.ContentType,
                Description = description,
                Tags = tags?.Split(';') ?? []
            };

            await dbContext.CreateAsync(image, cancellationToken);
            await dbContext.SaveAsync(cancellationToken);

            var createdImage = new Image
            {
                Id = image.Id,
                FileName = file.FileName,
                Path = path,
                Length = image.Length,
                ContentType = image.ContentType
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
        var dbImage = await dbContext.GetAsync<Entities.Image>(id, cancellationToken);
        if (dbImage is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No image found", $"No image found with id {id}");
        }

        if (!dbImage.IsPublished)
        {
            return Result.Fail(FailureReasons.Forbidden, "Content not available", "Content not available");
        }

        var image = new Image
        {
            Id = id,
            FileName = dbImage.FileName,
            Path = dbImage.Path,
            Length = dbImage.Length,
            ContentType = dbImage.ContentType
        };

        return image;
    }

    public async Task<Result<IEnumerable<Image>>> GetListAsync(CancellationToken cancellationToken)
    {
        var images = await dbContext.GetData<Entities.Image>()
            .Where(i => i.IsPublished)
            .Select(i => new Image
            {
                Id = i.Id,
                FileName = i.FileName,
                Path = i.Path,
                Length = i.Length,
                ContentType = i.ContentType,
                Description = i.Description,
                Tags = i.Tags
            })
            .ToListAsync(cancellationToken);

        return images;
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

        await storageProvider.DeleteAsync(image.Path, cancellationToken);
        await dbContext.DeleteAsync(image, cancellationToken);

        await dbContext.SaveAsync(cancellationToken);
        return Result.Ok();
    }
}
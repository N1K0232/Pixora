using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MimeMapping;
using OperationResults;
using Pixora.Authentication.Entities;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.StorageProviders;

namespace Pixora.BusinessLayer.Services;

public class UserService(UserManager<ApplicationUser> userManager, IStorageProvider storageProvider) : IUserService
{
    public async Task<Result> UploadProfilePhotoAsync(IFormFile file, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        using var stream = file.OpenReadStream();
        var user = await userManager.GetUserAsync(principal);

        if (user is null)
        {
            return Result.Fail(FailureReasons.Unauthorized);
        }

        var path = $"users\\{user.Id}\\{file.FileName}";
        await storageProvider.SaveAsync(stream, path, false, cancellationToken);

        user.ProfilePhoto = path;
        await userManager.UpdateAsync(user);

        return Result.Ok();
    }

    public async Task<Result<StreamFileContent>> GetProfilePhotoAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return Result.Fail(FailureReasons.Unauthorized);
        }

        if (string.IsNullOrWhiteSpace(user.ProfilePhoto))
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No image was uploaded", "No image was uploaded");
        }

        var stream = await storageProvider.ReadAsStreamAsync(user.ProfilePhoto, cancellationToken);
        if (stream is null)
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No image was uploaded", "No image was uploaded");
        }

        var streamFileContent = new StreamFileContent(stream, MimeUtility.GetMimeMapping(user.ProfilePhoto));
        return streamFileContent;
    }

    public async Task<Result> DeleteProfilePhotoAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return Result.Fail(FailureReasons.Unauthorized);
        }

        if (string.IsNullOrWhiteSpace(user.ProfilePhoto))
        {
            return Result.Fail(FailureReasons.ItemNotFound, "No image was uploaded", "No image was uploaded");
        }

        await storageProvider.DeleteAsync(user.ProfilePhoto, cancellationToken);
        user.ProfilePhoto = null;

        await userManager.UpdateAsync(user);
        return Result.Ok();
    }
}
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using MimeMapping;
using OperationResults;
using Pixora.Authentication.Entities;
using Pixora.Authentication.Extensions;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.Shared.Models;
using Pixora.Shared.Notifications;
using Pixora.StorageProviders;
using SimpleTransit;
using TinyHelpers.Extensions;

namespace Pixora.BusinessLayer.Services;

public class UserService(UserManager<ApplicationUser> userManager, IStorageProvider storageProvider) : IUserService
{
    public Task<Result<User>> GetAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = new User
        {
            Id = principal.GetId(),
            FirstName = principal.GetFirstName(),
            LastName = principal.GetLastName(),
            Email = principal.GetEmail(),
            UserName = principal.Identity?.Name ?? string.Empty,
            Roles = principal.GetUserRoles()
        };

        var result = Result<User>.Ok(user);
        return Task.FromResult(result);
    }

    public async Task<Result> UploadProfilePhotoAsync(IFormFile file, ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
        {
            return Result.Fail(FailureReasons.Unauthorized);
        }

        if (user.ProfilePhoto.HasValue() && await storageProvider.ExistsAsync(user.ProfilePhoto, cancellationToken))
        {
            await storageProvider.DeleteAsync(user.ProfilePhoto, cancellationToken);
        }

        using var stream = file.OpenReadStream();
        var path = $"\\users\\{user.Id}\\{file.FileName}";
        user.ProfilePhoto = path;

        await userManager.UpdateAsync(user);
        await storageProvider.SaveAsync(stream, path, false, cancellationToken);
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
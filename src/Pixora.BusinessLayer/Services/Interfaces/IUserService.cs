using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using OperationResults;

namespace Pixora.BusinessLayer.Services.Interfaces;

public interface IUserService
{
    Task<Result> UploadProfilePhotoAsync(IFormFile file, ClaimsPrincipal principal, CancellationToken cancellationToken);

    Task<Result<StreamFileContent>> GetProfilePhotoAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);

    Task<Result> DeleteProfilePhotoAsync(ClaimsPrincipal principal, CancellationToken cancellationToken);
}
using Microsoft.AspNetCore.Http;
using OperationResults;
using Pixora.Shared.Models;

namespace Pixora.BusinessLayer.Services.Interfaces;

public interface IImageService
{
    Task<Result<Image>> SaveAsync(IFormFile file, string? description, string[]? tags, CancellationToken cancellationToken);

    Task<Result<Image>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<IEnumerable<Image>>> GetListAsync(CancellationToken cancellationToken);

    Task<Result<StreamFileContent>> DownloadAsync(Guid id, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
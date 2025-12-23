using OperationResults;
using Pixora.Shared.Models;
using Pixora.Shared.Models.Requests;

namespace Pixora.BusinessLayer.Services.Interfaces;

public interface IPostService
{
    Task<Result<Post>> CreateAsync(SavePostRequest request, CancellationToken cancellationToken);

    Task<Result<Post>> GetAsync(Guid id, CancellationToken cancellationToken);

    Task<Result<IEnumerable<Post>>> GetListAsync(CancellationToken cancellationToken);

    Task<Result> UpdateAsync(Guid id, SavePostRequest request, CancellationToken cancellationToken);

    Task<Result> DeleteAsync(Guid id, CancellationToken cancellationToken);
}
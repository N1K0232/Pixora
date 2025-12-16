using OperationResults;
using Pixora.Shared.Models;
using Pixora.Shared.Models.Requests;

namespace Pixora.BusinessLayer.Services.Interfaces;

public interface IIdentityService
{
    Task<Result> ConfirmEmailAsync(string secret, string token, CancellationToken cancellationToken);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);

    Task<Result<AuthResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken);

    Task<Result> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
}
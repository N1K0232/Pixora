namespace Pixora.BusinessLayer.Services.Interfaces;

public interface IIpBanService
{
    Task BanAsync(string ip, string? reason, TimeSpan duration, CancellationToken cancellationToken);

    Task UnbanAsync(Guid id, CancellationToken cancellationToken);

    Task<bool> IsBannedAsync(string ipAddress, CancellationToken cancellationToken);
}
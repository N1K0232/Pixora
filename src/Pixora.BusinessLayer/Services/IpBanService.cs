using Microsoft.EntityFrameworkCore;
using Pixora.Authentication;
using Pixora.Authentication.Entities;
using Pixora.BusinessLayer.Services.Interfaces;

namespace Pixora.BusinessLayer.Services;

public class IpBanService(AuthenticationDbContext authenticationDbContext, TimeProvider timeProvider) : IIpBanService
{
    public async Task BanAsync(string ip, string? reason, TimeSpan duration, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var ban = new IpAddressBan
        {
            Value = ip,
            Reason = reason,
            BannedAt = now,
            ExpiresAt = now.Add(duration)
        };

        await authenticationDbContext.IpAddressBans.AddAsync(ban, cancellationToken);
        await authenticationDbContext.SaveChangesAsync(true, cancellationToken);
    }

    public async Task UnbanAsync(Guid id, CancellationToken cancellationToken)
    {
        var ban = await authenticationDbContext.IpAddressBans.FindAsync([id], cancellationToken);
        if (ban is not null)
        {
            authenticationDbContext.IpAddressBans.Remove(ban);
            await authenticationDbContext.SaveChangesAsync(true, cancellationToken);
        }
    }

    public async Task<bool> IsBannedAsync(string ipAddress, CancellationToken cancellationToken)
    {
        var ip = await authenticationDbContext.IpAddressBans.FirstOrDefaultAsync(a => a.Value == ipAddress, cancellationToken);
        return ip is not null && (ip.ExpiresAt is null || ip.ExpiresAt > timeProvider.GetUtcNow());
    }
}
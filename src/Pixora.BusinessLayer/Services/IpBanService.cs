using Microsoft.EntityFrameworkCore;
using Pixora.Authentication.Entities;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.DataAccessLayer;

namespace Pixora.BusinessLayer.Services;

public class IpBanService(ApplicationDbContext applicationDbContext, TimeProvider timeProvider) : IIpBanService
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

        await applicationDbContext.IpAddressBans.AddAsync(ban, cancellationToken);
        await applicationDbContext.SaveChangesAsync(true, cancellationToken);
    }

    public async Task UnbanAsync(Guid id, CancellationToken cancellationToken)
    {
        var ban = await applicationDbContext.IpAddressBans.FindAsync([id], cancellationToken);
        if (ban is not null)
        {
            applicationDbContext.IpAddressBans.Remove(ban);
            await applicationDbContext.SaveChangesAsync(true, cancellationToken);
        }
    }

    public async Task<bool> IsBannedAsync(string ipAddress, CancellationToken cancellationToken)
    {
        var ip = await applicationDbContext.IpAddressBans.FirstOrDefaultAsync(a => a.Value == ipAddress, cancellationToken);
        return ip is not null && (ip.ExpiresAt is null || ip.ExpiresAt > timeProvider.GetUtcNow());
    }
}
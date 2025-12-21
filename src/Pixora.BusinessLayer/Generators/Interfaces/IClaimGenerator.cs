using System.Security.Claims;
using Pixora.Authentication.Entities;

namespace Pixora.BusinessLayer.Generators.Interfaces;

public interface IClaimGenerator
{
    Task<IList<Claim>> GetOrCreateAsync(ApplicationUser user, CancellationToken cancellationToken = default);
}
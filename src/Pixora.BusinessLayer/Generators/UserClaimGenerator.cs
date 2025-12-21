using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.JsonWebTokens;
using Pixora.Authentication.Entities;
using Pixora.BusinessLayer.Generators.Interfaces;
using TinyHelpers.Extensions;

namespace Pixora.BusinessLayer.Generators;

public class UserClaimGenerator(UserManager<ApplicationUser> userManager) : IClaimGenerator
{
    public async Task<IList<Claim>> GetOrCreateAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var claims = await userManager.GetClaimsAsync(user);
        if (claims is null || claims.Count == 0)
        {
            claims =
            [
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(JwtRegisteredClaimNames.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(JwtRegisteredClaimNames.FamilyName, user.LastName),
                new Claim(ClaimTypes.Email, user.Email!)
            ];

            if (user.PhoneNumber.HasValue() && user.PhoneNumberConfirmed)
            {
                claims.Add(new Claim(ClaimTypes.MobilePhone, user.PhoneNumber));
            }

            await userManager.AddClaimsAsync(user, claims);
        }

        var userRoles = await userManager.GetRolesAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        var hostName = Dns.GetHostName();
        var addresses = await Dns.GetHostAddressesAsync(hostName, cancellationToken);

        claims.Add(new Claim(ClaimTypes.SerialNumber, user.SecurityStamp ?? string.Empty));
        claims.Add(new Claim(ClaimTypes.Dns, hostName));

        return claims.Union(addresses.Select(address => new Claim(ClaimTypes.Dns, address.ToString())))
            .Union(userRoles.Select(role => new Claim(ClaimTypes.Role, role)))
            .ToList();
    }
}
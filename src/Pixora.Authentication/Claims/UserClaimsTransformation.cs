using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Pixora.Authentication.Entities;

namespace Pixora.Authentication.Claims;

public class UserClaimsTransformation(UserManager<ApplicationUser> userManager) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is not null)
        {
            var claims = await userManager.GetClaimsAsync(user);
            if (claims.Count == 0)
            {
                await userManager.AddClaimsAsync(user, principal.Claims);
            }
        }

        return principal;
    }
}
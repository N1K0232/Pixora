using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Pixora.Authentication.Entities;
using SimpleAuthentication;

namespace Pixora.Requirements;

public class UserActiveHandler(UserManager<ApplicationUser> userManager) : AuthorizationHandler<UserActiveRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, UserActiveRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated ?? false)
        {
            var user = await userManager.GetUserAsync(context.User);
            var securityStamp = context.User.GetClaimValue(ClaimTypes.SerialNumber);

            if (user is not null && !await userManager.IsLockedOutAsync(user) && securityStamp == user.SecurityStamp)
            {
                context.Succeed(requirement);
            }
        }
    }
}
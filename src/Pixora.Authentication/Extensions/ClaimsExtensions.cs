using System.Security.Claims;
using SimpleAuthentication;

namespace Pixora.Authentication.Extensions;

public static class ClaimsExtensions
{
    public static Guid GetId(this ClaimsPrincipal user)
    {
        var idString = user.GetClaimValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(idString, out var id))
        {
            return id;
        }

        return Guid.Empty;
    }

    public static string GetFirstName(this ClaimsPrincipal user)
        => user.GetClaimValue(ClaimTypes.GivenName) ?? string.Empty;

    public static string GetLastName(this ClaimsPrincipal user)
        => user.GetClaimValue(ClaimTypes.Surname) ?? string.Empty;

    public static string GetEmail(this ClaimsPrincipal user)
        => user.GetClaimValue(ClaimTypes.Email) ?? string.Empty;

    public static IEnumerable<string?> GetUserRoles(this ClaimsPrincipal user)
        => user.GetClaimValues<string>(ClaimTypes.Role);
}
using Microsoft.AspNetCore.Identity;

namespace Pixora.Authentication.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public bool EnableNotifications { get; set; }

    public string? ProfilePhoto { get; set; }

    public string? RefreshToken { get; set; }

    public DateTimeOffset? RefreshTokenExpirationDate { get; set; }

    public virtual ICollection<ApplicationUserRole> UserRoles { get; set; } = [];
}
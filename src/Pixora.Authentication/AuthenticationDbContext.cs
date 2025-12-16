using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Pixora.Authentication.Entities;

namespace Pixora.Authentication;

public abstract class AuthenticationDbContext(DbContextOptions options) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid, IdentityUserClaim<Guid>, ApplicationUserRole, IdentityUserLogin<Guid>, IdentityRoleClaim<Guid>, IdentityUserToken<Guid>, IdentityUserPasskey<Guid>>(options), IDataProtectionKeyContext
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.FirstName).HasMaxLength(256).IsRequired();
            b.Property(u => u.LastName).HasMaxLength(256).IsRequired();

            b.Property(u => u.ProfilePhoto).HasMaxLength(512).IsRequired(false);
            b.Property(u => u.RefreshToken).HasMaxLength(2048).IsRequired(false);
        });

        builder.Entity<ApplicationUserRole>(b =>
        {
            b.HasKey(ur => new { ur.UserId, ur.RoleId });

            b.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();

            b.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });

        builder.Entity<DataProtectionKey>(b =>
        {
            b.HasKey(k => k.Id);
            b.Property(k => k.Id).ValueGeneratedOnAdd();

            b.Property(k => k.FriendlyName).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
            b.Property(k => k.Xml).HasColumnType("NVARCHAR(MAX)").IsRequired(false);
        });
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pixora.DataAccessLayer.Configurations.Common;
using Pixora.DataAccessLayer.Entities;

namespace Pixora.DataAccessLayer.Configurations;

internal class PostConfiguration : BaseEntityConfiguration<Post>
{
    public override void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.Property(p => p.Title).HasMaxLength(255).IsRequired();
        builder.Property(p => p.Content).HasColumnType("NVARCHAR(MAX)").IsRequired();

        builder.Property(p => p.IsPublished).HasDefaultValueSql("((1))").IsRequired();
        builder.Property(p => p.IsEdited).HasDefaultValueSql("((0))").IsRequired();

        builder.Property(p => p.Visibility).HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(p => p.UserId).IsRequired();

        builder.ToTable("Posts");
        base.Configure(builder);
    }
}
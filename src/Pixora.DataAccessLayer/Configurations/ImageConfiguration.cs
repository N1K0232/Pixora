using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pixora.DataAccessLayer.Configurations.Common;
using Pixora.DataAccessLayer.Entities;
using TinyHelpers.EntityFrameworkCore.Extensions;

namespace Pixora.DataAccessLayer.Configurations;

internal class ImageConfiguration : BaseEntityConfiguration<Image>
{
    public override void Configure(EntityTypeBuilder<Image> builder)
    {
        builder.Property(i => i.FileName).HasMaxLength(255).IsRequired();
        builder.Property(i => i.Path).HasMaxLength(512).IsRequired();

        builder.Property(i => i.ContentType).HasMaxLength(50).IsRequired();
        builder.Property(i => i.UserId).IsRequired();

        builder.Property(i => i.Description).HasMaxLength(4000).IsRequired(false);
        builder.Property(i => i.Tags).HasArrayConversion().HasColumnType("NVARCHAR(MAX)").IsRequired(false);

        builder.Property(i => i.IsPublished).HasDefaultValueSql("((1))").IsRequired();
        builder.Property(i => i.PublishedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()").IsRequired();

        builder.HasIndex(i => i.FileName)
            .IsClustered(false)
            .IsUnique();

        builder.HasIndex(i => i.Path)
            .IsClustered(false)
            .IsUnique();

        builder.ToTable("Images");
        base.Configure(builder);
    }
}
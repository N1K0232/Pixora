using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pixora.DataAccessLayer.Entities;

namespace Pixora.DataAccessLayer.Configurations;

internal class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.ToTable("ChatMessages");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

        builder.Property(m => m.Content).IsRequired().HasMaxLength(4000);
        builder.Property(m => m.SentAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");

        builder.HasIndex(m => m.ConversationId, "IX_ChatMessages_ConversationId").IsClustered(false).IsDescending(false);
        builder.HasIndex(m => m.SentAt, "IX_ChatMessages_SentAt").IsClustered(false).IsDescending(false);

        builder.HasOne(m => m.Conversation)
            .WithMany(m => m.Messages)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
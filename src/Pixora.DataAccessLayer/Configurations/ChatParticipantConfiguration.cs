using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Pixora.DataAccessLayer.Entities;

namespace Pixora.DataAccessLayer.Configurations;

internal class ChatParticipantConfiguration : IEntityTypeConfiguration<ChatParticipant>
{
    public void Configure(EntityTypeBuilder<ChatParticipant> builder)
    {
        builder.ToTable("ChatParticipants");
        builder.HasKey(p => new { p.ConversationId, p.UserId });
        builder.Property(p => p.JoinedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");

        builder.HasIndex(p => p.UserId, "IX_ChatParticipants_UserId")
            .IsClustered(false)
            .IsDescending(false);

        builder.HasOne(p => p.Conversation)
            .WithMany(p => p.Participants)
            .HasForeignKey(p => p.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
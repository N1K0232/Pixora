using Pixora.Authentication.Entities;

namespace Pixora.DataAccessLayer.Entities;

public class ChatParticipant
{
    public Guid ConversationId { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset JoinedAt { get; set; }

    public virtual ChatConversation Conversation { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}
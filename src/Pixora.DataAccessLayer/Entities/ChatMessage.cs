using Pixora.Authentication.Entities;

namespace Pixora.DataAccessLayer.Entities;

public class ChatMessage
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }

    public Guid SenderId { get; set; }

    public string Content { get; set; } = null!;

    public DateTimeOffset SentAt { get; set; }

    public virtual ChatConversation Conversation { get; set; } = null!;

    public virtual ApplicationUser User { get; set; } = null!;
}
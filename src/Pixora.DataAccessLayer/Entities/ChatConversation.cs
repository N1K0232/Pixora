namespace Pixora.DataAccessLayer.Entities;

public class ChatConversation
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public virtual ICollection<ChatParticipant> Participants { get; set; } = [];

    public virtual ICollection<ChatMessage> Messages { get; set; } = [];
}
namespace Pixora.Shared.Models;

public record class ChatMessage(Guid Id, Guid ConversationId, string Content, IEnumerable<Guid>? Participants);
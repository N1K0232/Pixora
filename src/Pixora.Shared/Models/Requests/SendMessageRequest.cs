namespace Pixora.Shared.Models.Requests;

public record class SendMessageRequest(Guid SenderId, Guid ConversationId, string Content);
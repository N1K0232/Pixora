namespace Pixora.Shared.Models;

public record class WsMessage(string Type, Guid? ConversationId, Guid? MessageId, string? Content, int? Take);
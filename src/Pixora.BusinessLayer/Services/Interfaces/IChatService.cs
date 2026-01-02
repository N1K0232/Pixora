using Pixora.Shared.Models;
using Pixora.Shared.Models.Requests;

namespace Pixora.BusinessLayer.Services.Interfaces;

public interface IChatService
{
    Task<ChatMessage> SendAsync(SendMessageRequest request, CancellationToken cancellationToken);

    Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId, int? take, CancellationToken cancellationToken);
}
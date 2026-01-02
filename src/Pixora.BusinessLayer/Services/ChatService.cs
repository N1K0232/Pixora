using Microsoft.EntityFrameworkCore;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.DataAccessLayer;
using Pixora.Shared.Models;
using Pixora.Shared.Models.Requests;
using Entities = Pixora.DataAccessLayer.Entities;

namespace Pixora.BusinessLayer.Services;

public class ChatService(IApplicationDbContext applicationDbContext) : IChatService
{
    public async Task<ChatConversation> GetOrCreateAsync(Guid userA, Guid userB, CancellationToken cancellationToken)
    {
        var dbConversation = await applicationDbContext.GetData<Entities.ChatConversation>()
            .Where(c => c.Participants.Count == 2 && c.Participants.Any(p => p.UserId == userA) && c.Participants.Any(p => p.UserId == userB))
            .FirstOrDefaultAsync(cancellationToken);

        if (dbConversation is null)
        {
            dbConversation = new Entities.ChatConversation
            {
                Id = Guid.CreateVersion7()
            };

            dbConversation.Participants.Add(new Entities.ChatParticipant { ConversationId = dbConversation.Id, UserId = userA });
            dbConversation.Participants.Add(new Entities.ChatParticipant { ConversationId = dbConversation.Id, UserId = userB });

            await applicationDbContext.CreateAsync(dbConversation, cancellationToken);
            await applicationDbContext.SaveAsync(cancellationToken);
        }

        var conversation = new ChatConversation(dbConversation.Id);
        return conversation;
    }

    public async Task<ChatMessage> SendAsync(SendMessageRequest request, CancellationToken cancellationToken)
    {
        var conversation = await applicationDbContext.GetData<Entities.ChatConversation>()
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == request.ConversationId, cancellationToken);

        if (conversation is null)
        {
            throw new InvalidOperationException($"No conversation was found with id {request.ConversationId}");
        }

        if (conversation.Participants.Count == 0)
        {
            throw new UnauthorizedAccessException("No participant found");
        }

        var dbMessage = new Entities.ChatMessage
        {
            ConversationId = request.ConversationId,
            SenderId = request.SenderId,
            Content = request.Content.Trim()
        };

        await applicationDbContext.CreateAsync(dbMessage, cancellationToken);
        await applicationDbContext.SaveAsync(cancellationToken);

        var message = new ChatMessage(dbMessage.Id, request.ConversationId, request.Content, conversation.Participants.Select(p => p.UserId));
        return message;
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(Guid conversationId, int? take, CancellationToken cancellationToken)
    {
        var messages = await applicationDbContext.GetData<Entities.ChatMessage>()
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.SentAt)
            .Take(take.GetValueOrDefault(50))
            .Select(m => new ChatMessage(m.Id, m.ConversationId, m.Content, null))
            .ToListAsync(cancellationToken);

        return messages;
    }
}
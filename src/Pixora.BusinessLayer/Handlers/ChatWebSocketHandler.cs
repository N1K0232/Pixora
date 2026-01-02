using System.Net.Http;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Pixora.Authentication.Extensions;
using Pixora.BusinessLayer.Connections.Interfaces;
using Pixora.BusinessLayer.Handlers.Interfaces;
using Pixora.BusinessLayer.Services.Interfaces;
using Pixora.Shared.Models;
using Pixora.Shared.Models.Requests;
using SimpleAuthentication.JwtBearer;
using TinyHelpers.Extensions;

namespace Pixora.BusinessLayer.Handlers;

public class ChatWebSocketHandler(IChatService chatService, IUserConnectionManager connectionManager, IJwtBearerService jwtBearerService) : IChatWebSocketHandler
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task HandleSocketAsync(WebSocket socket, HttpContext httpContext, CancellationToken cancellationToken)
    {
        var user = await AuthenticateAsync(httpContext);
        connectionManager.Add(user.GetId(), socket);

        try
        {
            while (socket.State is WebSocketState.Open)
            {
                var message = await ReceiveAsync(socket, cancellationToken);
                await HandleMessageAsync(user, message, socket, cancellationToken);
            }
        }
        finally
        {
            connectionManager.Remove(user.GetId(), socket);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed", cancellationToken);
        }
    }

    private async Task<ClaimsPrincipal> AuthenticateAsync(HttpContext httpContext)
    {
        var accessToken = GetAccessToken(httpContext);
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new UnauthorizedAccessException("Missing access token");
        }

        return await jwtBearerService.ValidateTokenAsync(accessToken, true);
    }

    private static string? GetAccessToken(HttpContext httpContext)
    {
        string? token = null;

        if (httpContext.Request.Query.TryGetValue("access_token", out var qsToken))
        {
            token = qsToken.ToString();
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            string? authenticationHeader = httpContext.Request.Headers[HeaderNames.Authorization];

            if (authenticationHeader.HasValue() && authenticationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authenticationHeader["Bearer ".Length..];
            }
        }

        return token;
    }

    private static async Task<WsMessage> ReceiveAsync(WebSocket socket, CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        using var stream = new MemoryStream();

        while (true)
        {
            var result = await socket.ReceiveAsync(buffer, cancellationToken);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                throw new WebSocketException("Socket closed by client");
            }

            await stream.WriteAsync(buffer, 0, result.Count, cancellationToken);

            if (result.EndOfMessage)
            {
                break;
            }
        }

        var json = Encoding.UTF8.GetString(stream.ToArray());
        var message = JsonSerializer.Deserialize<WsMessage>(json, jsonSerializerOptions) ?? throw new InvalidOperationException("Invalid WebSocket message");

        return message;
    }

    private async Task SendAsync(Guid userId, WsMessage message, CancellationToken cancellationToken)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(message);
        var sockets = connectionManager.GetConnections(userId);

        foreach (var socket in sockets)
        {
            if (socket.State is not WebSocketState.Open)
            {
                continue;
            }

            await socket.SendAsync(payload, WebSocketMessageType.Text, true, cancellationToken);
        }
    }

    private async Task HandleMessageAsync(ClaimsPrincipal user, WsMessage message, WebSocket socket, CancellationToken cancellationToken)
    {
        switch (message.Type)
        {
            case "send":
                await HandleSendAsync(user, message, cancellationToken);
                break;
            case "history":
                await HandleHistoryAsync(user, message, socket, cancellationToken);
                break;
        }
    }

    private async Task HandleSendAsync(ClaimsPrincipal user, WsMessage message, CancellationToken cancellationToken)
    {
        if (message.ConversationId is null || string.IsNullOrWhiteSpace(message.Content))
        {
            return;
        }

        var request = new SendMessageRequest(user.GetId(), message.ConversationId.Value, message.Content);
        var result = await chatService.SendAsync(request, cancellationToken);

        var participants = result.Participants ?? [];
        foreach (var participant in participants)
        {
            await SendAsync(participant, new WsMessage("message", result.ConversationId, result.Id, result.Content, null), cancellationToken);
        }
    }

    private async Task HandleHistoryAsync(ClaimsPrincipal user, WsMessage message, WebSocket socket, CancellationToken cancellationToken)
    {
        var messages = await chatService.GetMessagesAsync(message.ConversationId.GetValueOrDefault(), message.Take, cancellationToken);
        await SendToSocketAsync(socket, new WsMessage("history", message.ConversationId.GetValueOrDefault(), null, JsonSerializer.Serialize(messages), null), cancellationToken);
    }

    private async Task SendToSocketAsync(WebSocket socket, WsMessage message, CancellationToken cancellationToken)
    {
        if (socket.State is not WebSocketState.Open)
        {
            return;
        }

        var json = JsonSerializer.Serialize(message);
        var segment = new ArraySegment<byte>(Encoding.UTF8.GetBytes(json));

        try
        {
            await socket.SendAsync(segment, WebSocketMessageType.Text, endOfMessage: true, cancellationToken);
        }
        catch
        {
        }
    }
}
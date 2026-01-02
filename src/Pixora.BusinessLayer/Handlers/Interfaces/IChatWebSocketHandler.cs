using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;

namespace Pixora.BusinessLayer.Handlers.Interfaces;

public interface IChatWebSocketHandler
{
    Task HandleSocketAsync(WebSocket socket, HttpContext httpContext, CancellationToken cancellationToken);
}
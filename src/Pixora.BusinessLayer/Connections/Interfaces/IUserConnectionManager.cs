using System.Net.WebSockets;

namespace Pixora.BusinessLayer.Connections.Interfaces;

public interface IUserConnectionManager
{
    void Add(Guid userId, WebSocket socket);

    void Remove(Guid userId, WebSocket socket);

    IReadOnlyCollection<WebSocket> GetConnections(Guid userId);
}
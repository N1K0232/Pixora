using System.Collections.Concurrent;
using System.Net.WebSockets;
using Pixora.BusinessLayer.Connections.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Pixora.BusinessLayer.Connections;

public class UserConnectionManager : IUserConnectionManager
{
    private readonly ConcurrentDictionary<Guid, HashSet<WebSocket>> connections = new();

    public void Add(Guid userId, WebSocket socket)
    {
        var sockets = connections.GetOrAdd(userId, _ => []);

        lock (sockets)
        {
            sockets.Add(socket);
        }
    }

    public void Remove(Guid userId, WebSocket socket)
    {
        if (!connections.TryGetValue(userId, out var sockets))
        {
            return;
        }

        lock (sockets)
        {
            sockets.Remove(socket);

            if (sockets.Count == 0)
            {
                connections.TryRemove(userId, out _);
            }
        }
    }

    public IReadOnlyCollection<WebSocket> GetConnections(Guid userId)
    {
        if (connections.TryGetValue(userId, out var sockets))
        {
            lock (sockets)
            {
                return sockets.ToList();
            }
        }

        return [];
    }

    public IReadOnlyDictionary<Guid, IReadOnlyCollection<WebSocket>> GetAllConnections()
    {
        return connections.ToDictionary
        (
            x => x.Key,
            x =>
            {
                lock (x.Value)
                {
                    return (IReadOnlyCollection<WebSocket>)x.Value.ToList();
                }
            }
        );
    }
}
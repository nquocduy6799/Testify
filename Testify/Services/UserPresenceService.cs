using System.Collections.Concurrent;
using System.Linq;
using Testify.Interfaces;

namespace Testify.Services
{
    public class UserPresenceService : IUserPresenceService
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, byte>> _userConnections = new();

        public void AddConnection(string userId, string connectionId)
        {
            _userConnections.AddOrUpdate(
                userId,
                _ => new ConcurrentDictionary<string, byte>(new[] { new KeyValuePair<string, byte>(connectionId, 0) }),
                (_, existing) => { existing.TryAdd(connectionId, 0); return existing; });
        }

        public void RemoveConnection(string userId, string connectionId)
        {
            if (!_userConnections.TryGetValue(userId, out var connections))
                return;
            connections.TryRemove(connectionId, out _);
            if (connections.IsEmpty)
                _userConnections.TryRemove(userId, out _);
        }

        public bool IsOnline(string userId)
        {
            return _userConnections.TryGetValue(userId, out var connections) && !connections.IsEmpty;
        }

        public IReadOnlyList<string> GetOnlineUserIds()
        {
            return _userConnections.Keys.ToList();
        }
    }
}

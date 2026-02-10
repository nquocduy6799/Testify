using System.Collections.Concurrent;
using System.Linq;
using Testify.Interfaces;

namespace Testify.Services
{
    public class UserPresenceService : IUserPresenceService
    {
        private readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();

        public void AddConnection(string userId, string connectionId)
        {
            _userConnections.AddOrUpdate(
                userId,
                new HashSet<string> { connectionId },
                (_, existing) => { existing.Add(connectionId); return existing; });
        }

        public void RemoveConnection(string userId, string connectionId)
        {
            if (!_userConnections.TryGetValue(userId, out var connections))
                return;
            connections.Remove(connectionId);
            if (connections.Count == 0)
                _userConnections.TryRemove(userId, out _);
        }

        public bool IsOnline(string userId)
        {
            return _userConnections.ContainsKey(userId) && _userConnections[userId].Count > 0;
        }

        public IReadOnlyList<string> GetOnlineUserIds()
        {
            return _userConnections.Keys.ToList();
        }
    }
}

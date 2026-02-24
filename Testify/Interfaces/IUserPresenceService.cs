namespace Testify.Interfaces
{
    public interface IUserPresenceService
    {
        void AddConnection(string userId, string connectionId);
        void RemoveConnection(string userId, string connectionId);
        bool IsOnline(string userId);
        IReadOnlyList<string> GetOnlineUserIds();
    }
}

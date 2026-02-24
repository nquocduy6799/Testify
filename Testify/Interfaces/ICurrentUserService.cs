namespace Testify.Interfaces
{
    public interface ICurrentUserRepository
    {
        string? UserId { get; }
        string? UserName { get; }
        string? Email { get; }
        bool IsAuthenticated { get; }
    }
}

using System.Security.Claims;
using Testify.Interfaces;

namespace Testify.Repositories
{
    public class CurrentUserRepository : ICurrentUserRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserRepository(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string? UserId => _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public string? UserName => _httpContextAccessor.HttpContext?.User.Identity?.Name;

        public string? Email => _httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.Email)?.Value;

        public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
    }
}

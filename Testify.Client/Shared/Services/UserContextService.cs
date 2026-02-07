using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using Testify.Client.Interfaces;
using Testify.Client.Models;
using Testify.Shared.DTOs.Projects;
using Testify.Shared.Enums;

namespace Testify.Client.Services;

public class UserContextService
{
    private readonly AuthenticationStateProvider _authStateProvider;
    private readonly IProjectService _projectService;
    private CurrentUserInfo? _cachedUserInfo;

    public UserContextService(
        AuthenticationStateProvider authStateProvider,
        IProjectService projectService)
    {
        _authStateProvider = authStateProvider;
        _projectService = projectService;
    }

    public async Task<CurrentUserInfo?> GetCurrentUserAsync()
    {
        if (_cachedUserInfo != null)
            return _cachedUserInfo;

        var authState = await _authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;

        if (!user.Identity?.IsAuthenticated == true)
            return null;

        _cachedUserInfo = new CurrentUserInfo
        {
            UserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
            UserName = user.Identity.Name ?? "Unknown",
            Email = user.FindFirst(ClaimTypes.Email)?.Value
        };

        return _cachedUserInfo;
    }

    public async Task<ProjectUserContext?> GetProjectUserContextAsync(int projectId)
    {
        var userInfo = await GetCurrentUserAsync();
        if (userInfo == null) return null;

        var projectMembers = await _projectService.GetMembersAsync(projectId);
        var currentMember = projectMembers.FirstOrDefault(pm => pm.UserId == userInfo.UserId);

        return new ProjectUserContext
        {
            UserId = userInfo.UserId,
            UserName = userInfo.UserName,
            Email = userInfo.Email,
            ProjectRole = currentMember?.Role,
            IsPM = currentMember?.Role == ProjectRole.PM,
            TeamMembers = projectMembers
        };
    }

    public void ClearCache() => _cachedUserInfo = null;
}




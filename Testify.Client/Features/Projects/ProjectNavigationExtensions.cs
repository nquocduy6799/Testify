using Microsoft.AspNetCore.Components;
using Testify.Shared.Enums;

namespace Testify.Client.Features.Projects
{
    public static class ProjectNavigationExtensions
    {
        public static void NavigateToProject(this NavigationManager nav, int projectId, ProjectRole userRole)
        {
            var route = userRole switch
            {
                ProjectRole.PM => $"/projects/{projectId}/pm",
                ProjectRole.Dev => $"/projects/{projectId}/dev",
                ProjectRole.Tester => $"/projects/{projectId}/tester",
                _ => $"/projects/{projectId}"
            };

            nav.NavigateTo(route);
        }
    }
}

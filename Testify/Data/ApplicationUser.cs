using Microsoft.AspNetCore.Identity;

namespace Testify.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; } = null;

        /// <summary>
        /// Number of times this user has used AI test case generation.
        /// Free tier allows up to 3 uses.
        /// </summary>
        public int AiGenerationCount { get; set; } = 0;
    }

}


        
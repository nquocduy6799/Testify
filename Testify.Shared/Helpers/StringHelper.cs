namespace Testify.Shared.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Get initials from a full name (e.g., "John Doe" → "JD", "Alice" → "A").
        /// Returns "?" if name is null or empty.
        /// </summary>
        public static string GetInitials(string? name)
        {
            if (string.IsNullOrWhiteSpace(name)) return "?";
            var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
                return $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
            return parts[0][0].ToString().ToUpperInvariant();
        }

        /// <summary>
        /// Format a duration in seconds to "MM:SS" or "HH:MM:SS" (if ≥ 1 hour).
        /// </summary>
        public static string FormatDuration(int totalSeconds)
        {
            var hours = totalSeconds / 3600;
            var minutes = (totalSeconds % 3600) / 60;
            var seconds = totalSeconds % 60;

            if (hours > 0)
                return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
            return $"{minutes:D2}:{seconds:D2}";
        }
    }
}

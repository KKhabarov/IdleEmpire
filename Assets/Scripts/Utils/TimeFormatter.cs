using System;

namespace IdleEmpire.Utils
{
    /// <summary>
    /// Static utility for formatting time durations into human-readable strings.
    /// </summary>
    public static class TimeFormatter
    {
        /// <summary>
        /// Formats seconds into "Xh Ym Zs" format.
        /// Examples: 0 → "0s", 65 → "1m 5s", 3661 → "1h 1m 1s", 90061 → "1d 1h 1m"
        /// </summary>
        /// <param name="totalSeconds">Duration in seconds. Negative values are treated as 0.</param>
        /// <returns>Human-readable duration string.</returns>
        public static string FormatDuration(float totalSeconds)
        {
            if (totalSeconds < 0f)
                return "0s";

            int seconds = (int)totalSeconds;
            int days    = seconds / 86400;
            seconds    %= 86400;
            int hours   = seconds / 3600;
            seconds    %= 3600;
            int minutes = seconds / 60;
            int secs    = seconds % 60;

            if (days > 0)
                return $"{days}d {hours}h {minutes}m";

            if (hours > 0)
                return $"{hours}h {minutes}m {secs}s";

            if (minutes > 0)
                return $"{minutes}m {secs}s";

            return $"{secs}s";
        }

        /// <summary>
        /// Formats a relative time string for "time ago" display.
        /// Examples: 0s → "Just now", 30s → "30s ago", 120s → "2m ago", 7200s → "2h ago"
        /// </summary>
        /// <param name="secondsAgo">Elapsed seconds. Values ≤ 0 return "Just now".</param>
        /// <returns>Human-readable relative time string.</returns>
        public static string FormatTimeAgo(float secondsAgo)
        {
            if (secondsAgo <= 0f)
                return "Just now";

            int seconds = (int)secondsAgo;

            if (seconds < 60)
                return $"{seconds}s ago";

            int minutes = seconds / 60;
            if (minutes < 60)
                return $"{minutes}m ago";

            int hours = minutes / 60;
            if (hours < 24)
                return $"{hours}h ago";

            int days = hours / 24;
            return $"{days}d ago";
        }
    }
}

using System;
using UnityEngine;
using IdleEmpire.Business;

namespace IdleEmpire.Utils
{
    /// <summary>
    /// Static utility class that calculates the total earnings accumulated
    /// while the player was away (offline earnings).
    /// </summary>
    public static class OfflineCalculator
    {
        #region Public API

        /// <summary>
        /// Calculates the total income earned by managed businesses during the time
        /// the player was offline, capped at <paramref name="maxOfflineHours"/>.
        /// Income per second is read directly from each <see cref="BusinessController"/>,
        /// which already applies level, prestige, and upgrade multipliers internally.
        /// </summary>
        /// <param name="lastSaveTimestamp">Unix timestamp (UTC seconds) of the last save.</param>
        /// <param name="businesses">All business controllers in the game.</param>
        /// <param name="maxOfflineHours">Maximum hours of offline earnings to award (default: 8).</param>
        /// <returns>Total offline earnings as a <c>double</c>.</returns>
        public static double CalculateOfflineEarnings(
            long lastSaveTimestamp,
            BusinessController[] businesses,
            int maxOfflineHours = 8)
        {
            if (businesses == null || businesses.Length == 0) return 0;

            long nowTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            long elapsedSeconds = nowTimestamp - lastSaveTimestamp;

            if (elapsedSeconds <= 0) return 0;

            // Cap offline time.
            long maxSeconds = maxOfflineHours * 3600L;
            elapsedSeconds = Math.Min(elapsedSeconds, maxSeconds);

            double totalEarnings = 0;

            foreach (var business in businesses)
            {
                if (business == null) continue;

                // Only businesses with a manager generate offline earnings.
                if (!business.HasManager) continue;

                double ips = business.CalculateIncomePerSecond();
                if (ips <= 0) continue;

                totalEarnings += ips * elapsedSeconds;
            }

            Debug.Log($"[OfflineCalculator] Elapsed: {elapsedSeconds}s — Offline earnings: {totalEarnings:F2}");
            return totalEarnings;
        }

        #endregion
    }
}

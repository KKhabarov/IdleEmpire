using System;
using UnityEngine;
using IdleEmpire.Business;

namespace IdleEmpire.Utils
{
    /// <summary>
    /// Static utility class that calculates total earnings accumulated
    /// while the player was away (offline earnings).
    /// </summary>
    public static class OfflineCalculator
    {
        #region Public API

        /// <summary>
        /// Calculates the total income earned by managed businesses during the time
        /// the player was offline, capped at <paramref name="maxOfflineHours"/> hours.
        /// </summary>
        /// <param name="lastSaveTime">The UTC <see cref="DateTime"/> of the last save.</param>
        /// <param name="businesses">All business controllers in the game.</param>
        /// <param name="prestigeMultiplier">The current prestige multiplier (already applied to businesses).</param>
        /// <param name="maxOfflineHours">Maximum hours of offline earnings to award (default: 8).</param>
        /// <returns>Total offline earnings as a <c>double</c>.</returns>
        public static double CalculateOfflineEarnings(
            DateTime lastSaveTime,
            BusinessController[] businesses,
            double prestigeMultiplier,
            float maxOfflineHours = 8f)
        {
            if (businesses == null || businesses.Length == 0) return 0;

            double elapsedSeconds = (DateTime.UtcNow - lastSaveTime).TotalSeconds;
            if (elapsedSeconds <= 0) return 0;

            // Cap offline time.
            double maxSeconds = maxOfflineHours * 3600.0;
            elapsedSeconds = Math.Min(elapsedSeconds, maxSeconds);

            double totalEarnings = 0;

            foreach (var business in businesses)
            {
                if (business == null) continue;

                // Only businesses with a manager generate offline earnings.
                if (!business.HasManager) continue;

                double ips = business.GetIncomePerSecond();
                if (ips <= 0) continue;

                totalEarnings += ips * elapsedSeconds;
            }

            Debug.Log($"[OfflineCalculator] Elapsed: {elapsedSeconds:F0}s — Offline earnings: {totalEarnings:F2}");
            return totalEarnings;
        }

        #endregion
    }
}

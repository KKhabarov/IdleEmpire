using System;

namespace IdleEmpire.Utils
{
    /// <summary>
    /// Static utility class for formatting large numbers into human-readable strings.
    /// </summary>
    public static class NumberFormatter
    {
        #region Thresholds

        private static readonly (double threshold, string suffix)[] _tiers =
        {
            (1e18, "Qi"),   // Quintillion
            (1e15, "Qd"),   // Quadrillion
            (1e12, "T"),    // Trillion
            (1e9,  "B"),    // Billion
            (1e6,  "M"),    // Million
            (1e3,  "K"),    // Thousand
        };

        #endregion

        #region Public API

        /// <summary>
        /// Formats <paramref name="number"/> into a compact, human-readable string.
        /// </summary>
        /// <example>
        /// <code>
        /// FormatNumber(1500)          // "1.50K"
        /// FormatNumber(2_500_000)     // "2.50M"
        /// FormatNumber(3_700_000_000) // "3.70B"
        /// FormatNumber(999)           // "999"
        /// </code>
        /// </example>
        /// <param name="number">The value to format.</param>
        /// <returns>Formatted string with an appropriate suffix.</returns>
        public static string FormatNumber(double number)
        {
            if (double.IsNaN(number) || double.IsInfinity(number))
                return "∞";

            if (number < 0)
                return $"-{FormatNumber(-number)}";

            foreach (var (threshold, suffix) in _tiers)
            {
                if (number >= threshold)
                    return $"{number / threshold:F2}{suffix}";
            }

            // Below 1 000 — show as integer (no decimals needed for small values).
            return ((long)number).ToString();
        }

        #endregion
    }
}

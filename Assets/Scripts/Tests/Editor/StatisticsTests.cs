using NUnit.Framework;
using UnityEngine;
using IdleEmpire.Core;
using IdleEmpire.Statistics;

namespace IdleEmpire.Tests
{
    /// <summary>
    /// Unit tests for <see cref="SaveData"/> statistics fields and
    /// <see cref="StatisticsTracker.GetFormattedPlayTime"/>.
    /// </summary>
    [TestFixture]
    public class StatisticsTests
    {
        // ── SaveData statistics defaults ──────────────────────────────────────

        [Test]
        public void SaveData_TotalMoneyEarned_DefaultsToZero()
        {
            var data = new SaveData();
            Assert.AreEqual(0.0, data.totalMoneyEarned, 1e-9);
        }

        [Test]
        public void SaveData_TotalMoneySpent_DefaultsToZero()
        {
            var data = new SaveData();
            Assert.AreEqual(0.0, data.totalMoneySpent, 1e-9);
        }

        [Test]
        public void SaveData_TotalBusinessesPurchased_DefaultsToZero()
        {
            var data = new SaveData();
            Assert.AreEqual(0, data.totalBusinessesPurchased);
        }

        [Test]
        public void SaveData_TotalIncomeCollections_DefaultsToZero()
        {
            var data = new SaveData();
            Assert.AreEqual(0, data.totalIncomeCollections);
        }

        [Test]
        public void SaveData_HighestIncomePerSecond_DefaultsToZero()
        {
            var data = new SaveData();
            Assert.AreEqual(0.0, data.highestIncomePerSecond, 1e-9);
        }

        [Test]
        public void SaveData_TotalPlayTimeSeconds_DefaultsToZero()
        {
            var data = new SaveData();
            Assert.AreEqual(0f, data.totalPlayTimeSeconds, 1e-6f);
        }

        [Test]
        public void SaveData_PrestigeCount_DefaultsToZero()
        {
            var data = new SaveData();
            Assert.AreEqual(0, data.prestigeCount);
        }

        [Test]
        public void SaveData_UnlockedAchievementIndices_DefaultsToEmptyArray()
        {
            var data = new SaveData();
            Assert.IsNotNull(data.unlockedAchievementIndices);
            Assert.AreEqual(0, data.unlockedAchievementIndices.Length);
        }

        // ── SaveData statistics serialization round-trip ─────────────────────

        [Test]
        public void SaveData_StatisticsFields_RoundTrip()
        {
            var original = new SaveData
            {
                totalMoneyEarned        = 9_999_999.99,
                totalMoneySpent         = 5_000_000.0,
                totalBusinessesPurchased = 42,
                totalIncomeCollections  = 1337,
                highestIncomePerSecond  = 12345.6,
                totalPlayTimeSeconds    = 3661f,
                prestigeCount           = 3,
                unlockedAchievementIndices = new[] { 0, 2, 7 }
            };

            string json       = JsonUtility.ToJson(original);
            SaveData restored = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(original.totalMoneyEarned,         restored.totalMoneyEarned,         1e-6);
            Assert.AreEqual(original.totalMoneySpent,          restored.totalMoneySpent,           1e-6);
            Assert.AreEqual(original.totalBusinessesPurchased, restored.totalBusinessesPurchased);
            Assert.AreEqual(original.totalIncomeCollections,   restored.totalIncomeCollections);
            Assert.AreEqual(original.highestIncomePerSecond,   restored.highestIncomePerSecond,    1e-6);
            Assert.AreEqual(original.totalPlayTimeSeconds,     restored.totalPlayTimeSeconds,      1e-3f);
            Assert.AreEqual(original.prestigeCount,            restored.prestigeCount);
            Assert.AreEqual(original.unlockedAchievementIndices.Length, restored.unlockedAchievementIndices.Length);
            for (int i = 0; i < original.unlockedAchievementIndices.Length; i++)
                Assert.AreEqual(original.unlockedAchievementIndices[i], restored.unlockedAchievementIndices[i]);
        }

        // ── GetFormattedPlayTime ──────────────────────────────────────────────

        /// <summary>
        /// Helper: sets the private <c>_totalPlayTimeSeconds</c> field on a
        /// <see cref="StatisticsTracker"/> via reflection and returns formatted play time.
        /// </summary>
        private string FormatPlayTime(float seconds)
        {
            var go      = new GameObject();
            var tracker = go.AddComponent<StatisticsTracker>();
            var field   = typeof(StatisticsTracker).GetField(
                              "_totalPlayTimeSeconds",
                              System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field.SetValue(tracker, seconds);
            string result = tracker.GetFormattedPlayTime();
            Object.DestroyImmediate(go);
            return result;
        }

        [Test]
        public void GetFormattedPlayTime_Zero_ReturnsZeroSeconds()
        {
            Assert.AreEqual("0s", FormatPlayTime(0f));
        }

        [Test]
        public void GetFormattedPlayTime_59Seconds_ReturnsSeconds()
        {
            Assert.AreEqual("59s", FormatPlayTime(59f));
        }

        [Test]
        public void GetFormattedPlayTime_60Seconds_ReturnsOneMinute()
        {
            Assert.AreEqual("1m 0s", FormatPlayTime(60f));
        }

        [Test]
        public void GetFormattedPlayTime_90Seconds_ReturnsOneMinute30Seconds()
        {
            Assert.AreEqual("1m 30s", FormatPlayTime(90f));
        }

        [Test]
        public void GetFormattedPlayTime_3600Seconds_ReturnsOneHour()
        {
            Assert.AreEqual("1h 0m 0s", FormatPlayTime(3600f));
        }

        [Test]
        public void GetFormattedPlayTime_5445Seconds_ReturnsCorrectHoursMinutesSeconds()
        {
            // 5445 = 1h 30m 45s
            Assert.AreEqual("1h 30m 45s", FormatPlayTime(5445f));
        }

        [Test]
        public void GetFormattedPlayTime_LargeValue_FormatsCorrectly()
        {
            // 7200 = 2h 0m 0s
            Assert.AreEqual("2h 0m 0s", FormatPlayTime(7200f));
        }
    }
}

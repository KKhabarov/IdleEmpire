using System;
using NUnit.Framework;
using UnityEngine;
using IdleEmpire.Business;

namespace IdleEmpire.Tests
{
    /// <summary>
    /// Validates that the game economy values are balanced and internally consistent.
    /// Business/upgrade/manager data matches the design table in ECONOMY_BALANCE.md.
    /// </summary>
    [TestFixture]
    public class EconomyBalanceTests
    {
        #region Test Data

        // (baseCost, baseIncome, cycleDuration, costMultiplier)
        private static readonly (double baseCost, double baseIncome, float cycleDuration, float costMultiplier)[] BusinessStats =
        {
            (4,               1,    1f,   1.07f),   // 0 Lemonade Stand
            (60,              3,    3f,   1.15f),   // 1 Newspaper Route
            (720,             8,    6f,   1.14f),   // 2 Car Wash
            (8_640,           20,   12f,  1.13f),   // 3 Pizza Delivery
            (103_680,         50,   24f,  1.12f),   // 4 Donut Shop
            (1_244_160,       120,  48f,  1.11f),   // 5 Shrimp Boat
            (14_929_920,      300,  96f,  1.10f),   // 6 Hockey Team
            (179_159_040,     750,  192f, 1.09f),   // 7 Movie Studio
            (2_149_908_480,   1800, 384f, 1.08f),   // 8 Bank
            (25_798_901_760,  4500, 768f, 1.07f),   // 9 Oil Company
        };

        private static readonly double[] ManagerCosts =
        {
            1_000,
            15_000,
            150_000,
            1_500_000,
            15_000_000,
            100_000_000,
            750_000_000,
            5_000_000_000,
            25_000_000_000,
            150_000_000_000,
        };

        private static readonly double[] UpgradeCosts =
        {
            100,
            1_000,
            10_000,
            100_000,
            500_000,
            2_500_000,
            10_000_000,
            50_000_000,
            250_000_000,
            1_000_000_000,
            50_000,
            5_000_000,
            25_000_000,
            500_000_000,
            5_000_000_000,
        };

        #endregion

        #region Helpers

        private static BusinessData CreateBusiness(int index)
        {
            var (baseCost, baseIncome, cycleDuration, costMultiplier) = BusinessStats[index];
            var bd = ScriptableObject.CreateInstance<BusinessData>();
            var type = typeof(BusinessData);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            type.GetField("_baseCost",       flags).SetValue(bd, baseCost);
            type.GetField("_baseIncome",     flags).SetValue(bd, baseIncome);
            type.GetField("_cycleDuration",  flags).SetValue(bd, cycleDuration);
            type.GetField("_costMultiplier", flags).SetValue(bd, costMultiplier);
            return bd;
        }

        #endregion

        [Test]
        public void AllBusinesses_IncomePerSecondAtLevel1_IsPositive()
        {
            for (int i = 0; i < BusinessStats.Length; i++)
            {
                var bd = CreateBusiness(i);
                double incomePerSec = bd.GetIncomeForLevel(1) / bd.CycleDuration;
                Assert.Greater(incomePerSec, 0.0, $"Business {i} income/sec should be positive");
                UnityEngine.Object.DestroyImmediate(bd);
            }
        }

        [Test]
        public void BusinessCosts_IncreaseWithIndex()
        {
            for (int i = 1; i < BusinessStats.Length; i++)
            {
                Assert.Greater(
                    BusinessStats[i].baseCost,
                    BusinessStats[i - 1].baseCost,
                    $"Business {i} base cost should exceed business {i - 1}");
            }
        }

        [Test]
        public void ManagerCosts_IncreaseWithIndex()
        {
            for (int i = 1; i < ManagerCosts.Length; i++)
            {
                Assert.Greater(
                    ManagerCosts[i],
                    ManagerCosts[i - 1],
                    $"Manager {i} cost should exceed manager {i - 1}");
            }
        }

        [Test]
        public void AllBusinesses_ROI_IsWithinReasonableBounds()
        {
            // ROI (seconds to recoup base cost at level 1) should be between 1 second and 24 hours.
            const double minROI = 1.0;
            const double maxROI = 24.0 * 3600.0;

            for (int i = 0; i < BusinessStats.Length; i++)
            {
                var bd = CreateBusiness(i);
                double incomePerSec = bd.GetIncomeForLevel(1) / bd.CycleDuration;
                double roi = bd.BaseCost / incomePerSec;
                Assert.GreaterOrEqual(roi, minROI, $"Business {i} ROI is unrealistically fast");
                Assert.LessOrEqual(roi, maxROI, $"Business {i} ROI is unrealistically slow");
                UnityEngine.Object.DestroyImmediate(bd);
            }
        }

        [Test]
        public void UpgradeCosts_FirstTenIncrease_WithBusinessIndex()
        {
            // The first 10 upgrades (one per business) should cost progressively more.
            for (int i = 1; i < 10; i++)
            {
                Assert.Greater(
                    UpgradeCosts[i],
                    UpgradeCosts[i - 1],
                    $"Upgrade {i} cost should exceed upgrade {i - 1}");
            }
        }
    }
}

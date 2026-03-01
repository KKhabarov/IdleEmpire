using NUnit.Framework;
using UnityEngine;
using IdleEmpire.Business;

namespace IdleEmpire.Tests
{
    /// <summary>
    /// Unit tests for <see cref="BusinessData.GetCostForLevel"/> and
    /// <see cref="BusinessData.GetIncomeForLevel"/>.
    /// </summary>
    [TestFixture]
    public class BusinessDataTests
    {
        private BusinessData _business;

        [SetUp]
        public void SetUp()
        {
            _business = ScriptableObject.CreateInstance<BusinessData>();
            // Use reflection to set private serialized fields
            var type = typeof(BusinessData);
            type.GetField("_baseCost",      System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_business, 100.0);
            type.GetField("_baseIncome",    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_business, 10.0);
            type.GetField("_costMultiplier",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_business, 1.15f);
            type.GetField("_cycleDuration", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_business, 1f);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_business);
        }

        [Test]
        public void GetCostForLevel_Level0_ReturnsBaseCost()
        {
            Assert.AreEqual(100.0, _business.GetCostForLevel(0), 1e-6);
        }

        [Test]
        public void GetCostForLevel_Level1_ReturnsBaseCostTimesMultiplier()
        {
            double expected = 100.0 * 1.15;
            Assert.AreEqual(expected, _business.GetCostForLevel(1), 1e-6);
        }

        [Test]
        public void GetCostForLevel_Level10_ReturnsBaseCostTimesMultiplierPow10()
        {
            double expected = 100.0 * System.Math.Pow(1.15, 10);
            Assert.AreEqual(expected, _business.GetCostForLevel(10), 1e-4);
        }

        [Test]
        public void GetIncomeForLevel_Level0_ReturnsZero()
        {
            Assert.AreEqual(0.0, _business.GetIncomeForLevel(0), 1e-6);
        }

        [Test]
        public void GetIncomeForLevel_Level1_ReturnsBaseIncome()
        {
            Assert.AreEqual(10.0, _business.GetIncomeForLevel(1), 1e-6);
        }

        [Test]
        public void GetIncomeForLevel_Level5_ReturnsBaseIncomeTimes5()
        {
            Assert.AreEqual(50.0, _business.GetIncomeForLevel(5), 1e-6);
        }
    }
}

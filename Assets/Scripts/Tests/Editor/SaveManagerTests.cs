using NUnit.Framework;
using UnityEngine;
using IdleEmpire.Core;

namespace IdleEmpire.Tests
{
    /// <summary>
    /// Unit tests for <see cref="SaveData"/> serialization round-trips.
    /// </summary>
    [TestFixture]
    public class SaveManagerTests
    {
        [Test]
        public void DefaultSaveData_Money_IsZero()
        {
            var data = new SaveData();
            Assert.AreEqual(0.0, data.money, 1e-9);
        }

        [Test]
        public void DefaultSaveData_PrestigeMultiplier_IsOne()
        {
            var data = new SaveData();
            Assert.AreEqual(1.0, data.prestigeMultiplier, 1e-9);
        }

        [Test]
        public void SaveData_RoundTrip_PreservesAllFields()
        {
            var original = new SaveData
            {
                money             = 12345.67,
                prestigeMultiplier = 2.5,
                businessLevels    = new[] { 1, 3, 5 },
                managersHired     = new[] { true, false, true },
                upgradesPurchased = new[] { false, true },
                lastSaveTime      = "2026-01-01T00:00:00Z"
            };

            string json      = JsonUtility.ToJson(original);
            SaveData restored = JsonUtility.FromJson<SaveData>(json);

            Assert.AreEqual(original.money,              restored.money,              1e-9);
            Assert.AreEqual(original.prestigeMultiplier, restored.prestigeMultiplier, 1e-9);
            Assert.AreEqual(original.lastSaveTime,       restored.lastSaveTime);
            Assert.AreEqual(original.businessLevels.Length,    restored.businessLevels.Length);
            Assert.AreEqual(original.managersHired.Length,     restored.managersHired.Length);
            Assert.AreEqual(original.upgradesPurchased.Length, restored.upgradesPurchased.Length);

            for (int i = 0; i < original.businessLevels.Length; i++)
                Assert.AreEqual(original.businessLevels[i], restored.businessLevels[i]);

            for (int i = 0; i < original.managersHired.Length; i++)
                Assert.AreEqual(original.managersHired[i], restored.managersHired[i]);

            for (int i = 0; i < original.upgradesPurchased.Length; i++)
                Assert.AreEqual(original.upgradesPurchased[i], restored.upgradesPurchased[i]);
        }

        [Test]
        public void SaveData_EmptyArrays_SerializeAndDeserializeCorrectly()
        {
            var original = new SaveData
            {
                businessLevels    = new int[0],
                managersHired     = new bool[0],
                upgradesPurchased = new bool[0]
            };

            string json      = JsonUtility.ToJson(original);
            SaveData restored = JsonUtility.FromJson<SaveData>(json);

            Assert.IsNotNull(restored.businessLevels);
            Assert.IsNotNull(restored.managersHired);
            Assert.IsNotNull(restored.upgradesPurchased);
            Assert.AreEqual(0, restored.businessLevels.Length);
            Assert.AreEqual(0, restored.managersHired.Length);
            Assert.AreEqual(0, restored.upgradesPurchased.Length);
        }
    }
}

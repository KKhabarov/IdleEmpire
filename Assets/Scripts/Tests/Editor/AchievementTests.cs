using NUnit.Framework;
using UnityEngine;
using IdleEmpire.Achievements;

namespace IdleEmpire.Tests
{
    /// <summary>
    /// Unit tests for <see cref="AchievementData"/> and the <see cref="AchievementType"/> enum.
    /// </summary>
    [TestFixture]
    public class AchievementTests
    {
        private AchievementData _achievement;

        [SetUp]
        public void SetUp()
        {
            _achievement = ScriptableObject.CreateInstance<AchievementData>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_achievement);
        }

        // ── AchievementData creation ──────────────────────────────────────────

        [Test]
        public void AchievementData_CanBeCreated()
        {
            Assert.IsNotNull(_achievement);
        }

        [Test]
        public void AchievementData_DefaultName_IsNullOrEmpty()
        {
            Assert.IsTrue(string.IsNullOrEmpty(_achievement.AchievementName));
        }

        [Test]
        public void AchievementData_DefaultDescription_IsNullOrEmpty()
        {
            Assert.IsTrue(string.IsNullOrEmpty(_achievement.Description));
        }

        [Test]
        public void AchievementData_DefaultTargetValue_IsZero()
        {
            Assert.AreEqual(0.0, _achievement.TargetValue, 1e-9);
        }

        [Test]
        public void AchievementData_DefaultTargetBusinessIndex_IsMinusOne()
        {
            Assert.AreEqual(-1, _achievement.TargetBusinessIndex);
        }

        [Test]
        public void AchievementData_DefaultRewardMoney_IsZero()
        {
            Assert.AreEqual(0.0, _achievement.RewardMoney, 1e-9);
        }

        // ── Property round-trip via reflection ───────────────────────────────

        [Test]
        public void AchievementData_Properties_ReturnExpectedValues()
        {
            var type = typeof(AchievementData);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            type.GetField("_achievementName",    flags).SetValue(_achievement, "First Dollar");
            type.GetField("_description",        flags).SetValue(_achievement, "Earn your first dollar.");
            type.GetField("_type",               flags).SetValue(_achievement, AchievementType.TotalMoneyEarned);
            type.GetField("_targetValue",        flags).SetValue(_achievement, 1.0);
            type.GetField("_targetBusinessIndex",flags).SetValue(_achievement, 0);
            type.GetField("_rewardMoney",        flags).SetValue(_achievement, 100.0);

            Assert.AreEqual("First Dollar",                     _achievement.AchievementName);
            Assert.AreEqual("Earn your first dollar.",           _achievement.Description);
            Assert.AreEqual(AchievementType.TotalMoneyEarned,   _achievement.Type);
            Assert.AreEqual(1.0,                                 _achievement.TargetValue,  1e-9);
            Assert.AreEqual(0,                                   _achievement.TargetBusinessIndex);
            Assert.AreEqual(100.0,                               _achievement.RewardMoney,  1e-9);
        }

        // ── AchievementType enum ──────────────────────────────────────────────

        [Test]
        public void AchievementType_HasTotalMoneyEarned()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementType), AchievementType.TotalMoneyEarned));
        }

        [Test]
        public void AchievementType_HasBusinessLevel()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementType), AchievementType.BusinessLevel));
        }

        [Test]
        public void AchievementType_HasTotalBusinessLevel()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementType), AchievementType.TotalBusinessLevel));
        }

        [Test]
        public void AchievementType_HasManagersHired()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementType), AchievementType.ManagersHired));
        }

        [Test]
        public void AchievementType_HasUpgradesPurchased()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementType), AchievementType.UpgradesPurchased));
        }

        [Test]
        public void AchievementType_HasPrestigeCount()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementType), AchievementType.PrestigeCount));
        }

        [Test]
        public void AchievementType_HasIncomePerSecond()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementType), AchievementType.IncomePerSecond));
        }

        [Test]
        public void AchievementType_HasBusinessOwned()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementType), AchievementType.BusinessOwned));
        }

        [Test]
        public void AchievementType_HasSpecificBusinessLevel()
        {
            Assert.IsTrue(System.Enum.IsDefined(typeof(AchievementType), AchievementType.SpecificBusinessLevel));
        }

        [Test]
        public void AchievementType_HasExactlyNineValues()
        {
            Assert.AreEqual(9, System.Enum.GetValues(typeof(AchievementType)).Length);
        }

        // ── Target value validation ───────────────────────────────────────────

        [Test]
        public void AchievementData_TargetValue_CanBeSetPositive()
        {
            var type  = typeof(AchievementData);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            type.GetField("_targetValue", flags).SetValue(_achievement, 1000.0);

            Assert.Greater(_achievement.TargetValue, 0.0);
        }

        [Test]
        public void AchievementData_RewardMoney_CanBeSetToZero()
        {
            var type  = typeof(AchievementData);
            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
            type.GetField("_rewardMoney", flags).SetValue(_achievement, 0.0);

            Assert.AreEqual(0.0, _achievement.RewardMoney, 1e-9);
        }
    }
}

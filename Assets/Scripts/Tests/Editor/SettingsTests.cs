using System.Text.RegularExpressions;
using NUnit.Framework;
using IdleEmpire.Core;
using IdleEmpire.UI;

namespace IdleEmpire.Tests
{
    /// <summary>
    /// Unit tests for <see cref="GameVersion"/> constants and the <see cref="BulkBuyMode"/> enum.
    /// </summary>
    [TestFixture]
    public class SettingsTests
    {
        // ── GameVersion ───────────────────────────────────────────────────────

        [Test]
        public void GameVersion_Version_IsNotNullOrEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(GameVersion.Version));
        }

        [Test]
        public void GameVersion_BuildDate_IsNotNullOrEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(GameVersion.BuildDate));
        }

        [Test]
        public void GameVersion_GameName_IsNotNullOrEmpty()
        {
            Assert.IsFalse(string.IsNullOrEmpty(GameVersion.GameName));
        }

        [Test]
        public void GameVersion_Version_MatchesSemVer()
        {
            // Accepts strings like "1.0.0" or "1.2.3"
            Assert.IsTrue(Regex.IsMatch(GameVersion.Version, @"^\d+\.\d+\.\d+$"),
                $"Version '{GameVersion.Version}' does not match semver format.");
        }

        // ── BulkBuyMode enum ──────────────────────────────────────────────────

        [Test]
        public void BulkBuyMode_Buy1_HasValue1()
        {
            Assert.AreEqual(1, (int)BulkBuyMode.Buy1);
        }

        [Test]
        public void BulkBuyMode_Buy10_HasValue10()
        {
            Assert.AreEqual(10, (int)BulkBuyMode.Buy10);
        }

        [Test]
        public void BulkBuyMode_Buy25_HasValue25()
        {
            Assert.AreEqual(25, (int)BulkBuyMode.Buy25);
        }

        [Test]
        public void BulkBuyMode_BuyMax_HasValueMinusOne()
        {
            Assert.AreEqual(-1, (int)BulkBuyMode.BuyMax);
        }
    }
}

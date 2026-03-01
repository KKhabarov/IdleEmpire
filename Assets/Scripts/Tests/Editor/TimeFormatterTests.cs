using NUnit.Framework;
using IdleEmpire.Utils;

namespace IdleEmpire.Tests
{
    /// <summary>
    /// Unit tests for <see cref="TimeFormatter.FormatDuration"/> and
    /// <see cref="TimeFormatter.FormatTimeAgo"/>.
    /// </summary>
    [TestFixture]
    public class TimeFormatterTests
    {
        // ── FormatDuration ────────────────────────────────────────────────────

        [Test]
        public void FormatDuration_Zero_Returns0s()
        {
            Assert.AreEqual("0s", TimeFormatter.FormatDuration(0f));
        }

        [Test]
        public void FormatDuration_30_Returns30s()
        {
            Assert.AreEqual("30s", TimeFormatter.FormatDuration(30f));
        }

        [Test]
        public void FormatDuration_60_Returns1m0s()
        {
            Assert.AreEqual("1m 0s", TimeFormatter.FormatDuration(60f));
        }

        [Test]
        public void FormatDuration_65_Returns1m5s()
        {
            Assert.AreEqual("1m 5s", TimeFormatter.FormatDuration(65f));
        }

        [Test]
        public void FormatDuration_3600_Returns1h0m0s()
        {
            Assert.AreEqual("1h 0m 0s", TimeFormatter.FormatDuration(3600f));
        }

        [Test]
        public void FormatDuration_3661_Returns1h1m1s()
        {
            Assert.AreEqual("1h 1m 1s", TimeFormatter.FormatDuration(3661f));
        }

        [Test]
        public void FormatDuration_86400_Returns1d0h0m()
        {
            Assert.AreEqual("1d 0h 0m", TimeFormatter.FormatDuration(86400f));
        }

        [Test]
        public void FormatDuration_90061_Returns1d1h1m()
        {
            Assert.AreEqual("1d 1h 1m", TimeFormatter.FormatDuration(90061f));
        }

        [Test]
        public void FormatDuration_Negative_Returns0s()
        {
            Assert.AreEqual("0s", TimeFormatter.FormatDuration(-10f));
        }

        // ── FormatTimeAgo ─────────────────────────────────────────────────────

        [Test]
        public void FormatTimeAgo_Zero_ReturnsJustNow()
        {
            Assert.AreEqual("Just now", TimeFormatter.FormatTimeAgo(0f));
        }

        [Test]
        public void FormatTimeAgo_5_Returns5sAgo()
        {
            Assert.AreEqual("5s ago", TimeFormatter.FormatTimeAgo(5f));
        }

        [Test]
        public void FormatTimeAgo_60_Returns1mAgo()
        {
            Assert.AreEqual("1m ago", TimeFormatter.FormatTimeAgo(60f));
        }

        [Test]
        public void FormatTimeAgo_3600_Returns1hAgo()
        {
            Assert.AreEqual("1h ago", TimeFormatter.FormatTimeAgo(3600f));
        }

        [Test]
        public void FormatTimeAgo_7200_Returns2hAgo()
        {
            Assert.AreEqual("2h ago", TimeFormatter.FormatTimeAgo(7200f));
        }

        [Test]
        public void FormatTimeAgo_86400_Returns1dAgo()
        {
            Assert.AreEqual("1d ago", TimeFormatter.FormatTimeAgo(86400f));
        }
    }
}

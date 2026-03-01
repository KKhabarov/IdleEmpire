using NUnit.Framework;
using IdleEmpire.Utils;

namespace IdleEmpire.Tests
{
    /// <summary>
    /// Unit tests for <see cref="NumberFormatter.FormatNumber"/>.
    /// </summary>
    [TestFixture]
    public class NumberFormatterTests
    {
        [Test]
        public void FormatNumber_Zero_ReturnsZero()
        {
            Assert.AreEqual("0", NumberFormatter.FormatNumber(0));
        }

        [Test]
        public void FormatNumber_BelowThousand_ReturnsInteger()
        {
            Assert.AreEqual("999", NumberFormatter.FormatNumber(999));
        }

        [Test]
        public void FormatNumber_ExactlyThousand_ReturnsOnePointZeroZeroK()
        {
            Assert.AreEqual("1.00K", NumberFormatter.FormatNumber(1000));
        }

        [Test]
        public void FormatNumber_1500_Returns1Point50K()
        {
            Assert.AreEqual("1.50K", NumberFormatter.FormatNumber(1500));
        }

        [Test]
        public void FormatNumber_OneMillion_Returns1Point00M()
        {
            Assert.AreEqual("1.00M", NumberFormatter.FormatNumber(1_000_000));
        }

        [Test]
        public void FormatNumber_2500000_Returns2Point50M()
        {
            Assert.AreEqual("2.50M", NumberFormatter.FormatNumber(2_500_000));
        }

        [Test]
        public void FormatNumber_OneBillion_Returns1Point00B()
        {
            Assert.AreEqual("1.00B", NumberFormatter.FormatNumber(1_000_000_000));
        }

        [Test]
        public void FormatNumber_OneTrillion_Returns1Point00T()
        {
            Assert.AreEqual("1.00T", NumberFormatter.FormatNumber(1_000_000_000_000));
        }

        [Test]
        public void FormatNumber_Negative1500_ReturnsMinus1Point50K()
        {
            Assert.AreEqual("-1.50K", NumberFormatter.FormatNumber(-1500));
        }

        [Test]
        public void FormatNumber_NaN_ReturnsInfinitySymbol()
        {
            Assert.AreEqual("∞", NumberFormatter.FormatNumber(double.NaN));
        }

        [Test]
        public void FormatNumber_PositiveInfinity_ReturnsInfinitySymbol()
        {
            Assert.AreEqual("∞", NumberFormatter.FormatNumber(double.PositiveInfinity));
        }

        [Test]
        public void FormatNumber_1e18_Returns1Point00Qi()
        {
            Assert.AreEqual("1.00Qi", NumberFormatter.FormatNumber(1e18));
        }

        [Test]
        public void FormatNumber_1e21_Returns1Point00Sx()
        {
            Assert.AreEqual("1.00Sx", NumberFormatter.FormatNumber(1e21));
        }

        [Test]
        public void FormatNumber_1e33_Returns1Point00Dc()
        {
            Assert.AreEqual("1.00Dc", NumberFormatter.FormatNumber(1e33));
        }
    }
}

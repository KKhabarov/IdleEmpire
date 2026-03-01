using NUnit.Framework;
using UnityEngine;
using IdleEmpire.Tutorial;

namespace IdleEmpire.Tests
{
    /// <summary>
    /// Unit tests for <see cref="TutorialStep"/> ScriptableObject.
    /// </summary>
    [TestFixture]
    public class TutorialTests
    {
        private TutorialStep _step;

        [SetUp]
        public void SetUp()
        {
            _step = ScriptableObject.CreateInstance<TutorialStep>();
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_step);
        }

        [Test]
        public void TutorialStep_CanBeCreated()
        {
            Assert.IsNotNull(_step);
        }

        [Test]
        public void TutorialStep_DefaultProperties_AreEmpty()
        {
            Assert.IsNull(_step.Title);
            Assert.IsNull(_step.Message);
            Assert.IsNull(_step.TargetObjectName);
            Assert.IsFalse(_step.WaitForClick);
            Assert.IsFalse(_step.WaitForPurchase);
            Assert.IsFalse(_step.WaitForUpgrade);
            Assert.IsFalse(_step.WaitForManager);
            Assert.AreEqual(0f, _step.AutoAdvanceDelay, 1e-6f);
        }

        [Test]
        public void TutorialStep_WithAutoAdvanceDelay_IsAutoAdvance()
        {
            var type = typeof(TutorialStep);
            type.GetField("_autoAdvanceDelay",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_step, 3f);

            Assert.IsTrue(_step.IsAutoAdvance);
            Assert.AreEqual(3f, _step.AutoAdvanceDelay, 1e-6f);
        }

        [Test]
        public void TutorialStep_WithZeroAutoAdvanceDelay_IsNotAutoAdvance()
        {
            Assert.IsFalse(_step.IsAutoAdvance);
        }

        [Test]
        public void TutorialStep_WaitForPurchase_RequiresPurchase()
        {
            var type = typeof(TutorialStep);
            type.GetField("_waitForPurchase",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(_step, true);

            Assert.IsTrue(_step.WaitForPurchase);
        }

        [Test]
        public void TutorialStep_Properties_ReturnExpectedValues()
        {
            var type = typeof(TutorialStep);
            type.GetField("_title",            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_step, "Test Title");
            type.GetField("_message",          System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_step, "Test Message");
            type.GetField("_targetObjectName", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_step, "MyButton");
            type.GetField("_waitForClick",     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_step, true);
            type.GetField("_waitForUpgrade",   System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_step, true);
            type.GetField("_waitForManager",   System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(_step, true);

            Assert.AreEqual("Test Title",   _step.Title);
            Assert.AreEqual("Test Message", _step.Message);
            Assert.AreEqual("MyButton",     _step.TargetObjectName);
            Assert.IsTrue(_step.WaitForClick);
            Assert.IsTrue(_step.WaitForUpgrade);
            Assert.IsTrue(_step.WaitForManager);
        }
    }
}

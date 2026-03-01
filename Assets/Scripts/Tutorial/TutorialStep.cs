using UnityEngine;

namespace IdleEmpire.Tutorial
{
    /// <summary>
    /// ScriptableObject defining a single step in the new-player tutorial sequence.
    /// Create instances via Assets → Create → IdleEmpire → Tutorial Step.
    /// </summary>
    [CreateAssetMenu(menuName = "IdleEmpire/Tutorial Step", fileName = "NewTutorialStep")]
    public class TutorialStep : ScriptableObject
    {
        #region Inspector Fields

        [Header("Content")]
        [SerializeField] private string _title;
        [SerializeField] [TextArea(2, 5)] private string _message;

        [Header("Target")]
        [SerializeField] private string _targetObjectName;

        [Header("Advance Conditions")]
        [SerializeField] private bool _waitForClick;
        [SerializeField] private bool _waitForPurchase;
        [SerializeField] private bool _waitForUpgrade;
        [SerializeField] private bool _waitForManager;
        [SerializeField] private float _autoAdvanceDelay;

        #endregion

        #region Properties

        /// <summary>Heading displayed in the tutorial panel.</summary>
        public string Title => _title;

        /// <summary>Body text displayed in the tutorial panel.</summary>
        public string Message => _message;

        /// <summary>Name of the GameObject to highlight (optional; empty = no highlight).</summary>
        public string TargetObjectName => _targetObjectName;

        /// <summary>When <c>true</c>, step waits for the player to click the target before advancing.</summary>
        public bool WaitForClick => _waitForClick;

        /// <summary>When <c>true</c>, step waits for any business purchase before advancing.</summary>
        public bool WaitForPurchase => _waitForPurchase;

        /// <summary>When <c>true</c>, step waits for any upgrade purchase before advancing.</summary>
        public bool WaitForUpgrade => _waitForUpgrade;

        /// <summary>When <c>true</c>, step waits for any manager hire before advancing.</summary>
        public bool WaitForManager => _waitForManager;

        /// <summary>
        /// When greater than zero the step auto-advances after this many seconds,
        /// regardless of other wait conditions.
        /// </summary>
        public float AutoAdvanceDelay => _autoAdvanceDelay;

        /// <summary>Returns <c>true</c> when the step will auto-advance after a delay.</summary>
        public bool IsAutoAdvance => _autoAdvanceDelay > 0f;

        #endregion
    }
}

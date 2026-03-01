using System.Collections;
using UnityEngine;
using IdleEmpire.Business;
using IdleEmpire.Upgrades;
using IdleEmpire.Managers;

namespace IdleEmpire.Tutorial
{
    /// <summary>
    /// Drives the new-player tutorial sequence.
    /// Checks <c>PlayerPrefs</c> on <c>Start</c>; if the tutorial has not yet been
    /// completed it begins from step 0.  Subscribe buttons or external code to
    /// <see cref="AdvanceStep"/> and <see cref="SkipTutorial"/> as needed.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        #region Constants

        /// <summary>PlayerPrefs key used to persist tutorial completion state.</summary>
        public const string TutorialCompleteKey = "IdleEmpire_TutorialComplete";

        #endregion

        #region Inspector Fields

        [Header("Steps")]
        [SerializeField] private TutorialStep[] _steps;

        [Header("UI")]
        [SerializeField] private TutorialUI _tutorialUI;

        [Header("Business Controllers")]
        [SerializeField] private BusinessController[] _businesses;

        [Header("Managers")]
        [SerializeField] private UpgradeManager _upgradeManager;
        [SerializeField] private ManagerController _managerController;

        #endregion

        #region Private Fields

        private int _currentStepIndex = -1;
        private bool _tutorialCompleted;
        private Coroutine _autoAdvanceCoroutine;

        #endregion

        #region Properties

        /// <summary>Whether the tutorial has been completed or skipped.</summary>
        public bool TutorialCompleted => _tutorialCompleted;

        /// <summary>The index of the currently active tutorial step (-1 = not started).</summary>
        public int CurrentStepIndex => _currentStepIndex;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            _tutorialCompleted = PlayerPrefs.GetInt(TutorialCompleteKey, 0) == 1;

            if (!_tutorialCompleted)
                StartTutorial();
        }

        #endregion

        #region Public API

        /// <summary>Begins the tutorial from step 0.</summary>
        public void StartTutorial()
        {
            if (_steps == null || _steps.Length == 0)
            {
                Debug.LogWarning("[TutorialManager] No tutorial steps assigned.");
                return;
            }

            _currentStepIndex = -1;
            AdvanceStep();
        }

        /// <summary>
        /// Advances to the next tutorial step.
        /// Completes the tutorial when there are no more steps.
        /// </summary>
        public void AdvanceStep()
        {
            UnsubscribeFromEvents();

            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }

            _currentStepIndex++;

            if (_steps == null || _currentStepIndex >= _steps.Length)
            {
                CompleteTutorial();
                return;
            }

            TutorialStep step = _steps[_currentStepIndex];

            _tutorialUI?.ShowStep(step);

            bool hasEventWait = step.WaitForPurchase || step.WaitForUpgrade || step.WaitForManager;

            // Hide "Next" button when the step requires an in-game action or a click on the target.
            _tutorialUI?.SetNextButtonVisible(!hasEventWait && !step.WaitForClick);

            if (step.WaitForPurchase)
                SubscribeToBusinessEvents();

            if (step.WaitForUpgrade && _upgradeManager != null)
                _upgradeManager.OnUpgradesChanged += OnUpgradesChanged;

            if (step.WaitForManager && _managerController != null)
                _managerController.OnManagersChanged += OnManagersChanged;

            if (step.IsAutoAdvance)
                _autoAdvanceCoroutine = StartCoroutine(AutoAdvanceAfterDelay(step.AutoAdvanceDelay));

            Debug.Log($"[TutorialManager] Step {_currentStepIndex + 1}/{_steps.Length}: {step.Title}");
        }

        /// <summary>Marks the tutorial as complete and hides the tutorial UI.</summary>
        public void CompleteTutorial()
        {
            _tutorialCompleted = true;
            PlayerPrefs.SetInt(TutorialCompleteKey, 1);
            PlayerPrefs.Save();

            UnsubscribeFromEvents();

            if (_autoAdvanceCoroutine != null)
            {
                StopCoroutine(_autoAdvanceCoroutine);
                _autoAdvanceCoroutine = null;
            }

            _tutorialUI?.HideStep();
            Debug.Log("[TutorialManager] Tutorial completed.");
        }

        /// <summary>
        /// Lets the player skip the tutorial entirely.
        /// Equivalent to completing it immediately.
        /// </summary>
        public void SkipTutorial()
        {
            Debug.Log("[TutorialManager] Tutorial skipped by player.");
            CompleteTutorial();
        }

        #endregion

        #region Event Handlers

        private void OnBusinessLevelChanged(BusinessController business)
        {
            if (_currentStepIndex < 0 || _steps == null || _currentStepIndex >= _steps.Length) return;
            if (_steps[_currentStepIndex].WaitForPurchase)
                AdvanceStep();
        }

        private void OnUpgradesChanged()
        {
            if (_currentStepIndex < 0 || _steps == null || _currentStepIndex >= _steps.Length) return;
            if (_steps[_currentStepIndex].WaitForUpgrade)
                AdvanceStep();
        }

        private void OnManagersChanged()
        {
            if (_currentStepIndex < 0 || _steps == null || _currentStepIndex >= _steps.Length) return;
            if (_steps[_currentStepIndex].WaitForManager)
                AdvanceStep();
        }

        #endregion

        #region Coroutines

        private IEnumerator AutoAdvanceAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            AdvanceStep();
        }

        #endregion

        #region Helpers

        private void SubscribeToBusinessEvents()
        {
            if (_businesses == null) return;
            foreach (var business in _businesses)
            {
                if (business != null)
                    business.OnLevelChanged += OnBusinessLevelChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (_businesses != null)
            {
                foreach (var business in _businesses)
                {
                    if (business != null)
                        business.OnLevelChanged -= OnBusinessLevelChanged;
                }
            }

            if (_upgradeManager != null)
                _upgradeManager.OnUpgradesChanged -= OnUpgradesChanged;

            if (_managerController != null)
                _managerController.OnManagersChanged -= OnManagersChanged;
        }

        #endregion
    }
}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleEmpire.Tutorial
{
    /// <summary>
    /// Manages the tutorial overlay UI: shows/hides the panel, updates text,
    /// and optionally highlights a named target GameObject.
    /// </summary>
    public class TutorialUI : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Panel")]
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;

        [Header("Buttons")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _skipButton;

        [Header("Highlight")]
        [SerializeField] private Image _highlightOverlay;
        [SerializeField] private RectTransform _highlightCutout;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (_tutorialPanel != null)
                _tutorialPanel.SetActive(false);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Displays the tutorial panel populated with data from <paramref name="step"/>.
        /// </summary>
        /// <param name="step">The tutorial step to display.</param>
        public void ShowStep(TutorialStep step)
        {
            if (step == null) return;

            if (_titleText != null)
                _titleText.text = step.Title;

            if (_messageText != null)
                _messageText.text = step.Message;

            if (_tutorialPanel != null)
                _tutorialPanel.SetActive(true);

            if (!string.IsNullOrEmpty(step.TargetObjectName))
                HighlightTarget(step.TargetObjectName);
            else
                ClearHighlight();
        }

        /// <summary>Hides the tutorial panel.</summary>
        public void HideStep()
        {
            if (_tutorialPanel != null)
                _tutorialPanel.SetActive(false);

            ClearHighlight();
        }

        /// <summary>
        /// Shows or hides the "Next / Got it!" button.
        /// Hide it when the step requires the player to perform an in-game action.
        /// </summary>
        /// <param name="visible"><c>true</c> to show, <c>false</c> to hide.</param>
        public void SetNextButtonVisible(bool visible)
        {
            if (_nextButton != null)
                _nextButton.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Finds <paramref name="targetObjectName"/> in the scene and positions
        /// <see cref="_highlightCutout"/> around its RectTransform so it stands out
        /// through the semi-transparent overlay.
        /// </summary>
        /// <param name="targetObjectName">The name of the GameObject to highlight.</param>
        public void HighlightTarget(string targetObjectName)
        {
            if (string.IsNullOrEmpty(targetObjectName)) return;

            GameObject target = GameObject.Find(targetObjectName);
            if (target == null)
            {
                Debug.LogWarning($"[TutorialUI] Target '{targetObjectName}' not found in scene.");
                ClearHighlight();
                return;
            }

            RectTransform targetRect = target.GetComponent<RectTransform>();
            if (targetRect == null)
            {
                ClearHighlight();
                return;
            }

            if (_highlightOverlay != null)
                _highlightOverlay.gameObject.SetActive(true);

            if (_highlightCutout != null)
            {
                _highlightCutout.gameObject.SetActive(true);
                _highlightCutout.position = targetRect.position;
                _highlightCutout.sizeDelta = targetRect.rect.size;
            }
        }

        #endregion

        #region Helpers

        private void ClearHighlight()
        {
            if (_highlightOverlay != null)
                _highlightOverlay.gameObject.SetActive(false);

            if (_highlightCutout != null)
                _highlightCutout.gameObject.SetActive(false);
        }

        #endregion
    }
}

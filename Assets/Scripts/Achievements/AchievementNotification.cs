using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleEmpire.Achievements
{
    /// <summary>
    /// Displays a brief slide-in popup whenever an achievement is unlocked.
    /// Multiple simultaneous unlocks are queued and shown one after another.
    /// </summary>
    public class AchievementNotification : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Panel")]
        [Tooltip("Root GameObject of the notification panel. Shown/hidden during animations.")]
        [SerializeField] private GameObject _notificationPanel;

        [Tooltip("CanvasGroup used to fade the notification in and out.")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Content")]
        [Tooltip("Text field for the achievement name.")]
        [SerializeField] private TextMeshProUGUI _achievementNameText;

        [Tooltip("Text field for the achievement description.")]
        [SerializeField] private TextMeshProUGUI _achievementDescriptionText;

        [Tooltip("Image for the achievement icon.")]
        [SerializeField] private Image _achievementIcon;

        [Header("Settings")]
        [Tooltip("How long (in seconds) the notification stays fully visible before fading out.")]
        [SerializeField] private float _displayDuration = 3f;

        #endregion

        #region Private Fields

        private readonly Queue<AchievementData> _notificationQueue = new Queue<AchievementData>();
        private bool _isShowing;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (_notificationPanel != null)
                _notificationPanel.SetActive(false);
            if (_canvasGroup != null)
                _canvasGroup.alpha = 0f;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Adds <paramref name="achievement"/> to the notification queue.
        /// If nothing is currently being displayed the sequence starts immediately.
        /// </summary>
        public void Show(AchievementData achievement)
        {
            if (achievement == null) return;
            _notificationQueue.Enqueue(achievement);
            if (!_isShowing)
                StartCoroutine(ShowNext());
        }

        #endregion

        #region Private Helpers

        private IEnumerator ShowNext()
        {
            if (_notificationQueue.Count == 0)
            {
                _isShowing = false;
                yield break;
            }

            _isShowing = true;
            AchievementData data = _notificationQueue.Dequeue();

            // Populate content.
            if (_achievementNameText != null)
                _achievementNameText.text = data.AchievementName;
            if (_achievementDescriptionText != null)
                _achievementDescriptionText.text = data.Description;
            if (_achievementIcon != null && data.Icon != null)
                _achievementIcon.sprite = data.Icon;

            // Show panel.
            if (_notificationPanel != null)
                _notificationPanel.SetActive(true);

            // Fade in.
            yield return StartCoroutine(Fade(0f, 1f, 0.3f));

            // Wait.
            yield return new WaitForSeconds(_displayDuration);

            // Fade out.
            yield return StartCoroutine(Fade(1f, 0f, 0.3f));

            // Hide panel.
            if (_notificationPanel != null)
                _notificationPanel.SetActive(false);

            // Show next queued notification.
            yield return StartCoroutine(ShowNext());
        }

        private IEnumerator Fade(float from, float to, float duration)
        {
            if (_canvasGroup == null) yield break;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            _canvasGroup.alpha = to;
        }

        #endregion
    }
}

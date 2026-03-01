using UnityEngine;
using TMPro;
using IdleEmpire.Utils;

namespace IdleEmpire.UI
{
    /// <summary>
    /// Floating text feedback prefab spawned when the player collects income.
    /// Floats upward and fades out over <see cref="_duration"/> seconds, then destroys itself.
    /// </summary>
    public class ClickFeedback : MonoBehaviour
    {
        #region Inspector Fields

        [Header("References")]
        [SerializeField] private TextMeshProUGUI _text;
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Animation")]
        [Tooltip("How long the feedback lives (seconds).")]
        [SerializeField] private float _duration = 1f;
        [Tooltip("Pixels per second the text moves upward.")]
        [SerializeField] private float _floatSpeed = 100f;

        #endregion

        #region Private Fields

        private float _timer;

        #endregion

        #region Public API

        /// <summary>
        /// Initialises the feedback at the given world position with the collected amount as text.
        /// </summary>
        /// <param name="amount">The income amount to display.</param>
        /// <param name="position">World-space position at which to spawn the text.</param>
        public void Show(double amount, Vector3 position)
        {
            transform.position = position;

            if (_text != null)
                _text.text = $"+{NumberFormatter.FormatNumber(amount)}";

            _timer = 0f;

            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f;
        }

        #endregion

        #region Unity Callbacks

        private void Update()
        {
            _timer += Time.deltaTime;
            float progress = _timer / Mathf.Max(_duration, float.Epsilon);

            // Float upward.
            transform.position += Vector3.up * _floatSpeed * Time.deltaTime;

            // Fade out.
            if (_canvasGroup != null)
                _canvasGroup.alpha = 1f - progress;

            if (_timer >= _duration)
                Destroy(gameObject);
        }

        #endregion
    }
}

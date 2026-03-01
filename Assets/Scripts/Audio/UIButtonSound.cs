using UnityEngine;
using UnityEngine.UI;

namespace IdleEmpire.Audio
{
    /// <summary>
    /// Attach to any <c>GameObject</c> that has a <see cref="Button"/> component to
    /// automatically play a click sound when the button is pressed.
    /// Uses <see cref="AudioManager.Instance.PlayClick"/> by default, or a custom
    /// <see cref="AudioClip"/> when <see cref="_useCustomClip"/> is enabled.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButtonSound : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Custom Clip (optional)")]
        [SerializeField] private bool _useCustomClip = false;
        [SerializeField] private AudioClip _customClip;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnClicked);
        }

        #endregion

        #region Event Handlers

        private void OnClicked()
        {
            if (AudioManager.Instance == null) return;

            if (_useCustomClip && _customClip != null)
                AudioManager.Instance.PlaySfx(_customClip);
            else
                AudioManager.Instance.PlayClick();
        }

        #endregion
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleEmpire.Audio;
using IdleEmpire.Core;
using IdleEmpire.Tutorial;
using IdleEmpire.Utils;

namespace IdleEmpire.UI
{
    /// <summary>
    /// Settings panel UI controller.
    /// Provides audio volume sliders, toggles, data management buttons,
    /// and displays game version information.
    /// </summary>
    public class SettingsUI : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Audio Settings")]
        [SerializeField] private Slider _musicVolumeSlider;
        [SerializeField] private Slider _sfxVolumeSlider;
        [SerializeField] private Toggle _musicToggle;
        [SerializeField] private Toggle _sfxToggle;
        [SerializeField] private TextMeshProUGUI _musicVolumeLabel;
        [SerializeField] private TextMeshProUGUI _sfxVolumeLabel;

        [Header("Data Management")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _resetTutorialButton;
        [SerializeField] private GameObject _resetConfirmPanel;
        [SerializeField] private Button _resetConfirmYes;
        [SerializeField] private Button _resetConfirmNo;
        [SerializeField] private TextMeshProUGUI _lastSaveTimeText;

        [Header("Game Info")]
        [SerializeField] private TextMeshProUGUI _versionText;
        [SerializeField] private TextMeshProUGUI _creditsText;

        [Header("References")]
        [SerializeField] private GameManager _gameManager;

        #endregion

        #region Private Fields

        private float _lastSaveTimestamp = -1f;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (_saveButton        != null) _saveButton.onClick.AddListener(OnSaveClicked);
            if (_resetButton       != null) _resetButton.onClick.AddListener(OnResetClicked);
            if (_resetTutorialButton != null) _resetTutorialButton.onClick.AddListener(OnResetTutorialClicked);
            if (_resetConfirmYes   != null) _resetConfirmYes.onClick.AddListener(OnResetConfirmYes);
            if (_resetConfirmNo    != null) _resetConfirmNo.onClick.AddListener(OnResetConfirmNo);

            if (_musicVolumeSlider != null)
                _musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (_sfxVolumeSlider   != null)
                _sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            if (_musicToggle != null)
                _musicToggle.onValueChanged.AddListener(OnMusicToggleChanged);
            if (_sfxToggle   != null)
                _sfxToggle.onValueChanged.AddListener(OnSfxToggleChanged);

            if (_versionText != null)
                _versionText.text = $"v{GameVersion.Version}";

            if (_resetConfirmPanel != null)
                _resetConfirmPanel.SetActive(false);
        }

        private void OnEnable()
        {
            LoadCurrentSettings();
            UpdateLastSaveTime();
        }

        private void Update()
        {
            UpdateLastSaveTime();
        }

        #endregion

        #region Settings Loading

        private void LoadCurrentSettings()
        {
            AudioManager audio = AudioManager.Instance;
            if (audio == null) return;

            if (_musicVolumeSlider != null)
            {
                _musicVolumeSlider.SetValueWithoutNotify(audio.MusicVolume);
                UpdateMusicLabel(audio.MusicVolume);
            }

            if (_sfxVolumeSlider != null)
            {
                _sfxVolumeSlider.SetValueWithoutNotify(audio.SfxVolume);
                UpdateSfxLabel(audio.SfxVolume);
            }

            if (_musicToggle != null)
                _musicToggle.SetIsOnWithoutNotify(audio.IsMusicEnabled);

            if (_sfxToggle != null)
                _sfxToggle.SetIsOnWithoutNotify(audio.IsSfxEnabled);
        }

        #endregion

        #region Slider Callbacks

        private void OnMusicVolumeChanged(float value)
        {
            AudioManager.Instance?.SetMusicVolume(value);
            UpdateMusicLabel(value);
        }

        private void OnSfxVolumeChanged(float value)
        {
            AudioManager.Instance?.SetSfxVolume(value);
            UpdateSfxLabel(value);
        }

        private void UpdateMusicLabel(float value)
        {
            if (_musicVolumeLabel != null)
                _musicVolumeLabel.text = $"Music: {Mathf.RoundToInt(value * 100f)}%";
        }

        private void UpdateSfxLabel(float value)
        {
            if (_sfxVolumeLabel != null)
                _sfxVolumeLabel.text = $"SFX: {Mathf.RoundToInt(value * 100f)}%";
        }

        #endregion

        #region Toggle Callbacks

        private void OnMusicToggleChanged(bool enabled)
        {
            AudioManager.Instance?.ToggleMusic(enabled);
        }

        private void OnSfxToggleChanged(bool enabled)
        {
            AudioManager.Instance?.ToggleSfx(enabled);
        }

        #endregion

        #region Button Callbacks

        private void OnSaveClicked()
        {
            AudioManager.Instance?.PlayClick();
            GameManager gm = _gameManager != null ? _gameManager : GameManager.Instance;
            gm?.SaveGame();
            _lastSaveTimestamp = Time.realtimeSinceStartup;
            UpdateLastSaveTime();
        }

        private void OnResetClicked()
        {
            if (_resetConfirmPanel != null)
                _resetConfirmPanel.SetActive(true);
        }

        private void OnResetConfirmYes()
        {
            if (_resetConfirmPanel != null)
                _resetConfirmPanel.SetActive(false);

            // Delete all saved data.
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();

            // Reload the current scene to reset game state.
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
        }

        private void OnResetConfirmNo()
        {
            if (_resetConfirmPanel != null)
                _resetConfirmPanel.SetActive(false);
        }

        private void OnResetTutorialClicked()
        {
            PlayerPrefs.DeleteKey(TutorialManager.TutorialCompleteKey);
            PlayerPrefs.Save();
            AudioManager.Instance?.PlayClick();
            Debug.Log("[SettingsUI] Tutorial progress reset.");
        }

        #endregion

        #region Last Save Time

        private void UpdateLastSaveTime()
        {
            if (_lastSaveTimeText == null) return;

            if (_lastSaveTimestamp < 0f)
            {
                _lastSaveTimeText.text = "Last saved: Never";
                return;
            }

            float elapsed = Time.realtimeSinceStartup - _lastSaveTimestamp;
            _lastSaveTimeText.text = $"Last saved: {TimeFormatter.FormatTimeAgo(elapsed)}";
        }

        #endregion
    }
}

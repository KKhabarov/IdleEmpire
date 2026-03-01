using UnityEngine;

namespace IdleEmpire.Audio
{
    /// <summary>
    /// Singleton MonoBehaviour that manages all game audio — background music and sound effects.
    /// Audio settings (volume, enabled flags) are persisted independently in <c>PlayerPrefs</c>
    /// so they survive prestige resets.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        #region Constants

        private const string KeyMusicVolume  = "IdleEmpire_MusicVolume";
        private const string KeySfxVolume    = "IdleEmpire_SfxVolume";
        private const string KeyMusicEnabled = "IdleEmpire_MusicEnabled";
        private const string KeySfxEnabled   = "IdleEmpire_SfxEnabled";

        private const float DefaultMusicVolume = 0.3f;
        private const float DefaultSfxVolume   = 0.5f;

        #endregion

        #region Singleton

        /// <summary>Shared instance of the AudioManager.</summary>
        public static AudioManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        #endregion

        #region Inspector Fields

        [Header("Audio Sources")]
        [SerializeField] private AudioSource _musicSource;
        [SerializeField] private AudioSource _sfxSource;

        [Header("Music")]
        [SerializeField] private AudioClip _backgroundMusic;
        [SerializeField] private float _musicVolume = 0.3f;

        [Header("Sound Effects")]
        [SerializeField] private AudioClip _clickSfx;
        [SerializeField] private AudioClip _purchaseSfx;
        [SerializeField] private AudioClip _collectSfx;
        [SerializeField] private AudioClip _managerHireSfx;
        [SerializeField] private AudioClip _prestigeSfx;
        [SerializeField] private AudioClip _upgradesSfx;
        [SerializeField] private AudioClip _errorSfx;
        [SerializeField] private AudioClip _levelUpSfx;
        [SerializeField] private AudioClip _offlineEarningsSfx;

        [Header("Settings")]
        [SerializeField] private float _sfxVolume = 0.5f;
        [SerializeField] private bool _musicEnabled = true;
        [SerializeField] private bool _sfxEnabled = true;

        #endregion

        #region Properties

        /// <summary>Whether background music is currently enabled.</summary>
        public bool IsMusicEnabled => _musicEnabled;

        /// <summary>Whether sound effects are currently enabled.</summary>
        public bool IsSfxEnabled => _sfxEnabled;

        /// <summary>Current music volume (0–1).</summary>
        public float MusicVolume => _musicVolume;

        /// <summary>Current SFX volume (0–1).</summary>
        public float SfxVolume => _sfxVolume;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            LoadSettings();
            ApplyMusicSettings();
        }

        #endregion
        #region SFX Public API

        /// <summary>Plays an arbitrary one-shot clip if SFX are enabled.</summary>
        /// <param name="clip">The clip to play.</param>
        public void PlaySfx(AudioClip clip)
        {
            if (!_sfxEnabled || clip == null || _sfxSource == null) return;
            _sfxSource.PlayOneShot(clip, _sfxVolume);
        }

        /// <summary>Plays the button-click sound effect.</summary>
        public void PlayClick() => PlaySfx(_clickSfx);

        /// <summary>Plays the business purchase / level-up purchase sound effect.</summary>
        public void PlayPurchase() => PlaySfx(_purchaseSfx);

        /// <summary>Plays the income collection sound effect.</summary>
        public void PlayCollect() => PlaySfx(_collectSfx);

        /// <summary>Plays the manager-hired sound effect.</summary>
        public void PlayManagerHire() => PlaySfx(_managerHireSfx);

        /// <summary>Plays the prestige reset sound effect.</summary>
        public void PlayPrestige() => PlaySfx(_prestigeSfx);

        /// <summary>Plays the upgrade purchased sound effect.</summary>
        public void PlayUpgrade() => PlaySfx(_upgradesSfx);

        /// <summary>Plays the error / can't-afford sound effect.</summary>
        public void PlayError() => PlaySfx(_errorSfx);

        /// <summary>Plays the level-milestone sound effect (e.g. every 25 levels).</summary>
        public void PlayLevelUp() => PlaySfx(_levelUpSfx);

        /// <summary>Plays the offline-earnings-popup sound effect.</summary>
        public void PlayOfflineEarnings() => PlaySfx(_offlineEarningsSfx);

        #endregion

        #region Settings Public API

        /// <summary>
        /// Sets the music volume, applies it immediately, and persists the setting.
        /// </summary>
        /// <param name="volume">New volume in the range [0, 1].</param>
        public void SetMusicVolume(float volume)
        {
            _musicVolume = Mathf.Clamp01(volume);
            if (_musicSource != null)
                _musicSource.volume = _musicVolume;
            PlayerPrefs.SetFloat(KeyMusicVolume, _musicVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Sets the SFX volume and persists the setting.
        /// </summary>
        /// <param name="volume">New volume in the range [0, 1].</param>
        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
            PlayerPrefs.SetFloat(KeySfxVolume, _sfxVolume);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Enables or disables background music and persists the setting.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable music, <c>false</c> to stop it.</param>
        public void ToggleMusic(bool enabled)
        {
            _musicEnabled = enabled;
            ApplyMusicSettings();
            PlayerPrefs.SetInt(KeyMusicEnabled, _musicEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Enables or disables all sound effects and persists the setting.
        /// </summary>
        /// <param name="enabled"><c>true</c> to enable SFX, <c>false</c> to disable them.</param>
        public void ToggleSfx(bool enabled)
        {
            _sfxEnabled = enabled;
            PlayerPrefs.SetInt(KeySfxEnabled, _sfxEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }

        #endregion

        #region Helpers

        private void LoadSettings()
        {
            _musicVolume  = PlayerPrefs.GetFloat(KeyMusicVolume,  DefaultMusicVolume);
            _sfxVolume    = PlayerPrefs.GetFloat(KeySfxVolume,    DefaultSfxVolume);
            _musicEnabled = PlayerPrefs.GetInt(KeyMusicEnabled, _musicEnabled ? 1 : 0) == 1;
            _sfxEnabled   = PlayerPrefs.GetInt(KeySfxEnabled,   _sfxEnabled   ? 1 : 0) == 1;
        }

        private void ApplyMusicSettings()
        {
            if (_musicSource == null) return;

            _musicSource.volume = _musicVolume;

            if (_musicEnabled && _backgroundMusic != null)
            {
                if (!_musicSource.isPlaying)
                {
                    _musicSource.clip = _backgroundMusic;
                    _musicSource.loop = true;
                    _musicSource.Play();
                }
            }
            else
            {
                _musicSource.Stop();
            }
        }

        #endregion
    }
}

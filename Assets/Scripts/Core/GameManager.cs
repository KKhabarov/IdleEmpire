using System;
using UnityEngine;
using IdleEmpire.Business;
using IdleEmpire.Upgrades;
using IdleEmpire.Managers;
using IdleEmpire.Utils;

namespace IdleEmpire.Core
{
    /// <summary>
    /// Main game controller. Singleton MonoBehaviour with <c>DontDestroyOnLoad</c>.
    /// Manages initialization, offline earnings, periodic auto-save, pause/resume, and prestige.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        #region Singleton

        /// <summary>The single shared instance of GameManager.</summary>
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Find managers if not assigned in inspector.
            if (_currencyManager == null)
                _currencyManager = FindObjectOfType<CurrencyManager>();
            if (_saveManager == null)
                _saveManager = FindObjectOfType<SaveManager>();
        }

        #endregion

        #region Inspector Fields

        [Header("Managers")]
        [SerializeField] private CurrencyManager _currencyManager;
        [SerializeField] private SaveManager _saveManager;
        [SerializeField] private UpgradeManager _upgradeManager;
        [SerializeField] private ManagerController _managerController;

        [Header("Businesses")]
        [SerializeField] private BusinessController[] _businesses;

        [Header("Settings")]
        [SerializeField] private float _autoSaveInterval = 60f;
        [SerializeField] private float _maxOfflineHours = 8f;
        [SerializeField] private double _prestigeBonusPerReset = 0.5;

        #endregion

        #region Private Fields

        private double _prestigeMultiplier = 1.0;

        #endregion

        #region Properties

        /// <summary>Gets the CurrencyManager instance.</summary>
        public CurrencyManager CurrencyManager => _currencyManager;

        /// <summary>Gets the SaveManager instance.</summary>
        public SaveManager SaveManager => _saveManager;

        /// <summary>Gets the ManagerController instance.</summary>
        public ManagerController ManagerController => _managerController;

        /// <summary>Whether the game is currently paused.</summary>
        public bool IsPaused { get; private set; }

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            // Load save and apply state.
            SaveData saveData = _saveManager?.Load();
            if (saveData != null)
            {
                ApplySaveData(saveData);

                // Calculate and apply offline earnings.
                if (DateTime.TryParse(saveData.lastSaveTime, null,
                        System.Globalization.DateTimeStyles.RoundtripKind, out DateTime lastSave))
                {
                    double offlineEarnings = OfflineCalculator.CalculateOfflineEarnings(
                        lastSave,
                        _businesses,
                        _prestigeMultiplier,
                        _maxOfflineHours);

                    if (offlineEarnings > 0)
                    {
                        _currencyManager?.AddMoney(offlineEarnings);
                        Debug.Log($"[GameManager] Offline earnings applied: {NumberFormatter.FormatNumber(offlineEarnings)}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[GameManager] Could not parse lastSaveTime: '{saveData.lastSaveTime}'. Offline earnings skipped.");
                }
            }

            // Auto-save every 60 seconds.
            InvokeRepeating(nameof(SaveGame), _autoSaveInterval, _autoSaveInterval);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                SaveGame();
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        #endregion

        #region Pause / Resume

        /// <summary>Pauses all game logic (time scale = 0).</summary>
        public void PauseGame()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            Debug.Log("[GameManager] Game paused.");
        }

        /// <summary>Resumes game logic after a pause.</summary>
        public void ResumeGame()
        {
            IsPaused = false;
            Time.timeScale = 1f;
            Debug.Log("[GameManager] Game resumed.");
        }

        #endregion

        #region Save

        /// <summary>Manually triggers a game save.</summary>
        public void SaveGame()
        {
            SaveData data = CollectSaveData();
            _saveManager?.Save(data);
            Debug.Log("[GameManager] Game saved.");
        }

        private void ApplySaveData(SaveData saveData)
        {
            _currencyManager?.SetMoney(saveData.money);
            _prestigeMultiplier = saveData.prestigeMultiplier;

            if (_businesses == null) return;

            for (int i = 0; i < _businesses.Length; i++)
            {
                if (i < saveData.businessLevels.Length)
                    _businesses[i].SetLevel(saveData.businessLevels[i]);

                if (i < saveData.managersHired.Length)
                    _businesses[i].SetManager(saveData.managersHired[i]);

                _businesses[i].SetPrestigeMultiplier(_prestigeMultiplier);
            }
        }

        private SaveData CollectSaveData()
        {
            int[] levels = new int[_businesses != null ? _businesses.Length : 0];
            bool[] managers = new bool[_businesses != null ? _businesses.Length : 0];

            if (_businesses != null)
            {
                for (int i = 0; i < _businesses.Length; i++)
                {
                    levels[i] = _businesses[i].Level;
                    managers[i] = _businesses[i].HasManager;
                }
            }

            bool[] upgrades = _upgradeManager != null
                ? _upgradeManager.GetPurchasedArray()
                : Array.Empty<bool>();

            return new SaveData
            {
                money = _currencyManager?.GetMoney() ?? 0,
                businessLevels = levels,
                managersHired = managers,
                upgradesPurchased = upgrades,
                prestigeMultiplier = _prestigeMultiplier,
                lastSaveTime = DateTime.UtcNow.ToString("O")
            };
        }

        #endregion

        #region Prestige

        /// <summary>
        /// Performs a full prestige reset: resets all businesses and money, clears managers and upgrades,
        /// increments the prestige multiplier by <c>_prestigeBonusPerReset</c>, and saves.
        /// </summary>
        public void PrestigeReset()
        {
            _prestigeMultiplier += _prestigeBonusPerReset;

            if (_businesses != null)
            {
                foreach (var business in _businesses)
                {
                    business.ResetMultiplier();
                    business.SetLevel(0);
                    business.SetManager(false);
                    business.SetPrestigeMultiplier(_prestigeMultiplier);
                }
            }

            _upgradeManager?.ResetUpgrades();
            _managerController?.ResetManagers();
            _currencyManager?.SetMoney(0);
            SaveGame();

            Debug.Log($"[GameManager] Prestige reset performed. New multiplier: {_prestigeMultiplier}");
        }

        /// <summary>
        /// Performs a prestige reset: resets all businesses and money, then applies a new prestige multiplier.
        /// </summary>
        /// <param name="newMultiplier">The new prestige multiplier to apply.</param>
        public void PerformPrestige(float newMultiplier)
        {
            _prestigeMultiplier = newMultiplier;

            if (_businesses != null)
            {
                foreach (var business in _businesses)
                {
                    business.SetLevel(0);
                    business.SetPrestigeMultiplier(_prestigeMultiplier);
                }
            }

            _currencyManager?.SetMoney(0);
            SaveGame();

            Debug.Log($"[GameManager] Prestige performed. New multiplier: {newMultiplier}");
        }

        #endregion
    }
}

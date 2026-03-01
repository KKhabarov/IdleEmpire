using System;
using System.Collections;
using UnityEngine;
using IdleEmpire.Business;
using IdleEmpire.Utils;

namespace IdleEmpire.Core
{
    /// <summary>
    /// Main game controller. Singleton MonoBehaviour that manages game initialization,
    /// pause/resume, offline earnings, and periodic auto-save.
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
            InitializeGame();
        }

        #endregion

        #region Inspector Fields

        [Header("Managers")]
        [SerializeField] private CurrencyManager _currencyManager;
        [SerializeField] private SaveManager _saveManager;
        [SerializeField] private IdleEmpire.Upgrades.UpgradeManager _upgradeManager;

        [Header("Businesses")]
        [SerializeField] private BusinessController[] _businesses;

        [Header("Settings")]
        [SerializeField] private float _autoSaveInterval = 60f;
        [SerializeField] private int _maxOfflineHours = 8;

        #endregion

        #region Private Fields

        private float _prestigeMultiplier = 1f;

        #endregion

        #region Properties

        /// <summary>Gets the CurrencyManager instance.</summary>
        public CurrencyManager CurrencyManager => _currencyManager;

        /// <summary>Gets the SaveManager instance.</summary>
        public SaveManager SaveManager => _saveManager;

        /// <summary>Whether the game is currently paused.</summary>
        public bool IsPaused { get; private set; }

        #endregion

        #region Initialization

        private void InitializeGame()
        {
            // Load saved game state
            SaveData saveData = _saveManager.Load();
            ApplySaveData(saveData);

            // Calculate and apply offline earnings
            double offlineEarnings = OfflineCalculator.CalculateOfflineEarnings(
                saveData.lastSaveTimestamp,
                _businesses,
                _maxOfflineHours
            );

            if (offlineEarnings > 0)
            {
                _currencyManager.AddMoney(offlineEarnings);
                Debug.Log($"[GameManager] Offline earnings applied: {NumberFormatter.FormatNumber(offlineEarnings)}");
            }

            // Start auto-save coroutine
            StartCoroutine(AutoSaveCoroutine());
        }

        private void ApplySaveData(SaveData saveData)
        {
            _currencyManager.SetMoney(saveData.money);
            _prestigeMultiplier = saveData.prestigeMultiplier;

            if (_businesses == null) return;

            for (int i = 0; i < _businesses.Length; i++)
            {
                if (i < saveData.businessLevels.Length)
                    _businesses[i].SetLevel(saveData.businessLevels[i]);

                if (i < saveData.managerStates.Length)
                    _businesses[i].SetManager(saveData.managerStates[i]);

                _businesses[i].SetPrestigeMultiplier(saveData.prestigeMultiplier);
            }
        }

        #endregion

        #region Auto Save

        private IEnumerator AutoSaveCoroutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(_autoSaveInterval);
                SaveGame();
            }
        }

        /// <summary>Manually triggers a game save.</summary>
        public void SaveGame()
        {
            SaveData data = CollectSaveData();
            _saveManager.Save(data);
            Debug.Log("[GameManager] Game auto-saved.");
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

            return new SaveData
            {
                money = _currencyManager.GetMoney(),
                businessLevels = levels,
                managerStates = managers,
                lastSaveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                prestigeMultiplier = _prestigeMultiplier,
                purchasedUpgradeIndices = _upgradeManager != null
                    ? _upgradeManager.GetPurchasedIndices()
                    : Array.Empty<int>()
            };
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

        #region Unity Callbacks

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

        #region Prestige

        /// <summary>
        /// Performs a prestige reset: resets all businesses and money, then applies a new prestige multiplier.
        /// </summary>
        /// <param name="newMultiplier">The new prestige multiplier to apply.</param>
        public void PerformPrestige(float newMultiplier)
        {
            _prestigeMultiplier = newMultiplier;

            // Reset businesses
            if (_businesses != null)
            {
                foreach (var business in _businesses)
                {
                    business.SetLevel(0);
                    business.SetPrestigeMultiplier(newMultiplier);
                }
            }

            _currencyManager.SetMoney(0);

            SaveGame();

            Debug.Log($"[GameManager] Prestige performed. New multiplier: {newMultiplier}");
        }

        #endregion
    }
}

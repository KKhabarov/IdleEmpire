using System;
using System.Collections.Generic;
using UnityEngine;
using IdleEmpire.Core;
using IdleEmpire.Business;
using IdleEmpire.Managers;
using IdleEmpire.Upgrades;
using IdleEmpire.Audio;

namespace IdleEmpire.Achievements
{
    /// <summary>
    /// MonoBehaviour that tracks player progress and unlocks achievements when conditions are met.
    /// Persists unlock state, lifetime earnings, and prestige count via <see cref="SaveData"/>.
    /// </summary>
    public class AchievementManager : MonoBehaviour
    {
        #region Events

        /// <summary>Fired when an achievement is unlocked for the first time. Passes the unlocked data.</summary>
        public event Action<AchievementData> OnAchievementUnlocked;

        #endregion

        #region Inspector Fields

        [Header("Configuration")]
        [Tooltip("All achievement definitions to track.")]
        [SerializeField] private AchievementData[] _allAchievements;

        [Tooltip("All business controllers in the scene (used for level/owned checks).")]
        [SerializeField] private BusinessController[] _businesses;

        [Header("References")]
        [Tooltip("ManagerController for hired-manager counts.")]
        [SerializeField] private ManagerController _managerController;

        [Tooltip("UpgradeManager for purchased-upgrade counts.")]
        [SerializeField] private UpgradeManager _upgradeManager;

        [Tooltip("AchievementNotification for displaying unlock popups.")]
        [SerializeField] private AchievementNotification _notification;

        #endregion

        #region Private Fields

        private readonly HashSet<int> _unlockedIndices = new HashSet<int>();
        private double _totalMoneyEarned;
        private int _prestigeCount;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            // Restore persisted state from save data.
            SaveData save = GameManager.Instance?.SaveManager?.Load();
            if (save != null)
            {
                _totalMoneyEarned = save.totalMoneyEarned;
                _prestigeCount    = save.prestigeCount;

                if (save.unlockedAchievementIndices != null)
                {
                    foreach (int idx in save.unlockedAchievementIndices)
                        _unlockedIndices.Add(idx);
                }
            }
        }

        private void OnEnable()
        {
            var currency = CurrencyManager.Instance;
            if (currency != null)
                currency.OnMoneyAdded += TrackMoneyEarned;

            if (_businesses != null)
            {
                foreach (var business in _businesses)
                {
                    if (business != null)
                        business.OnLevelChanged += OnBusinessLevelChanged;
                }
            }

            if (_managerController != null)
                _managerController.OnManagersChanged += OnManagersChanged;

            if (_upgradeManager != null)
                _upgradeManager.OnUpgradesChanged += OnUpgradesChanged;

            if (GameManager.Instance != null)
                GameManager.Instance.OnPrestigeReset += OnPrestigeReset;
        }

        private void OnDisable()
        {
            var currency = CurrencyManager.Instance;
            if (currency != null)
                currency.OnMoneyAdded -= TrackMoneyEarned;

            if (_businesses != null)
            {
                foreach (var business in _businesses)
                {
                    if (business != null)
                        business.OnLevelChanged -= OnBusinessLevelChanged;
                }
            }

            if (_managerController != null)
                _managerController.OnManagersChanged -= OnManagersChanged;

            if (_upgradeManager != null)
                _upgradeManager.OnUpgradesChanged -= OnUpgradesChanged;

            if (GameManager.Instance != null)
                GameManager.Instance.OnPrestigeReset -= OnPrestigeReset;
        }

        #endregion

        #region Public API

        /// <summary>Returns <c>true</c> if the achievement at <paramref name="index"/> is unlocked.</summary>
        public bool IsUnlocked(int index) => _unlockedIndices.Contains(index);

        /// <summary>Returns the total number of unlocked achievements.</summary>
        public int GetUnlockedCount() => _unlockedIndices.Count;

        /// <summary>Returns the total number of achievements.</summary>
        public int GetTotalCount() => _allAchievements?.Length ?? 0;

        /// <summary>Returns all achievement data entries.</summary>
        public AchievementData[] GetAllAchievements() => _allAchievements ?? Array.Empty<AchievementData>();

        /// <summary>
        /// Returns the unlock progress for the achievement at <paramref name="index"/> as a
        /// value between 0.0 (not started) and 1.0 (completed/unlocked).
        /// </summary>
        public float GetProgress(int index)
        {
            if (_allAchievements == null || index < 0 || index >= _allAchievements.Length)
                return 0f;
            if (_unlockedIndices.Contains(index))
                return 1f;

            AchievementData data = _allAchievements[index];
            double current = GetCurrentValue(data);
            return (float)Math.Min(1.0, current / Math.Max(1.0, data.TargetValue));
        }

        /// <summary>
        /// Evaluates every achievement and unlocks any whose conditions are now satisfied.
        /// </summary>
        public void CheckAllAchievements()
        {
            if (_allAchievements == null) return;

            for (int i = 0; i < _allAchievements.Length; i++)
            {
                if (_unlockedIndices.Contains(i)) continue;
                if (_allAchievements[i] == null) continue;

                double current = GetCurrentValue(_allAchievements[i]);
                if (current >= _allAchievements[i].TargetValue)
                    UnlockAchievement(i);
            }
        }

        #endregion

        #region Private Helpers

        private void TrackMoneyEarned(double amount)
        {
            _totalMoneyEarned += amount;
            CheckAllAchievements();
        }

        private void OnBusinessLevelChanged(BusinessController business)
        {
            CheckAllAchievements();
        }

        private void OnManagersChanged()
        {
            CheckAllAchievements();
        }

        private void OnUpgradesChanged()
        {
            CheckAllAchievements();
        }

        private void OnPrestigeReset()
        {
            _prestigeCount++;
            CheckAllAchievements();
        }

        private void UnlockAchievement(int index)
        {
            if (index < 0 || _allAchievements == null || index >= _allAchievements.Length) return;
            if (_unlockedIndices.Contains(index)) return;

            _unlockedIndices.Add(index);
            AchievementData data = _allAchievements[index];

            // Grant money reward if applicable.
            if (data.RewardMoney > 0)
                CurrencyManager.Instance?.AddMoney(data.RewardMoney);

            AudioManager.Instance?.PlayAchievement();
            _notification?.Show(data);
            OnAchievementUnlocked?.Invoke(data);

            Debug.Log($"[AchievementManager] Achievement unlocked: '{data.AchievementName}'");
        }

        private double GetCurrentValue(AchievementData data)
        {
            switch (data.Type)
            {
                case AchievementType.TotalMoneyEarned:
                    return _totalMoneyEarned;

                case AchievementType.PrestigeCount:
                    return _prestigeCount;

                case AchievementType.IncomePerSecond:
                    return CurrencyManager.Instance?.GetIncomePerSecond() ?? 0;

                case AchievementType.BusinessLevel:
                {
                    if (_businesses == null) return 0;
                    double max = 0;
                    foreach (var b in _businesses)
                        if (b != null && b.Level > max) max = b.Level;
                    return max;
                }

                case AchievementType.TotalBusinessLevel:
                {
                    if (_businesses == null) return 0;
                    double total = 0;
                    foreach (var b in _businesses)
                        if (b != null) total += b.Level;
                    return total;
                }

                case AchievementType.BusinessOwned:
                {
                    if (_businesses == null) return 0;
                    int owned = 0;
                    foreach (var b in _businesses)
                        if (b != null && b.Level > 0) owned++;
                    return owned;
                }

                case AchievementType.SpecificBusinessLevel:
                {
                    int idx = data.TargetBusinessIndex;
                    if (_businesses == null || idx < 0 || idx >= _businesses.Length) return 0;
                    return _businesses[idx]?.Level ?? 0;
                }

                case AchievementType.ManagersHired:
                    return _managerController?.GetAllManagers() != null
                        ? CountHiredManagers()
                        : 0;

                case AchievementType.UpgradesPurchased:
                    return _upgradeManager?.GetPurchasedIndices()?.Length ?? 0;

                default:
                    return 0;
            }
        }

        private int CountHiredManagers()
        {
            if (_managerController == null) return 0;
            ManagerData[] all = _managerController.GetAllManagers();
            int count = 0;
            for (int i = 0; i < all.Length; i++)
            {
                if (_managerController.IsManagerHired(i))
                    count++;
            }
            return count;
        }

        #endregion
    }
}

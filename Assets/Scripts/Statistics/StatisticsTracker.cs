using System;
using UnityEngine;
using IdleEmpire.Core;
using IdleEmpire.Business;
using IdleEmpire.Managers;
using IdleEmpire.Upgrades;

namespace IdleEmpire.Statistics
{
    /// <summary>
    /// MonoBehaviour that tracks and persists lifetime gameplay statistics.
    /// All stats survive prestige resets and are saved to <see cref="SaveData"/>.
    /// </summary>
    public class StatisticsTracker : MonoBehaviour
    {
        #region Inspector Fields

        [Header("References")]
        [Tooltip("Business controllers used to count level-ups and total levels.")]
        [SerializeField] private BusinessController[] _businesses;

        [Tooltip("ManagerController for manager-hire tracking.")]
        [SerializeField] private ManagerController _managerController;

        [Tooltip("UpgradeManager for upgrade-purchase tracking.")]
        [SerializeField] private UpgradeManager _upgradeManager;

        #endregion

        #region Private Fields

        private double _totalMoneyEarned;
        private double _totalMoneySpent;
        private int    _totalBusinessesPurchased;
        private int    _totalUpgradesPurchased;
        private int    _totalManagersHired;
        private int    _totalPrestigeResets;
        private int    _totalIncomeCollections;
        private double _highestIncomePerSecond;
        private float  _totalPlayTimeSeconds;

        /// <summary>Set to <c>true</c> after save data is restored, to avoid counting load-time events.</summary>
        private bool _loaded;

        #endregion

        #region Properties

        /// <summary>Total money earned across all sessions (lifetime earnings).</summary>
        public double TotalMoneyEarned => _totalMoneyEarned;

        /// <summary>Total money spent across all sessions.</summary>
        public double TotalMoneySpent => _totalMoneySpent;

        /// <summary>Total number of business level-ups performed.</summary>
        public int TotalBusinessesPurchased => _totalBusinessesPurchased;

        /// <summary>Total upgrades purchased across all sessions.</summary>
        public int TotalUpgradesPurchased => _totalUpgradesPurchased;

        /// <summary>Total managers hired across all sessions.</summary>
        public int TotalManagersHired => _totalManagersHired;

        /// <summary>Total prestige resets performed.</summary>
        public int TotalPrestigeResets => _totalPrestigeResets;

        /// <summary>Total manual income collections performed.</summary>
        public int TotalIncomeCollections => _totalIncomeCollections;

        /// <summary>Highest income per second ever achieved.</summary>
        public double HighestIncomePerSecond => _highestIncomePerSecond;

        /// <summary>Total cumulative play time in seconds.</summary>
        public float TotalPlayTimeSeconds => _totalPlayTimeSeconds;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            SaveData save = GameManager.Instance?.SaveManager?.Load();
            if (save != null)
            {
                _totalMoneyEarned        = save.totalMoneyEarned;
                _totalMoneySpent         = save.totalMoneySpent;
                _totalBusinessesPurchased = save.totalBusinessesPurchased;
                _totalIncomeCollections  = save.totalIncomeCollections;
                _highestIncomePerSecond  = save.highestIncomePerSecond;
                _totalPlayTimeSeconds    = save.totalPlayTimeSeconds;
                _totalPrestigeResets     = save.prestigeCount;

                // Derive from existing save arrays where possible.
                if (save.upgradesPurchased != null)
                {
                    int count = 0;
                    foreach (bool b in save.upgradesPurchased)
                        if (b) count++;
                    _totalUpgradesPurchased = count;
                }

                if (save.managersHired != null)
                {
                    int count = 0;
                    foreach (bool b in save.managersHired)
                        if (b) count++;
                    _totalManagersHired = count;
                }
            }

            _loaded = true;
        }

        private void Update()
        {
            _totalPlayTimeSeconds += Time.deltaTime;

            // Periodically check if a new income-per-second record was set.
            double ips = CurrencyManager.Instance?.GetIncomePerSecond() ?? 0;
            if (ips > _highestIncomePerSecond)
                _highestIncomePerSecond = ips;
        }

        private void OnEnable()
        {
            var currency = CurrencyManager.Instance;
            if (currency != null)
            {
                currency.OnMoneyAdded += OnMoneyAdded;
                currency.OnMoneySpent += OnMoneySpent;
            }

            if (_businesses != null)
            {
                foreach (var b in _businesses)
                {
                    if (b != null)
                    {
                        b.OnIncomeCollected += OnIncomeCollected;
                        b.OnLevelChanged    += OnBusinessLevelChanged;
                    }
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
            {
                currency.OnMoneyAdded -= OnMoneyAdded;
                currency.OnMoneySpent -= OnMoneySpent;
            }

            if (_businesses != null)
            {
                foreach (var b in _businesses)
                {
                    if (b != null)
                    {
                        b.OnIncomeCollected -= OnIncomeCollected;
                        b.OnLevelChanged    -= OnBusinessLevelChanged;
                    }
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

        /// <summary>
        /// Returns a human-readable string of total play time, e.g. "2h 35m 12s".
        /// </summary>
        public string GetFormattedPlayTime()
        {
            int total   = (int)_totalPlayTimeSeconds;
            int hours   = total / 3600;
            int minutes = (total % 3600) / 60;
            int seconds = total % 60;

            if (hours > 0)
                return $"{hours}h {minutes}m {seconds}s";
            if (minutes > 0)
                return $"{minutes}m {seconds}s";
            return $"{seconds}s";
        }

        /// <summary>
        /// Writes all tracked statistics into the provided <paramref name="save"/> object so they
        /// can be persisted by <see cref="Core.GameManager"/>.
        /// </summary>
        public void PopulateSaveData(SaveData save)
        {
            if (save == null) return;
            save.totalMoneyEarned        = _totalMoneyEarned;
            save.totalMoneySpent         = _totalMoneySpent;
            save.totalBusinessesPurchased = _totalBusinessesPurchased;
            save.totalIncomeCollections  = _totalIncomeCollections;
            save.highestIncomePerSecond  = _highestIncomePerSecond;
            save.totalPlayTimeSeconds    = _totalPlayTimeSeconds;
            save.prestigeCount           = _totalPrestigeResets;
        }

        #endregion

        #region Event Handlers

        private void OnMoneyAdded(double amount)
        {
            _totalMoneyEarned += amount;
        }

        private void OnMoneySpent(double amount)
        {
            _totalMoneySpent += amount;
        }

        private void OnBusinessLevelChanged(BusinessController business)
        {
            // Only count genuine level-ups that happen after initialisation.
            if (!_loaded || business == null || business.Level <= 0) return;
            _totalBusinessesPurchased++;
        }

        private void OnIncomeCollected(BusinessController business, double amount)
        {
            if (!_loaded) return;
            _totalIncomeCollections++;
        }

        private void OnManagersChanged()
        {
            if (!_loaded) return;
            _totalManagersHired++;
        }

        private void OnUpgradesChanged()
        {
            if (!_loaded) return;
            _totalUpgradesPurchased++;
        }

        private void OnPrestigeReset()
        {
            _totalPrestigeResets++;
        }

        #endregion
    }
}

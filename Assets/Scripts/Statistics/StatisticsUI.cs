using UnityEngine;
using TMPro;
using IdleEmpire.Core;
using IdleEmpire.Utils;

namespace IdleEmpire.Statistics
{
    /// <summary>
    /// UI panel that displays all tracked gameplay statistics.
    /// Refreshes automatically when the panel becomes active and updates play time every second.
    /// </summary>
    public class StatisticsUI : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Text Fields")]
        [Tooltip("Total lifetime money earned.")]
        [SerializeField] private TextMeshProUGUI _totalEarnedText;

        [Tooltip("Total lifetime money spent.")]
        [SerializeField] private TextMeshProUGUI _totalSpentText;

        [Tooltip("Current money balance.")]
        [SerializeField] private TextMeshProUGUI _currentMoneyText;

        [Tooltip("Current income per second.")]
        [SerializeField] private TextMeshProUGUI _currentIncomeText;

        [Tooltip("Highest income per second ever achieved.")]
        [SerializeField] private TextMeshProUGUI _highestIncomeText;

        [Tooltip("Total sum of all business levels purchased.")]
        [SerializeField] private TextMeshProUGUI _businessLevelsText;

        [Tooltip("Total upgrades purchased.")]
        [SerializeField] private TextMeshProUGUI _upgradesText;

        [Tooltip("Total managers hired.")]
        [SerializeField] private TextMeshProUGUI _managersText;

        [Tooltip("Total prestige resets performed.")]
        [SerializeField] private TextMeshProUGUI _prestigeText;

        [Tooltip("Total manual income collections.")]
        [SerializeField] private TextMeshProUGUI _collectionsText;

        [Tooltip("Total cumulative play time.")]
        [SerializeField] private TextMeshProUGUI _playTimeText;

        [Tooltip("Current active prestige multiplier.")]
        [SerializeField] private TextMeshProUGUI _prestigeMultiplierText;

        [Header("References")]
        [Tooltip("StatisticsTracker providing all stat data.")]
        [SerializeField] private StatisticsTracker _tracker;

        #endregion

        #region Private Fields

        private float _playTimeRefreshTimer;
        private const float PlayTimeRefreshInterval = 1f;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            RefreshAll();
        }

        private void Update()
        {
            // Refresh play-time text every second without updating all fields.
            _playTimeRefreshTimer += Time.deltaTime;
            if (_playTimeRefreshTimer >= PlayTimeRefreshInterval)
            {
                _playTimeRefreshTimer = 0f;
                if (_playTimeText != null && _tracker != null)
                    _playTimeText.text = $"Play Time: {_tracker.GetFormattedPlayTime()}";
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Updates all stat text fields with current values from <see cref="StatisticsTracker"/>
        /// and <see cref="CurrencyManager"/>.
        /// </summary>
        public void RefreshAll()
        {
            if (_tracker == null) return;

            var currency = CurrencyManager.Instance;

            SetText(_totalEarnedText,       $"Total Earned: {NumberFormatter.FormatNumber(_tracker.TotalMoneyEarned)}");
            SetText(_totalSpentText,        $"Total Spent: {NumberFormatter.FormatNumber(_tracker.TotalMoneySpent)}");
            SetText(_currentMoneyText,      $"Current Money: {NumberFormatter.FormatNumber(currency?.GetMoney() ?? 0)}");
            SetText(_currentIncomeText,     $"Income/sec: {NumberFormatter.FormatNumber(currency?.GetIncomePerSecond() ?? 0)}");
            SetText(_highestIncomeText,     $"Best Income/sec: {NumberFormatter.FormatNumber(_tracker.HighestIncomePerSecond)}");
            SetText(_businessLevelsText,    $"Businesses Purchased: {_tracker.TotalBusinessesPurchased}");
            SetText(_upgradesText,          $"Upgrades Purchased: {_tracker.TotalUpgradesPurchased}");
            SetText(_managersText,          $"Managers Hired: {_tracker.TotalManagersHired}");
            SetText(_prestigeText,          $"Prestige Resets: {_tracker.TotalPrestigeResets}");
            SetText(_collectionsText,       $"Income Collections: {_tracker.TotalIncomeCollections}");
            SetText(_playTimeText,          $"Play Time: {_tracker.GetFormattedPlayTime()}");

            // Prestige multiplier comes from GameManager if available.
            // We expose it via SaveData; retrieve it via a fresh load for simplicity.
            SaveData save = GameManager.Instance?.SaveManager?.Load();
            if (_prestigeMultiplierText != null)
                _prestigeMultiplierText.text = $"Prestige Multiplier: x{(save?.prestigeMultiplier ?? 1.0):F2}";
        }

        #endregion

        #region Helpers

        private static void SetText(TextMeshProUGUI label, string text)
        {
            if (label != null)
                label.text = text;
        }

        #endregion
    }
}

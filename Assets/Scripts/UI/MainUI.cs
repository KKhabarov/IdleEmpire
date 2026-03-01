using UnityEngine;
using TMPro;
using IdleEmpire.Business;
using IdleEmpire.Core;
using IdleEmpire.Utils;

namespace IdleEmpire.UI
{
    /// <summary>
    /// Main HUD controller. Displays the player's total money and total income per second.
    /// Subscribe to <see cref="CurrencyManager.OnMoneyChanged"/> to keep values current.
    /// </summary>
    public class MainUI : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Money Display")]
        [SerializeField] private TextMeshProUGUI _moneyText;
        [SerializeField] private TextMeshProUGUI _incomePerSecondText;

        [Header("References")]
        [SerializeField] private BusinessController[] _businesses;

        [Tooltip("Parent transform that holds the instantiated business card UI elements.")]
        [SerializeField] private Transform _businessListContainer;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            var currency = GameManager.Instance?.CurrencyManager;
            if (currency != null)
                currency.OnMoneyChanged += OnMoneyChanged;

            RefreshAll();
        }

        private void OnDisable()
        {
            var currency = GameManager.Instance?.CurrencyManager;
            if (currency != null)
                currency.OnMoneyChanged -= OnMoneyChanged;
        }

        #endregion

        #region UI Refresh

        private void OnMoneyChanged(double newBalance)
        {
            UpdateMoneyDisplay(newBalance);
            UpdateIncomeDisplay();
        }

        private void RefreshAll()
        {
            double money = GameManager.Instance?.CurrencyManager?.GetMoney() ?? 0;
            UpdateMoneyDisplay(money);
            UpdateIncomeDisplay();
        }

        private void UpdateMoneyDisplay(double money)
        {
            if (_moneyText != null)
                _moneyText.text = NumberFormatter.FormatNumber(money);
        }

        private void UpdateIncomeDisplay()
        {
            if (_incomePerSecondText == null || _businesses == null) return;

            double totalIPS = 0;
            foreach (var business in _businesses)
            {
                if (business != null)
                    totalIPS += business.GetIncomePerSecond();
            }

            _incomePerSecondText.text = $"{NumberFormatter.FormatNumber(totalIPS)}/s";
        }

        #endregion
    }
}

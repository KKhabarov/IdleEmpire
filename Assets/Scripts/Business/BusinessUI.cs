// Note: Requires TextMeshPro package (com.unity.textmeshpro) to be installed.
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleEmpire.Core;
using IdleEmpire.Utils;

namespace IdleEmpire.Business
{
    /// <summary>
    /// MonoBehaviour that drives the business card UI element.
    /// Displays the business name, level, income per second, upgrade cost,
    /// and a progress bar for the collection cycle.
    /// </summary>
    public class BusinessUI : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Business Reference")]
        [SerializeField] private BusinessController _controller;

        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _incomeText;
        [SerializeField] private TextMeshProUGUI _costText;

        [Header("Interactive Elements")]
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _collectButton;
        [SerializeField] private Image _progressBar;

        [Header("Visual")]
        [SerializeField] private Image _iconImage;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            if (_controller != null)
            {
                _controller.OnLevelChanged += OnLevelChanged;
                _controller.OnCycleProgress += OnCycleProgress;
            }

            var currency = GameManager.Instance?.CurrencyManager;
            if (currency != null)
                currency.OnMoneyChanged += OnMoneyChanged;

            UpdateUI();
        }

        private void OnDisable()
        {
            if (_controller != null)
            {
                _controller.OnLevelChanged -= OnLevelChanged;
                _controller.OnCycleProgress -= OnCycleProgress;
            }

            var currency = GameManager.Instance?.CurrencyManager;
            if (currency != null)
                currency.OnMoneyChanged -= OnMoneyChanged;
        }

        #endregion

        #region UI Refresh

        /// <summary>Refreshes all text fields and button states.</summary>
        public void UpdateUI()
        {
            if (_controller == null || _controller.BusinessData == null) return;

            var data = _controller.BusinessData;

            if (_nameText != null)
                _nameText.text = data.BusinessName;

            if (_levelText != null)
                _levelText.text = $"Lvl {_controller.Level}";

            if (_incomeText != null)
                _incomeText.text = $"{NumberFormatter.FormatNumber(_controller.GetIncomePerSecond())}/s";

            if (_costText != null)
                _costText.text = NumberFormatter.FormatNumber(_controller.GetCurrentCost());

            if (_iconImage != null && data.Icon != null)
                _iconImage.sprite = data.Icon;

            UpdateBuyButtonState();
            UpdateCollectButtonState();
        }

        private void OnLevelChanged(BusinessController _) => UpdateUI();

        private void OnMoneyChanged(double _) => UpdateBuyButtonState();

        private void OnCycleProgress(BusinessController _, float progress)
        {
            if (_progressBar != null)
                _progressBar.fillAmount = progress;
        }

        private void UpdateBuyButtonState()
        {
            if (_buyButton == null || _controller == null) return;
            bool canAfford = GameManager.Instance?.CurrencyManager?.CanAfford(
                _controller.GetCurrentCost()) ?? false;
            _buyButton.interactable = canAfford;
        }

        private void UpdateCollectButtonState()
        {
            if (_collectButton == null || _controller == null) return;
            // Manual collect button is only visible when no manager is hired.
            _collectButton.gameObject.SetActive(!_controller.HasManager);
        }

        #endregion

        #region Button Handlers

        /// <summary>Called by the Buy button's OnClick event.</summary>
        public void OnBuyClicked()
        {
            _controller?.Purchase();
        }

        /// <summary>Called by the Collect button's OnClick event (manual tap).</summary>
        public void OnCollectClicked()
        {
            _controller?.StartCollecting();
        }

        #endregion
    }
}

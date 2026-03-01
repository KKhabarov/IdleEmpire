using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleEmpire.Core;
using IdleEmpire.Utils;

namespace IdleEmpire.Business
{
    /// <summary>
    /// MonoBehaviour that drives the business card UI element.
    /// Displays the business name, current level, income per second, and upgrade cost.
    /// The buy button is disabled whenever the player cannot afford the next upgrade.
    /// </summary>
    public class BusinessUI : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Business Reference")]
        [SerializeField] private BusinessController _businessController;

        [Header("Text Elements")]
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _incomeText;
        [SerializeField] private TextMeshProUGUI _costText;

        [Header("Interactive Elements")]
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _collectButton;
        [SerializeField] private Slider _progressBar;

        [Header("Visual")]
        [SerializeField] private Image _iconImage;

        #endregion

        #region Private Fields

        private float _collectionTimer;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            if (_businessController != null)
                _businessController.OnBusinessChanged += RefreshUI;

            if (GameManager.Instance?.CurrencyManager != null)
                GameManager.Instance.CurrencyManager.OnMoneyChanged += OnMoneyChanged;

            RefreshUI();
        }

        private void OnDisable()
        {
            if (_businessController != null)
                _businessController.OnBusinessChanged -= RefreshUI;

            if (GameManager.Instance?.CurrencyManager != null)
                GameManager.Instance.CurrencyManager.OnMoneyChanged -= OnMoneyChanged;
        }

        private void Update()
        {
            UpdateProgressBar();
        }

        #endregion

        #region UI Refresh

        private void RefreshUI()
        {
            if (_businessController == null || _businessController.Data == null) return;

            var data = _businessController.Data;

            if (_nameText != null)
                _nameText.text = data.BusinessName;

            if (_levelText != null)
                _levelText.text = $"Lvl {_businessController.Level}";

            if (_incomeText != null)
            {
                double ips = _businessController.CalculateIncomePerSecond();
                _incomeText.text = $"{NumberFormatter.FormatNumber(ips)}/s";
            }

            if (_costText != null)
            {
                double cost = _businessController.CalculateCost();
                _costText.text = NumberFormatter.FormatNumber(cost);
            }

            if (_iconImage != null && data.Icon != null)
                _iconImage.sprite = data.Icon;

            UpdateBuyButtonState();
            UpdateCollectButtonState();
        }

        private void OnMoneyChanged(double _)
        {
            // Only need to refresh affordability on money changes.
            UpdateBuyButtonState();
        }

        private void UpdateBuyButtonState()
        {
            if (_buyButton == null || _businessController == null) return;

            bool canAfford = GameManager.Instance?.CurrencyManager?.CanAfford(
                _businessController.CalculateCost()) ?? false;

            _buyButton.interactable = canAfford;
        }

        private void UpdateCollectButtonState()
        {
            if (_collectButton == null) return;
            // Manual collect only available when no manager is hired.
            _collectButton.gameObject.SetActive(!_businessController.HasManager);
        }

        #endregion

        #region Progress Bar

        private void UpdateProgressBar()
        {
            if (_progressBar == null || _businessController == null || _businessController.Data == null)
                return;

            float interval = _businessController.Data.CollectionInterval;
            if (interval <= 0f) return;

            _collectionTimer += Time.deltaTime;
            if (_collectionTimer >= interval)
                _collectionTimer = 0f;

            _progressBar.value = _collectionTimer / interval;
        }

        #endregion

        #region Button Handlers

        /// <summary>Called by the Buy button's OnClick event.</summary>
        public void OnBuyButtonClicked()
        {
            _businessController?.Purchase();
        }

        /// <summary>Called by the Collect button's OnClick event (manual tap).</summary>
        public void OnCollectButtonClicked()
        {
            _businessController?.CollectIncome();
            _collectionTimer = 0f;
        }

        #endregion
    }
}

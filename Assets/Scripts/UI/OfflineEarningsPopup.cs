using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleEmpire.Core;
using IdleEmpire.Ads;
using IdleEmpire.Utils;

namespace IdleEmpire.UI
{
    /// <summary>
    /// Popup shown when the player returns to the game after being offline.
    /// Lets the player claim their offline earnings directly or double them by watching a rewarded ad.
    /// </summary>
    public class OfflineEarningsPopup : MonoBehaviour
    {
        #region Inspector Fields

        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI _earningsText;
        [SerializeField] private Button _claimButton;
        [SerializeField] private Button _doubleButton;
        [SerializeField] private GameObject _popupPanel;

        #endregion

        #region Private Fields

        private double _pendingEarnings;
        private AdManager _subscribedAdManager;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _claimButton?.onClick.AddListener(OnClaimClicked);
            _doubleButton?.onClick.AddListener(OnDoubleClicked);

            if (_popupPanel != null)
                _popupPanel.SetActive(false);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Displays the popup with the amount earned while offline.
        /// </summary>
        /// <param name="earnings">Total earnings accumulated during offline time.</param>
        public void Show(double earnings)
        {
            _pendingEarnings = earnings;

            if (_earningsText != null)
                _earningsText.text = $"You earned {NumberFormatter.FormatNumber(earnings)} while away!";

            if (_popupPanel != null)
                _popupPanel.SetActive(true);
        }

        #endregion

        #region Button Handlers

        /// <summary>Claims the offline earnings at face value and hides the popup.</summary>
        private void OnClaimClicked()
        {
            GameManager.Instance?.CurrencyManager?.AddMoney(_pendingEarnings);
            Hide();
        }

        /// <summary>Shows a rewarded ad; on success, awards 2× the offline earnings.</summary>
        private void OnDoubleClicked()
        {
            if (AdManager.Instance != null)
            {
                _subscribedAdManager = AdManager.Instance;
                _subscribedAdManager.OnRewardClaimed += HandleRewardClaimed;
                _subscribedAdManager.ShowRewardedAd();
            }
            else
            {
                // Fallback: just claim without doubling if AdManager is unavailable.
                OnClaimClicked();
            }
        }

        #endregion

        #region Helpers

        private void HandleRewardClaimed(double multiplier)
        {
            // Unsubscribe using the stored reference, not Instance (which may have changed).
            if (_subscribedAdManager != null)
            {
                _subscribedAdManager.OnRewardClaimed -= HandleRewardClaimed;
                _subscribedAdManager = null;
            }

            GameManager.Instance?.CurrencyManager?.AddMoney(_pendingEarnings * multiplier);
            Hide();
        }

        private void Hide()
        {
            if (_popupPanel != null)
                _popupPanel.SetActive(false);
        }

        #endregion
    }
}

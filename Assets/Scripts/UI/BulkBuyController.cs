using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using IdleEmpire.Business;
using IdleEmpire.Core;

namespace IdleEmpire.UI
{
    /// <summary>Bulk buy mode for purchasing multiple business levels at once.</summary>
    public enum BulkBuyMode
    {
        Buy1   =  1,
        Buy10  = 10,
        Buy25  = 25,
        BuyMax = -1   // Buy as many as can afford
    }

    /// <summary>
    /// Controls the bulk buy mode toggle (x1 / x10 / x25 / Max).
    /// Updates all BusinessUI cards to show the correct cost and buy amount.
    /// </summary>
    public class BulkBuyController : MonoBehaviour
    {
        #region Events

        /// <summary>Fired whenever the active bulk-buy mode changes.</summary>
        public event Action<BulkBuyMode> OnBulkModeChanged;

        #endregion

        #region Inspector Fields

        [Header("Buttons")]
        [SerializeField] private Button _buy1Button;
        [SerializeField] private Button _buy10Button;
        [SerializeField] private Button _buy25Button;
        [SerializeField] private Button _buyMaxButton;

        [Header("Visual")]
        [SerializeField] private Color _activeColor   = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color _inactiveColor = Color.white;

        #endregion

        #region Private Fields

        private BulkBuyMode _currentMode = BulkBuyMode.Buy1;

        #endregion

        #region Properties

        /// <summary>The currently active bulk-buy mode.</summary>
        public BulkBuyMode CurrentMode => _currentMode;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            if (_buy1Button   != null) _buy1Button.onClick.AddListener(() => SetMode(BulkBuyMode.Buy1));
            if (_buy10Button  != null) _buy10Button.onClick.AddListener(() => SetMode(BulkBuyMode.Buy10));
            if (_buy25Button  != null) _buy25Button.onClick.AddListener(() => SetMode(BulkBuyMode.Buy25));
            if (_buyMaxButton != null) _buyMaxButton.onClick.AddListener(() => SetMode(BulkBuyMode.BuyMax));
        }

        private void Start()
        {
            HighlightActiveButton();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the active bulk-buy mode, highlights the corresponding button, and fires the change event.
        /// </summary>
        /// <param name="mode">The new bulk-buy mode to activate.</param>
        public void SetMode(BulkBuyMode mode)
        {
            _currentMode = mode;
            HighlightActiveButton();
            OnBulkModeChanged?.Invoke(_currentMode);
        }

        /// <summary>
        /// Calculates the total cost to purchase <paramref name="count"/> additional levels
        /// of <paramref name="business"/> starting from its current level.
        /// </summary>
        /// <param name="business">The business to calculate cost for.</param>
        /// <param name="count">Number of additional levels to buy.</param>
        /// <returns>Total cost for the requested levels.</returns>
        public double GetBulkCost(BusinessController business, int count)
        {
            if (business == null || count <= 0) return 0;

            double total = 0;
            int    baseLevel = business.Level;

            for (int i = 0; i < count; i++)
            {
                total += business.BusinessData != null
                    ? business.BusinessData.GetCostForLevel(baseLevel + i)
                    : 0;
            }

            return total;
        }

        /// <summary>
        /// Returns how many additional levels of <paramref name="business"/> can be afforded
        /// with <paramref name="money"/>.
        /// </summary>
        /// <param name="business">The business to evaluate.</param>
        /// <param name="money">Available currency.</param>
        /// <returns>Maximum number of levels purchasable (may be 0).</returns>
        public int GetMaxAffordable(BusinessController business, double money)
        {
            if (business == null || money <= 0) return 0;

            int    count = 0;
            double spent = 0;
            int    baseLevel = business.Level;

            while (true)
            {
                double cost = business.BusinessData != null
                    ? business.BusinessData.GetCostForLevel(baseLevel + count)
                    : double.MaxValue;

                if (spent + cost > money) break;
                spent += cost;
                count++;
            }

            return count;
        }

        #endregion

        #region Helpers

        private void HighlightActiveButton()
        {
            SetButtonColor(_buy1Button,   _currentMode == BulkBuyMode.Buy1);
            SetButtonColor(_buy10Button,  _currentMode == BulkBuyMode.Buy10);
            SetButtonColor(_buy25Button,  _currentMode == BulkBuyMode.Buy25);
            SetButtonColor(_buyMaxButton, _currentMode == BulkBuyMode.BuyMax);
        }

        private void SetButtonColor(Button button, bool isActive)
        {
            if (button == null) return;
            Image img = button.GetComponent<Image>();
            if (img != null)
                img.color = isActive ? _activeColor : _inactiveColor;
        }

        #endregion
    }
}

using System;
using UnityEngine;
using IdleEmpire.Core;
using IdleEmpire.Audio;

namespace IdleEmpire.Business
{
    /// <summary>
    /// MonoBehaviour attached to each business card GameObject.
    /// Handles purchasing levels, calculating income/cost, and collecting income
    /// either manually (tap) or automatically when a manager is assigned.
    /// </summary>
    public class BusinessController : MonoBehaviour
    {
        #region Events

        /// <summary>Fired whenever this business's level changes.</summary>
        public event Action<BusinessController> OnLevelChanged;

        /// <summary>Fired when income is collected. Passes the controller and the amount collected.</summary>
        public event Action<BusinessController, double> OnIncomeCollected;

        /// <summary>Fired each frame with the current cycle progress (0–1). Used to drive progress bars.</summary>
        public event Action<BusinessController, float> OnCycleProgress;

        #endregion

        #region Inspector Fields

        [Header("Configuration")]
        [SerializeField] private BusinessData _businessData;

        [Header("Initial State")]
        [SerializeField] private int _level = 0;
        [SerializeField] private bool _hasManager = false;

        #endregion

        #region Private Fields

        private double _incomeMultiplier = 1.0;
        private float _cycleTimer = 0f;
        private bool _isCollecting = false;
        private double _prestigeMultiplier = 1.0;

        #endregion

        #region Properties

        /// <summary>Current upgrade level (0 = not yet purchased).</summary>
        public int Level => _level;

        /// <summary>Whether a manager has been hired for this business.</summary>
        public bool HasManager => _hasManager;

        /// <summary>Reference to this business's static configuration data.</summary>
        public BusinessData BusinessData => _businessData;

        /// <summary>Whether this business has been purchased (level &gt; 0).</summary>
        public bool IsUnlocked => _level > 0;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (_hasManager)
                _isCollecting = true;
        }

        private void Update()
        {
            if (!IsUnlocked) return;
            if (!_isCollecting && !_hasManager) return;

            _cycleTimer += Time.deltaTime;
            float duration = GetCycleDuration();
            OnCycleProgress?.Invoke(this, Mathf.Clamp01(_cycleTimer / duration));

            if (_cycleTimer >= duration)
            {
                CollectIncome();
                _cycleTimer = 0f;

                // Manual (non-manager) collection stops after one cycle.
                if (!_hasManager)
                    _isCollecting = false;
            }
        }

        #endregion

        #region Purchase & Level

        /// <summary>
        /// Attempts to purchase the next level of this business.
        /// Deducts cost from <see cref="CurrencyManager"/> if the player can afford it.
        /// </summary>
        /// <returns><c>true</c> if the purchase was successful.</returns>
        public bool Purchase()
        {
            if (_businessData == null) return false;

            var currency = GameManager.Instance?.CurrencyManager;
            if (currency == null) return false;

            if (!currency.SpendMoney(GetCurrentCost())) 
            {
                AudioManager.Instance?.PlayError();
                return false;
            }

            _level++;
            OnLevelChanged?.Invoke(this);

            if (_level % 25 == 0)
                AudioManager.Instance?.PlayLevelUp();
            else
                AudioManager.Instance?.PlayPurchase();

            Debug.Log($"[BusinessController] {_businessData.BusinessName} upgraded to level {_level}.");
            return true;
        }

        /// <summary>
        /// Directly sets the business level. Used by <see cref="GameManager"/> during load/prestige.
        /// </summary>
        /// <param name="level">New level value (0 or higher).</param>
        public void SetLevel(int level)
        {
            _level = Mathf.Max(0, level);
            OnLevelChanged?.Invoke(this);
        }

        #endregion

        #region Income Calculation

        /// <summary>
        /// Returns the cost to purchase or upgrade to the next level.
        /// </summary>
        public double GetCurrentCost()
        {
            if (_businessData == null) return double.MaxValue;
            return _businessData.GetCostForLevel(_level);
        }

        /// <summary>
        /// Returns income per collection cycle, including all active multipliers.
        /// </summary>
        public double GetCurrentIncome()
        {
            if (_businessData == null || _level <= 0) return 0;
            return _businessData.GetIncomeForLevel(_level) * _incomeMultiplier * _prestigeMultiplier;
        }

        /// <summary>
        /// Returns the income per second for this business.
        /// </summary>
        public double GetIncomePerSecond()
        {
            if (!IsUnlocked) return 0;
            return GetCurrentIncome() / GetCycleDuration();
        }

        #endregion

        #region Collection

        /// <summary>
        /// Starts the income collection cycle (manual tap or manager automation).
        /// Has no effect if collection is already in progress.
        /// </summary>
        public void StartCollecting()
        {
            if (!IsUnlocked || _isCollecting) return;
            _isCollecting = true;
        }

        private void CollectIncome()
        {
            double income = GetCurrentIncome();
            if (income <= 0) return;

            GameManager.Instance?.CurrencyManager?.AddMoney(income);
            AudioManager.Instance?.PlayCollect();
            OnIncomeCollected?.Invoke(this, income);
        }

        #endregion

        #region Manager & Multipliers

        /// <summary>
        /// Sets whether a manager is assigned to this business.
        /// When <c>true</c>, income is collected automatically every cycle.
        /// </summary>
        /// <param name="value"><c>true</c> to enable manager auto-collection.</param>
        public void SetManager(bool value)
        {
            _hasManager = value;
            if (_hasManager)
            {
                _isCollecting = true;
                _cycleTimer = 0f;
            }
        }

        /// <summary>
        /// Multiplies <c>_incomeMultiplier</c> by the given value, permanently boosting income.
        /// </summary>
        /// <param name="multiplier">Multiplicative factor to apply.</param>
        public void ApplyMultiplier(double multiplier)
        {
            _incomeMultiplier *= multiplier;
        }

        /// <summary>
        /// Resets the income multiplier back to 1.0. Used during a prestige reset.
        /// </summary>
        public void ResetMultiplier()
        {
            _incomeMultiplier = 1.0;
        }

        /// <summary>
        /// Sets the prestige multiplier applied to income calculations.
        /// </summary>
        /// <param name="multiplier">Prestige multiplier value (minimum 1.0).</param>
        public void SetPrestigeMultiplier(double multiplier)
        {
            _prestigeMultiplier = Math.Max(1.0, multiplier);
        }

        #endregion

        #region Helpers

        private float GetCycleDuration()
        {
            return _businessData != null && _businessData.CycleDuration > 0
                ? _businessData.CycleDuration
                : 1f;
        }

        #endregion
    }
}

using System;
using System.Collections;
using UnityEngine;
using IdleEmpire.Core;

namespace IdleEmpire.Business
{
    /// <summary>
    /// MonoBehaviour attached to each business game object.
    /// Handles purchasing levels, calculating income/cost, and collecting income
    /// either manually (tap) or automatically when a manager is assigned.
    /// </summary>
    public class BusinessController : MonoBehaviour
    {
        #region Events

        /// <summary>Fired whenever this business's level or state changes.</summary>
        public event Action OnBusinessChanged;

        /// <summary>Fired when income is collected. Passes the collected amount.</summary>
        public event Action<double> OnIncomeCollected;

        #endregion

        #region Inspector Fields

        [Header("Configuration")]
        [SerializeField] private BusinessData _data;

        [Header("Initial State")]
        [SerializeField] private int _level = 0;
        [SerializeField] private bool _hasManager = false;

        #endregion

        #region Private Fields

        private float _prestigeMultiplier = 1f;
        private float _upgradeMultiplier = 1f;
        private Coroutine _autoCollectCoroutine;

        #endregion

        #region Properties

        /// <summary>Reference to this business's static configuration.</summary>
        public BusinessData Data => _data;

        /// <summary>Current upgrade level (0 = not yet purchased).</summary>
        public int Level => _level;

        /// <summary>Whether a manager has been hired for this business.</summary>
        public bool HasManager => _hasManager;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (_hasManager)
                StartAutoCollect();
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
            if (_data == null) return false;

            double cost = CalculateCost();
            var currencyManager = GameManager.Instance?.CurrencyManager;

            if (currencyManager == null || !currencyManager.CanAfford(cost))
                return false;

            currencyManager.SpendMoney(cost);
            _level++;
            OnBusinessChanged?.Invoke();

            Debug.Log($"[BusinessController] {_data.BusinessName} upgraded to level {_level}.");
            return true;
        }

        /// <summary>
        /// Directly sets the business level. Used by <see cref="Core.GameManager"/> during load/prestige.
        /// </summary>
        /// <param name="level">New level value (0 or higher).</param>
        public void SetLevel(int level)
        {
            _level = Mathf.Max(0, level);
            OnBusinessChanged?.Invoke();
        }

        #endregion

        #region Income Calculation

        /// <summary>
        /// Calculates the income for one collection cycle.
        /// Formula: <c>baseIncome * level * prestigeMultiplier * upgradeMultiplier</c>
        /// </summary>
        /// <returns>Income amount (double).</returns>
        public double CalculateIncome()
        {
            if (_data == null || _level <= 0) return 0;

            return _data.BaseIncome * _level * _prestigeMultiplier * _upgradeMultiplier;
        }

        /// <summary>
        /// Returns the income per second (useful for the UI "IPS" display).
        /// </summary>
        public double CalculateIncomePerSecond()
        {
            if (_data == null || _level <= 0 || !_hasManager) return 0;

            float interval = _data.CollectionInterval > 0 ? _data.CollectionInterval : 1f;
            return CalculateIncome() / interval;
        }

        /// <summary>
        /// Calculates the cost to purchase the next level.
        /// </summary>
        /// <returns>Cost in game currency.</returns>
        public double CalculateCost()
        {
            if (_data == null) return double.MaxValue;
            return _data.CalculateCost(_level);
        }

        #endregion

        #region Income Collection

        /// <summary>
        /// Manually collects income for this business (tap/click action).
        /// Only works if the business has at least level 1.
        /// </summary>
        public void CollectIncome()
        {
            double income = CalculateIncome();
            if (income <= 0) return;

            GameManager.Instance?.CurrencyManager?.AddMoney(income);
            OnIncomeCollected?.Invoke(income);

            Debug.Log($"[BusinessController] {_data?.BusinessName} collected {income:F2}.");
        }

        private IEnumerator AutoCollectCoroutine()
        {
            float interval = _data != null && _data.CollectionInterval > 0
                ? _data.CollectionInterval
                : 5f;

            while (_hasManager)
            {
                yield return new WaitForSeconds(interval);
                CollectIncome();
            }
        }

        #endregion

        #region Manager

        /// <summary>
        /// Sets whether this business has an active manager.
        /// When <c>true</c> the auto-collect coroutine starts automatically.
        /// </summary>
        /// <param name="hasManager"><c>true</c> to activate manager automation.</param>
        public void SetManager(bool hasManager)
        {
            _hasManager = hasManager;

            if (_hasManager)
                StartAutoCollect();
            else
                StopAutoCollect();

            OnBusinessChanged?.Invoke();
        }

        private void StartAutoCollect()
        {
            if (_autoCollectCoroutine != null)
                StopCoroutine(_autoCollectCoroutine);

            _autoCollectCoroutine = StartCoroutine(AutoCollectCoroutine());
        }

        private void StopAutoCollect()
        {
            if (_autoCollectCoroutine != null)
            {
                StopCoroutine(_autoCollectCoroutine);
                _autoCollectCoroutine = null;
            }
        }

        #endregion

        #region Multipliers

        /// <summary>
        /// Sets the prestige multiplier applied to income calculations.
        /// </summary>
        /// <param name="multiplier">Multiplier value (1.0 = no bonus).</param>
        public void SetPrestigeMultiplier(float multiplier)
        {
            _prestigeMultiplier = Mathf.Max(1f, multiplier);
            OnBusinessChanged?.Invoke();
        }

        /// <summary>
        /// Applies an upgrade multiplier on top of any existing multiplier.
        /// </summary>
        /// <param name="multiplier">Multiplicative factor applied to the current upgrade multiplier.</param>
        public void ApplyUpgradeMultiplier(float multiplier)
        {
            _upgradeMultiplier *= multiplier;
            OnBusinessChanged?.Invoke();
        }

        #endregion
    }
}

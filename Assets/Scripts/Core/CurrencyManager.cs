using System;
using UnityEngine;
using IdleEmpire.Business;

namespace IdleEmpire.Core
{
    /// <summary>
    /// Singleton MonoBehaviour that manages the player's currency.
    /// Fires <see cref="OnMoneyChanged"/> whenever the balance changes so UI can react.
    /// </summary>
    public class CurrencyManager : MonoBehaviour
    {
        #region Singleton

        /// <summary>The single shared instance of CurrencyManager.</summary>
        public static CurrencyManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            _money = _startingMoney;
        }

        #endregion

        #region Events

        /// <summary>
        /// Invoked after every balance change. Passes the new total as a <c>double</c>.
        /// Subscribe from UI scripts to refresh displays.
        /// </summary>
        public event Action<double> OnMoneyChanged;

        /// <summary>
        /// Invoked only when money is added via <see cref="AddMoney"/>.
        /// Passes the positive amount that was added. Use this for lifetime-earnings tracking.
        /// </summary>
        public event Action<double> OnMoneyAdded;

        /// <summary>
        /// Invoked only when money is deducted via <see cref="SpendMoney"/>.
        /// Passes the positive amount that was spent. Use this for lifetime-spending tracking.
        /// </summary>
        public event Action<double> OnMoneySpent;

        #endregion

        #region Inspector Fields

        [SerializeField] private double _startingMoney = 100;

        #endregion

        #region Private Fields

        private double _money;

        #endregion

        #region Public API

        /// <summary>
        /// Adds <paramref name="amount"/> to the current balance and fires <see cref="OnMoneyChanged"/>.
        /// </summary>
        /// <param name="amount">Positive amount to add.</param>
        public void AddMoney(double amount)
        {
            if (amount <= 0) return;
            _money += amount;
            OnMoneyAdded?.Invoke(amount);
            OnMoneyChanged?.Invoke(_money);
        }

        /// <summary>
        /// Deducts <paramref name="amount"/> from the current balance if the player can afford it,
        /// fires <see cref="OnMoneyChanged"/>, and returns whether the deduction succeeded.
        /// </summary>
        /// <param name="amount">Positive amount to deduct.</param>
        /// <returns><c>true</c> if the deduction was successful; <c>false</c> if insufficient funds.</returns>
        public bool SpendMoney(double amount)
        {
            if (amount <= 0 || !CanAfford(amount)) return false;
            _money -= amount;
            OnMoneySpent?.Invoke(amount);
            OnMoneyChanged?.Invoke(_money);
            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the player currently has at least <paramref name="amount"/> money.
        /// </summary>
        /// <param name="amount">Amount to check against.</param>
        public bool CanAfford(double amount) => _money >= amount;

        /// <summary>Returns the current money balance.</summary>
        public double GetMoney() => _money;

        /// <summary>
        /// Calculates the total income per second across all active businesses in the scene.
        /// </summary>
        /// <returns>Sum of <see cref="BusinessController.GetIncomePerSecond"/> for every business.</returns>
        public double GetIncomePerSecond()
        {
            double total = 0;
            var businesses = FindObjectsOfType<BusinessController>();
            foreach (var business in businesses)
                total += business.GetIncomePerSecond();
            return total;
        }

        /// <summary>
        /// Directly sets the balance to <paramref name="value"/>.
        /// Used during save/load and prestige resets — not for normal gameplay.
        /// </summary>
        /// <param name="value">New balance value (non-negative).</param>
        public void SetMoney(double value)
        {
            _money = Math.Max(0, value);
            OnMoneyChanged?.Invoke(_money);
        }

        #endregion
    }
}

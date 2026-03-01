using System;
using System.Collections.Generic;
using UnityEngine;
using IdleEmpire.Business;
using IdleEmpire.Core;

namespace IdleEmpire.Upgrades
{
    /// <summary>
    /// Manages the pool of available upgrades, tracks which ones have been purchased,
    /// and applies their multipliers to the appropriate <see cref="BusinessController"/> instances.
    /// </summary>
    public class UpgradeManager : MonoBehaviour
    {
        #region Events

        /// <summary>Fired whenever the list of available or purchased upgrades changes.</summary>
        public event Action OnUpgradesChanged;

        #endregion

        #region Inspector Fields

        [Header("Configuration")]
        [SerializeField] private UpgradeData[] _allUpgrades;
        [SerializeField] private BusinessController[] _businesses;

        #endregion

        #region Private Fields

        private readonly HashSet<int> _purchasedIndices = new HashSet<int>();

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            // Restore purchased upgrades from save data.
            var saveData = GameManager.Instance?.SaveManager?.Load();
            if (saveData?.upgradesPurchased != null)
            {
                for (int i = 0; i < saveData.upgradesPurchased.Length; i++)
                {
                    if (saveData.upgradesPurchased[i])
                        RestoreUpgrade(i);
                }
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Attempts to purchase the upgrade at <paramref name="upgradeIndex"/>.
        /// Deducts the cost from the player's balance and applies the multiplier
        /// to the target business if successful.
        /// </summary>
        /// <param name="upgradeIndex">Index into <c>_allUpgrades</c>.</param>
        /// <returns><c>true</c> if the purchase succeeded.</returns>
        public bool PurchaseUpgrade(int upgradeIndex)
        {
            if (!IsValidIndex(upgradeIndex)) return false;
            if (_purchasedIndices.Contains(upgradeIndex))
            {
                Debug.LogWarning($"[UpgradeManager] Upgrade {upgradeIndex} already purchased.");
                return false;
            }

            UpgradeData upgrade = _allUpgrades[upgradeIndex];
            var currency = GameManager.Instance?.CurrencyManager;

            if (currency == null || !currency.CanAfford(upgrade.Cost)) return false;

            currency.SpendMoney(upgrade.Cost);
            _purchasedIndices.Add(upgradeIndex);
            ApplyUpgrade(upgrade);
            OnUpgradesChanged?.Invoke();

            Debug.Log($"[UpgradeManager] Upgrade '{upgrade.UpgradeName}' purchased.");
            return true;
        }

        /// <summary>
        /// Returns the list of upgrades that are available (not yet purchased).
        /// </summary>
        /// <returns>Array of available <see cref="UpgradeData"/>.</returns>
        public UpgradeData[] GetAvailableUpgrades()
        {
            var available = new List<UpgradeData>();

            if (_allUpgrades == null) return available.ToArray();

            for (int i = 0; i < _allUpgrades.Length; i++)
            {
                if (!_purchasedIndices.Contains(i))
                    available.Add(_allUpgrades[i]);
            }

            return available.ToArray();
        }

        /// <summary>
        /// Returns the actual indices (into <c>_allUpgrades</c>) of upgrades that are not yet purchased.
        /// Use these indices when calling <see cref="PurchaseUpgrade"/> from the shop UI.
        /// </summary>
        /// <returns>Array of available upgrade indices.</returns>
        public int[] GetAvailableUpgradeIndices()
        {
            var indices = new List<int>();

            if (_allUpgrades == null) return indices.ToArray();

            for (int i = 0; i < _allUpgrades.Length; i++)
            {
                if (!_purchasedIndices.Contains(i))
                    indices.Add(i);
            }

            return indices.ToArray();
        }

        /// <summary>
        /// Returns <c>true</c> if the upgrade at <paramref name="upgradeIndex"/> has been purchased.
        /// </summary>
        public bool IsUpgradePurchased(int upgradeIndex) => _purchasedIndices.Contains(upgradeIndex);

        /// <summary>
        /// Restores previously purchased upgrades from a saved index array and re-applies their multipliers.
        /// Called by <see cref="Core.GameManager"/> during save-data load.
        /// </summary>
        /// <param name="indices">Array of upgrade indices that were purchased in a prior session.</param>
        public void LoadPurchasedUpgrades(int[] indices)
        {
            if (indices == null) return;

            foreach (int index in indices)
                RestoreUpgrade(index);
        }

        /// <summary>Returns the indices of all purchased upgrades (for serialization).</summary>
        public int[] GetPurchasedIndices()
        {
            var arr = new int[_purchasedIndices.Count];
            _purchasedIndices.CopyTo(arr);
            return arr;
        }

        /// <summary>
        /// Returns a bool array where each index corresponds to an upgrade and <c>true</c> means it was purchased.
        /// Used by <see cref="Core.GameManager"/> to populate <see cref="Core.SaveData.upgradesPurchased"/>.
        /// </summary>
        public bool[] GetPurchasedArray()
        {
            if (_allUpgrades == null) return Array.Empty<bool>();
            bool[] result = new bool[_allUpgrades.Length];
            foreach (int idx in _purchasedIndices)
                if (idx < result.Length)
                    result[idx] = true;
            return result;
        }

        /// <summary>
        /// Clears all purchased upgrades. Used during a prestige reset.
        /// Note: upgrade multipliers already applied to businesses are not reversed here;
        /// callers should also reset the business income multipliers (e.g., via <see cref="Business.BusinessController.ResetMultiplier"/>).
        /// </summary>
        public void ResetUpgrades()
        {
            _purchasedIndices.Clear();
        }

        #endregion

        private void ApplyUpgrade(UpgradeData upgrade)
        {
            if (_businesses == null) return;

            int target = upgrade.TargetBusinessIndex;

            if (target == -1)
            {
                // Global upgrade — apply to all businesses.
                foreach (var business in _businesses)
                    business.ApplyMultiplier(upgrade.Multiplier);
            }
            else if (target >= 0 && target < _businesses.Length)
            {
                _businesses[target].ApplyMultiplier(upgrade.Multiplier);
            }
            else
            {
                Debug.LogWarning($"[UpgradeManager] Invalid target business index {target} for upgrade '{upgrade.UpgradeName}'.");
            }
        }

        private void RestoreUpgrade(int upgradeIndex)
        {
            if (!IsValidIndex(upgradeIndex)) return;
            _purchasedIndices.Add(upgradeIndex);
            ApplyUpgrade(_allUpgrades[upgradeIndex]);
        }

        private bool IsValidIndex(int index)
        {
            return _allUpgrades != null && index >= 0 && index < _allUpgrades.Length;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using IdleEmpire.Business;
using IdleEmpire.Core;
using IdleEmpire.Audio;

namespace IdleEmpire.Managers
{
    /// <summary>
    /// Handles hiring managers from the shop and activating automation
    /// on the corresponding <see cref="BusinessController"/>.
    /// </summary>
    public class ManagerController : MonoBehaviour
    {
        #region Events

        /// <summary>Fired whenever a manager is hired successfully.</summary>
        public event Action OnManagersChanged;

        #endregion

        #region Inspector Fields

        [Header("Configuration")]
        [SerializeField] private ManagerData[] _allManagers;
        [SerializeField] private BusinessController[] _businesses;

        #endregion

        #region Private Fields

        private readonly HashSet<int> _hiredManagerIndices = new HashSet<int>();

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            // Restore manager states from save data.
            var saveData = GameManager.Instance?.SaveManager?.Load();
            if (saveData?.managersHired == null) return;

            for (int i = 0; i < saveData.managersHired.Length; i++)
            {
                if (saveData.managersHired[i])
                    ActivateManagerForBusiness(i);
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Attempts to hire the manager at <paramref name="managerIndex"/>.
        /// Deducts the cost and sets <c>hasManager = true</c> on the target business.
        /// </summary>
        /// <param name="managerIndex">Index into <c>_allManagers</c>.</param>
        /// <returns><c>true</c> if the hire was successful.</returns>
        public bool HireManager(int managerIndex)
        {
            if (!IsValidIndex(managerIndex)) return false;
            if (_hiredManagerIndices.Contains(managerIndex))
            {
                Debug.LogWarning($"[ManagerController] Manager {managerIndex} already hired.");
                return false;
            }

            ManagerData manager = _allManagers[managerIndex];
            var currency = GameManager.Instance?.CurrencyManager;

            if (currency == null || !currency.CanAfford(manager.Cost)) return false;

            currency.SpendMoney(manager.Cost);
            _hiredManagerIndices.Add(managerIndex);
            ActivateManagerForBusiness(manager.TargetBusinessIndex);
            OnManagersChanged?.Invoke();
            AudioManager.Instance?.PlayManagerHire();

            Debug.Log($"[ManagerController] Manager '{manager.ManagerName}' hired.");
            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the manager at <paramref name="managerIndex"/> has been hired.
        /// </summary>
        public bool IsManagerHired(int managerIndex) => _hiredManagerIndices.Contains(managerIndex);

        /// <summary>
        /// Returns all <see cref="ManagerData"/> entries regardless of hired status.
        /// Used by ShopUI to display the full manager list.
        /// </summary>
        /// <returns>A copy of the complete manager array.</returns>
        public ManagerData[] GetAllManagers()
        {
            return _allManagers ?? Array.Empty<ManagerData>();
        }

        /// <summary>Returns the total number of managers (hired or not).</summary>
        public int GetManagerCount() => _allManagers?.Length ?? 0;

        /// <summary>
        /// Returns the list of managers that have not yet been hired.
        /// </summary>
        /// <returns>Array of available (unhired) <see cref="ManagerData"/>.</returns>
        public ManagerData[] GetAvailableManagers()
        {
            var available = new List<ManagerData>();

            if (_allManagers == null) return available.ToArray();

            for (int i = 0; i < _allManagers.Length; i++)
            {
                if (!_hiredManagerIndices.Contains(i))
                    available.Add(_allManagers[i]);
            }

            return available.ToArray();
        }

        /// <summary>
        /// Returns the actual indices (into <c>_allManagers</c>) of managers not yet hired.
        /// Use these indices when calling <see cref="HireManager"/> from the shop UI.
        /// </summary>
        /// <returns>Array of available manager indices.</returns>
        public int[] GetAvailableManagerIndices()
        {
            var indices = new List<int>();

            if (_allManagers == null) return indices.ToArray();

            for (int i = 0; i < _allManagers.Length; i++)
            {
                if (!_hiredManagerIndices.Contains(i))
                    indices.Add(i);
            }

            return indices.ToArray();
        }

        /// <summary>
        /// Restores hired-manager state from a saved bool array and re-activates business automation.
        /// Called by <see cref="Core.GameManager"/> during save-data load.
        /// </summary>
        /// <param name="states">Bool array where index = manager index and value = was hired.</param>
        public void LoadHiredManagers(bool[] states)
        {
            if (states == null) return;

            for (int i = 0; i < states.Length; i++)
            {
                if (states[i] && IsValidIndex(i) && !_hiredManagerIndices.Contains(i))
                {
                    _hiredManagerIndices.Add(i);
                    ActivateManagerForBusiness(_allManagers[i].TargetBusinessIndex);
                }
            }
        }

        /// <summary>
        /// Clears all hired managers. Used during a prestige reset.
        /// </summary>
        public void ResetManagers()
        {
            _hiredManagerIndices.Clear();
        }

        #endregion

        #region Helpers

        private void ActivateManagerForBusiness(int businessIndex)
        {
            if (_businesses == null || businessIndex < 0 || businessIndex >= _businesses.Length)
            {
                Debug.LogWarning($"[ManagerController] Invalid business index {businessIndex}.");
                return;
            }

            _businesses[businessIndex].SetManager(true);
        }

        private bool IsValidIndex(int index)
        {
            return _allManagers != null && index >= 0 && index < _allManagers.Length;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;
using IdleEmpire.Business;
using IdleEmpire.Core;

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
            if (saveData?.managerStates == null) return;

            for (int i = 0; i < saveData.managerStates.Length; i++)
            {
                if (saveData.managerStates[i])
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

            Debug.Log($"[ManagerController] Manager '{manager.ManagerName}' hired.");
            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the manager at <paramref name="managerIndex"/> has been hired.
        /// </summary>
        public bool IsManagerHired(int managerIndex) => _hiredManagerIndices.Contains(managerIndex);

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

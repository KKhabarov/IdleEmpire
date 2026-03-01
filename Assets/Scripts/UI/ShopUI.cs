using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using IdleEmpire.Core;
using IdleEmpire.Upgrades;
using IdleEmpire.Managers;
using IdleEmpire.Utils;

namespace IdleEmpire.UI
{
    /// <summary>
    /// UI controller for the shop panel.
    /// Populates the upgrade list and manager list dynamically,
    /// and handles buy-button affordability checks.
    /// </summary>
    public class ShopUI : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Prefabs")]
        [SerializeField] private GameObject _upgradeItemPrefab;
        [SerializeField] private GameObject _managerItemPrefab;

        [Header("Containers")]
        [SerializeField] private Transform _upgradeListContainer;
        [SerializeField] private Transform _managerListContainer;

        [Header("References")]
        [SerializeField] private UpgradeManager _upgradeManager;
        [SerializeField] private ManagerController _managerController;

        #endregion

        #region Private Fields

        // Maps each instantiated shop item GameObject to its numeric cost so we can
        // refresh affordability without parsing the formatted display text.
        private readonly Dictionary<GameObject, double> _itemCosts = new Dictionary<GameObject, double>();

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            if (_upgradeManager != null)
                _upgradeManager.OnUpgradesChanged += PopulateUpgradeList;

            if (_managerController != null)
                _managerController.OnManagersChanged += PopulateManagerList;

            if (GameManager.Instance?.CurrencyManager != null)
                GameManager.Instance.CurrencyManager.OnMoneyChanged += OnMoneyChanged;

            PopulateUpgradeList();
            PopulateManagerList();
        }

        private void OnDisable()
        {
            if (_upgradeManager != null)
                _upgradeManager.OnUpgradesChanged -= PopulateUpgradeList;

            if (_managerController != null)
                _managerController.OnManagersChanged -= PopulateManagerList;

            if (GameManager.Instance?.CurrencyManager != null)
                GameManager.Instance.CurrencyManager.OnMoneyChanged -= OnMoneyChanged;
        }

        #endregion

        #region Upgrade List

        /// <summary>Clears and repopulates the upgrade list with currently available upgrades.</summary>
        public void PopulateUpgradeList()
        {
            if (_upgradeListContainer == null || _upgradeItemPrefab == null || _upgradeManager == null)
                return;

            ClearContainer(_upgradeListContainer);

            UpgradeData[] available = _upgradeManager.GetAvailableUpgrades();
            int[] actualIndices = _upgradeManager.GetAvailableUpgradeIndices();

            for (int i = 0; i < available.Length; i++)
            {
                int upgradeIndex = actualIndices[i]; // Actual index in _allUpgrades for PurchaseUpgrade().
                GameObject item = Instantiate(_upgradeItemPrefab, _upgradeListContainer);
                SetupShopItem(item, available[i].UpgradeName, available[i].Description,
                    available[i].Cost, available[i].Icon,
                    () => _upgradeManager.PurchaseUpgrade(upgradeIndex));
            }
        }

        #endregion

        #region Manager List

        /// <summary>Clears and repopulates the manager list.</summary>
        public void PopulateManagerList()
        {
            if (_managerListContainer == null || _managerItemPrefab == null || _managerController == null)
                return;

            ClearContainer(_managerListContainer);

            ManagerData[] allManagers = _managerController.GetAllManagers();

            for (int i = 0; i < allManagers.Length; i++)
            {
                int managerIndex = i;
                bool isHired = _managerController.IsManagerHired(managerIndex);

                GameObject item = Instantiate(_managerItemPrefab, _managerListContainer);

                if (isHired)
                {
                    // Show hired managers as disabled "Hired" entries.
                    SetupShopItem(item, allManagers[i].ManagerName, allManagers[i].Description,
                        allManagers[i].Cost, allManagers[i].Icon, null);

                    var buyButton = item.GetComponentInChildren<Button>();
                    if (buyButton != null)
                    {
                        buyButton.interactable = false;
                        var costText = item.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
                        if (costText != null) costText.text = "Hired";
                    }
                }
                else
                {
                    SetupShopItem(item, allManagers[i].ManagerName, allManagers[i].Description,
                        allManagers[i].Cost, allManagers[i].Icon,
                        () => _managerController.HireManager(managerIndex));
                }
            }
        }

        #endregion

        #region Helpers

        private void SetupShopItem(
            GameObject item,
            string itemName,
            string description,
            double cost,
            Sprite icon,
            System.Action onBuy)
        {
            // Track the numeric cost so affordability checks don't parse formatted text.
            _itemCosts[item] = cost;

            // Set name text.
            var nameText = item.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            if (nameText != null) nameText.text = itemName;

            // Set description text.
            var descText = item.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            if (descText != null) descText.text = description;

            // Set cost text (display only — use _itemCosts for affordability logic).
            var costText = item.transform.Find("CostText")?.GetComponent<TextMeshProUGUI>();
            if (costText != null) costText.text = NumberFormatter.FormatNumber(cost);

            // Set icon.
            var iconImage = item.transform.Find("Icon")?.GetComponent<Image>();
            if (iconImage != null && icon != null) iconImage.sprite = icon;

            // Wire up buy button.
            var buyButton = item.GetComponentInChildren<Button>();
            if (buyButton != null)
            {
                buyButton.onClick.AddListener(() => onBuy?.Invoke());
                bool canAfford = GameManager.Instance?.CurrencyManager?.CanAfford(cost) ?? false;
                buyButton.interactable = canAfford;
            }
        }

        private void OnMoneyChanged(double _)
        {
            // Refresh affordability for all items already in both lists.
            RefreshButtonStates(_upgradeListContainer);
            RefreshButtonStates(_managerListContainer);
        }

        private void RefreshButtonStates(Transform container)
        {
            if (container == null) return;

            foreach (Transform child in container)
            {
                if (!_itemCosts.TryGetValue(child.gameObject, out double cost)) continue;

                var buyButton = child.GetComponentInChildren<Button>();
                if (buyButton != null)
                    buyButton.interactable = GameManager.Instance?.CurrencyManager?.CanAfford(cost) ?? false;
            }
        }

        private void ClearContainer(Transform container)
        {
            foreach (Transform child in container)
            {
                _itemCosts.Remove(child.gameObject);
                Destroy(child.gameObject);
            }
        }

        #endregion
    }
}

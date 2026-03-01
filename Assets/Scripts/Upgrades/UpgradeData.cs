using UnityEngine;

namespace IdleEmpire.Upgrades
{
    /// <summary>
    /// ScriptableObject that holds the static configuration for a single upgrade.
    /// Create instances via Assets → Create → IdleEmpire → Upgrade Data.
    /// </summary>
    [CreateAssetMenu(menuName = "IdleEmpire/Upgrade Data", fileName = "NewUpgradeData")]
    public class UpgradeData : ScriptableObject
    {
        #region Inspector Fields

        [Header("Identity")]
        [SerializeField] private string _upgradeName = "New Upgrade";
        [SerializeField] private string _description = "Increases business income.";
        [SerializeField] private Sprite _icon;

        [Header("Economics")]
        [Tooltip("One-time purchase cost for this upgrade.")]
        [SerializeField] private double _cost = 100.0;

        [Tooltip("Income multiplier applied to the target business when purchased (e.g. 2.0 = double income).")]
        [SerializeField] private float _multiplier = 2f;

        [Header("Target")]
        [Tooltip("Index of the BusinessController in the GameManager's business array this upgrade affects. Use -1 for global.")]
        [SerializeField] private int _targetBusinessIndex = 0;

        #endregion

        #region Properties

        /// <summary>Display name shown in the shop.</summary>
        public string UpgradeName => _upgradeName;

        /// <summary>Short description shown in the shop or tooltip.</summary>
        public string Description => _description;

        /// <summary>Icon sprite used in the shop list.</summary>
        public Sprite Icon => _icon;

        /// <summary>One-time purchase cost.</summary>
        public double Cost => _cost;

        /// <summary>Income multiplier applied to the target business on purchase.</summary>
        public float Multiplier => _multiplier;

        /// <summary>
        /// Index of the target business in the GameManager's business array.
        /// -1 means the upgrade affects all businesses.
        /// </summary>
        public int TargetBusinessIndex => _targetBusinessIndex;

        #endregion
    }
}

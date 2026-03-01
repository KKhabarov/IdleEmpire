using UnityEngine;

namespace IdleEmpire.Business
{
    /// <summary>
    /// ScriptableObject that holds the static configuration data for a single business type.
    /// Create instances via Assets → Create → IdleEmpire → Business Data.
    /// </summary>
    [CreateAssetMenu(menuName = "IdleEmpire/Business Data", fileName = "NewBusinessData")]
    public class BusinessData : ScriptableObject
    {
        #region Inspector Fields

        [Header("Identity")]
        [SerializeField] private string _businessName = "New Business";
        [SerializeField] private string _description = "A brand new business.";
        [SerializeField] private Sprite _icon;

        [Header("Economics")]
        [Tooltip("Base purchase cost at level 0.")]
        [SerializeField] private double _baseCost = 10.0;

        [Tooltip("Base income produced per collection cycle at level 1.")]
        [SerializeField] private double _baseIncome = 1.0;

        [Tooltip("Multiplier applied to cost for each subsequent level. Default: 1.15")]
        [SerializeField] private float _costMultiplier = 1.15f;

        [Tooltip("Seconds between automatic income collections when a manager is assigned.")]
        [SerializeField] private float _collectionInterval = 5f;

        #endregion

        #region Properties

        /// <summary>Display name shown in the UI.</summary>
        public string BusinessName => _businessName;

        /// <summary>Short description shown in the shop or tooltip.</summary>
        public string Description => _description;

        /// <summary>Icon sprite used on the business card.</summary>
        public Sprite Icon => _icon;

        /// <summary>Cost to purchase the business at level 0.</summary>
        public double BaseCost => _baseCost;

        /// <summary>Base income value before level or prestige scaling.</summary>
        public double BaseIncome => _baseIncome;

        /// <summary>Cost scaling multiplier per level (e.g. 1.15 = 15 % more expensive each level).</summary>
        public float CostMultiplier => _costMultiplier;

        /// <summary>Seconds between income collections when a manager is assigned.</summary>
        public float CollectionInterval => _collectionInterval;

        #endregion

        #region Cost Formula

        /// <summary>
        /// Calculates the cost to reach the next level from <paramref name="currentLevel"/>.
        /// Formula: <c>BaseCost * CostMultiplier ^ currentLevel</c>
        /// </summary>
        /// <param name="currentLevel">The business's current level.</param>
        /// <returns>Cost in game currency (double).</returns>
        public double CalculateCost(int currentLevel)
        {
            return _baseCost * System.Math.Pow(_costMultiplier, currentLevel);
        }

        #endregion
    }
}

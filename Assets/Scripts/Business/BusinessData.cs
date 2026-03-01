using System;
using UnityEngine;

namespace IdleEmpire.Business
{
    /// <summary>
    /// ScriptableObject that holds the static configuration data for a single business type.
    /// Create instances via Assets → Create → IdleEmpire → Business Data.
    /// </summary>
    [CreateAssetMenu(fileName = "NewBusiness", menuName = "IdleEmpire/Business Data")]
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

        [Tooltip("Seconds per income cycle (default 1).")]
        [SerializeField] private float _cycleDuration = 1f;

        [Tooltip("Multiplier applied to cost for each subsequent level. Default: 1.15")]
        [SerializeField] private float _costMultiplier = 1.15f;

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

        /// <summary>Seconds between income collections per cycle (default 1).</summary>
        public float CycleDuration => _cycleDuration;

        /// <summary>Cost scaling multiplier per level (e.g. 1.15 = 15% more expensive each level).</summary>
        public float CostMultiplier => _costMultiplier;

        #endregion

        #region Methods

        /// <summary>
        /// Calculates the cost to purchase or upgrade to the next level.
        /// Formula: <c>baseCost * costMultiplier ^ level</c>
        /// </summary>
        /// <param name="level">The business's current level.</param>
        /// <returns>Cost in game currency (double).</returns>
        public double GetCostForLevel(int level)
        {
            return _baseCost * Math.Pow(_costMultiplier, level);
        }

        /// <summary>
        /// Calculates the income per cycle at the given level.
        /// Formula: <c>baseIncome * level</c>
        /// </summary>
        /// <param name="level">The business's current level.</param>
        /// <returns>Income per cycle (double).</returns>
        public double GetIncomeForLevel(int level)
        {
            return _baseIncome * level;
        }

        #endregion
    }
}

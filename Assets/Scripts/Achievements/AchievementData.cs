using UnityEngine;

namespace IdleEmpire.Achievements
{
    /// <summary>Defines the category of condition that must be met to unlock an achievement.</summary>
    public enum AchievementType
    {
        /// <summary>Lifetime earnings reach the target value.</summary>
        TotalMoneyEarned,
        /// <summary>Any single business reaches the target level.</summary>
        BusinessLevel,
        /// <summary>Sum of all business levels reaches the target value.</summary>
        TotalBusinessLevel,
        /// <summary>Total managers hired is at least the target value.</summary>
        ManagersHired,
        /// <summary>Total upgrades purchased is at least the target value.</summary>
        UpgradesPurchased,
        /// <summary>Number of prestige resets is at least the target value.</summary>
        PrestigeCount,
        /// <summary>Total income per second reaches the target value.</summary>
        IncomePerSecond,
        /// <summary>Own (level &gt; 0) at least the target number of different businesses.</summary>
        BusinessOwned,
        /// <summary>A specific business (identified by <c>TargetBusinessIndex</c>) reaches the target level.</summary>
        SpecificBusinessLevel,
    }

    /// <summary>
    /// ScriptableObject that defines a single achievement — its type, unlock condition,
    /// optional reward, icon, name, and description.
    /// </summary>
    [CreateAssetMenu(menuName = "IdleEmpire/Achievement Data", fileName = "NewAchievementData")]
    public class AchievementData : ScriptableObject
    {
        #region Inspector Fields

        [Header("Display")]
        [Tooltip("Short display name shown in the achievement list and notification.")]
        [SerializeField] private string _achievementName;

        [Tooltip("One-sentence description of what the player must do.")]
        [SerializeField] private string _description;

        [Tooltip("Icon displayed alongside the achievement.")]
        [SerializeField] private Sprite _icon;

        [Header("Condition")]
        [Tooltip("Category of progress metric used to determine unlock.")]
        [SerializeField] private AchievementType _type;

        [Tooltip("Numeric threshold that must be reached to unlock this achievement.")]
        [SerializeField] private double _targetValue;

        [Tooltip("Business index for SpecificBusinessLevel achievements; -1 = any business.")]
        [SerializeField] private int _targetBusinessIndex = -1;

        [Header("Reward")]
        [Tooltip("Money awarded when this achievement is unlocked. Set to 0 for no reward.")]
        [SerializeField] private double _rewardMoney;

        #endregion

        #region Properties

        /// <summary>Short display name of this achievement.</summary>
        public string AchievementName => _achievementName;

        /// <summary>Description of how to unlock this achievement.</summary>
        public string Description => _description;

        /// <summary>Icon sprite to display alongside this achievement.</summary>
        public Sprite Icon => _icon;

        /// <summary>The type of progress metric tracked for this achievement.</summary>
        public AchievementType Type => _type;

        /// <summary>The numeric threshold required to unlock this achievement.</summary>
        public double TargetValue => _targetValue;

        /// <summary>
        /// For <see cref="AchievementType.SpecificBusinessLevel"/> achievements, the index of the
        /// target business. <c>-1</c> for other achievement types.
        /// </summary>
        public int TargetBusinessIndex => _targetBusinessIndex;

        /// <summary>Money granted to the player when this achievement is first unlocked. 0 = no reward.</summary>
        public double RewardMoney => _rewardMoney;

        #endregion
    }
}

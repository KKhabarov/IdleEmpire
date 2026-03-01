using UnityEngine;

namespace IdleEmpire.Managers
{
    /// <summary>
    /// ScriptableObject that holds the static configuration for a business manager.
    /// Create instances via Assets → Create → IdleEmpire → Manager Data.
    /// </summary>
    [CreateAssetMenu(menuName = "IdleEmpire/Manager Data", fileName = "NewManagerData")]
    public class ManagerData : ScriptableObject
    {
        #region Inspector Fields

        [Header("Identity")]
        [SerializeField] private string _managerName = "New Manager";
        [SerializeField] private string _description = "Automates this business.";
        [SerializeField] private Sprite _icon;

        [Header("Economics")]
        [Tooltip("One-time hire cost for this manager.")]
        [SerializeField] private double _cost = 500.0;

        [Header("Target")]
        [Tooltip("Index of the BusinessController in GameManager's business array that this manager automates.")]
        [SerializeField] private int _targetBusinessIndex = 0;

        #endregion

        #region Properties

        /// <summary>Display name shown in the shop.</summary>
        public string ManagerName => _managerName;

        /// <summary>Short description shown in the shop or tooltip.</summary>
        public string Description => _description;

        /// <summary>Icon sprite used in the shop list.</summary>
        public Sprite Icon => _icon;

        /// <summary>One-time hire cost.</summary>
        public double Cost => _cost;

        /// <summary>Index of the business this manager automates.</summary>
        public int TargetBusinessIndex => _targetBusinessIndex;

        #endregion
    }
}

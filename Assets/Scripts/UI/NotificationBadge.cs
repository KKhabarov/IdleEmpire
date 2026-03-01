using TMPro;
using UnityEngine;

namespace IdleEmpire.UI
{
    /// <summary>
    /// Displays a notification count badge on a tab button.
    /// Shows number of affordable upgrades/managers in Shop tab,
    /// or new unlocked achievements in Achievements tab.
    /// </summary>
    public class NotificationBadge : MonoBehaviour
    {
        #region Inspector Fields

        [SerializeField] private GameObject _badgeObject;
        [SerializeField] private TextMeshProUGUI _countText;

        #endregion

        #region Public API

        /// <summary>
        /// Shows the badge with the given count, or hides it when count is zero.
        /// </summary>
        /// <param name="count">Number to display on the badge.</param>
        public void SetCount(int count)
        {
            if (_badgeObject == null) return;

            if (count > 0)
            {
                _badgeObject.SetActive(true);
                if (_countText != null)
                    _countText.text = count.ToString();
            }
            else
            {
                Hide();
            }
        }

        /// <summary>Hides the badge unconditionally.</summary>
        public void Hide()
        {
            if (_badgeObject != null)
                _badgeObject.SetActive(false);
        }

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            Hide();
        }

        #endregion
    }
}

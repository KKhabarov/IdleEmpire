using UnityEngine;
using UnityEngine.UI;

namespace IdleEmpire.UI
{
    /// <summary>
    /// Simple tab controller that switches between multiple panels by activating
    /// one at a time and highlighting its corresponding tab button.
    /// </summary>
    public class TabController : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Tabs")]
        [SerializeField] private Button[] _tabButtons;
        [SerializeField] private GameObject[] _tabPanels;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                int index = i;
                _tabButtons[i]?.onClick.AddListener(() => SelectTab(index));
            }

            // Show the first tab by default.
            if (_tabPanels != null && _tabPanels.Length > 0)
                SelectTab(0);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Activates the panel at <paramref name="index"/> and deactivates all others.
        /// The selected tab button is made non-interactable to indicate the active state;
        /// all others are made interactable again.
        /// </summary>
        /// <param name="index">Zero-based index of the tab to select.</param>
        public void SelectTab(int index)
        {
            if (_tabPanels != null)
            {
                for (int i = 0; i < _tabPanels.Length; i++)
                {
                    if (_tabPanels[i] != null)
                        _tabPanels[i].SetActive(i == index);
                }
            }

            if (_tabButtons != null)
            {
                for (int i = 0; i < _tabButtons.Length; i++)
                {
                    if (_tabButtons[i] != null)
                        _tabButtons[i].interactable = i != index;
                }
            }
        }

        #endregion
    }
}

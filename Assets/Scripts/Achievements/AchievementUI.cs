using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IdleEmpire.Achievements
{
    /// <summary>
    /// UI panel that displays all achievements as a scrollable list.
    /// Unlocked achievements are shown in full colour; locked ones are greyed out with a progress bar.
    /// </summary>
    public class AchievementUI : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Prefabs")]
        [Tooltip("Prefab instantiated for each achievement entry. Must contain child components for icon, name, description, progress bar, and a CanvasGroup for greying out.")]
        [SerializeField] private GameObject _achievementItemPrefab;

        [Header("Containers")]
        [Tooltip("Parent transform into which achievement item prefabs are instantiated.")]
        [SerializeField] private Transform _achievementListContainer;

        [Header("Header")]
        [Tooltip("Header text updated to show 'Achievements (unlocked/total)'.")]
        [SerializeField] private TextMeshProUGUI _headerText;

        [Header("References")]
        [Tooltip("AchievementManager providing achievement data and progress.")]
        [SerializeField] private AchievementManager _achievementManager;

        #endregion

        #region Unity Callbacks

        private void OnEnable()
        {
            PopulateList();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Clears and repopulates the achievement list, reflecting current unlock and progress state.
        /// </summary>
        public void PopulateList()
        {
            if (_achievementListContainer == null || _achievementItemPrefab == null || _achievementManager == null)
                return;

            // Clear existing items.
            foreach (Transform child in _achievementListContainer)
                Destroy(child.gameObject);

            AchievementData[] all = _achievementManager.GetAllAchievements();
            int unlocked = _achievementManager.GetUnlockedCount();
            int total    = _achievementManager.GetTotalCount();

            // Update header.
            if (_headerText != null)
                _headerText.text = $"Achievements ({unlocked}/{total})";

            for (int i = 0; i < all.Length; i++)
            {
                AchievementData data = all[i];
                if (data == null) continue;

                GameObject item = Instantiate(_achievementItemPrefab, _achievementListContainer);
                bool isUnlocked = _achievementManager.IsUnlocked(i);
                float progress  = _achievementManager.GetProgress(i);

                // Icon
                var icon = item.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null && data.Icon != null)
                    icon.sprite = data.Icon;

                // Name
                var nameText = item.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                if (nameText != null)
                    nameText.text = data.AchievementName;

                // Description
                var descText = item.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
                if (descText != null)
                    descText.text = data.Description;

                // Progress bar
                var progressBar = item.transform.Find("ProgressBar")?.GetComponent<Slider>();
                if (progressBar != null)
                    progressBar.value = progress;

                // Locked/unlocked visual state
                var canvasGroup = item.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                    canvasGroup.alpha = isUnlocked ? 1f : 0.45f;
            }
        }

        #endregion
    }
}

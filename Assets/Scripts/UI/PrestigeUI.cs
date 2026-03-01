using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IdleEmpire.Core;
using IdleEmpire.Business;
using IdleEmpire.Utils;

namespace IdleEmpire.UI
{
    /// <summary>
    /// UI controller for the prestige screen.
    /// Displays the current prestige multiplier, the potential bonus gained on reset,
    /// and a prestige button that requests user confirmation before performing the reset.
    /// </summary>
    public class PrestigeUI : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Display")]
        [SerializeField] private TextMeshProUGUI _currentMultiplierText;
        [SerializeField] private TextMeshProUGUI _potentialBonusText;
        [SerializeField] private TextMeshProUGUI _confirmationText;

        [Header("Buttons")]
        [SerializeField] private Button _prestigeButton;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private GameObject _confirmationPanel;

        [Header("References")]
        [SerializeField] private BusinessController[] _businesses;

        [Header("Prestige Formula")]
        [Tooltip("Minimum total income required to perform a prestige reset.")]
        [SerializeField] private double _minimumPrestigeIncome = 1_000_000;

        [Tooltip("Base multiplier increment added per prestige (e.g. 0.1 = +10% per prestige).")]
        [SerializeField] private float _multiplierIncrement = 0.1f;

        #endregion

        #region Private Fields

        private float _currentPrestigeMultiplier = 1f;

        #endregion

        #region Unity Callbacks

        private void Awake()
        {
            _confirmButton?.onClick.AddListener(OnConfirmPrestigeClicked);
            _cancelButton?.onClick.AddListener(OnCancelPrestigeClicked);
            _prestigeButton?.onClick.AddListener(OnPrestigeButtonClicked);
        }

        private void OnEnable()
        {
            LoadCurrentMultiplier();
            RefreshUI();
        }

        #endregion

        #region UI Refresh

        private void LoadCurrentMultiplier()
        {
            var saveData = GameManager.Instance?.SaveManager?.Load();
            _currentPrestigeMultiplier = (float)(saveData?.prestigeMultiplier ?? 1.0);
        }

        private void RefreshUI()
        {
            if (_currentMultiplierText != null)
                _currentMultiplierText.text = $"Current Bonus: x{_currentPrestigeMultiplier:F2}";

            float nextMultiplier = _currentPrestigeMultiplier + _multiplierIncrement;
            if (_potentialBonusText != null)
                _potentialBonusText.text = $"After Prestige: x{nextMultiplier:F2}";

            double totalIncome = GetTotalIncomePerSecond();
            bool canPrestige = totalIncome >= _minimumPrestigeIncome;

            if (_prestigeButton != null)
                _prestigeButton.interactable = canPrestige;

            if (_confirmationPanel != null)
                _confirmationPanel.SetActive(false);
        }

        private double GetTotalIncomePerSecond()
        {
            double total = 0;
            if (_businesses == null) return total;

            foreach (var business in _businesses)
            {
                if (business != null)
                    total += business.GetIncomePerSecond();
            }
            return total;
        }

        #endregion

        #region Button Handlers

        /// <summary>Called by the Prestige button's OnClick event. Shows the confirmation panel.</summary>
        public void OnPrestigeButtonClicked()
        {
            float nextMultiplier = _currentPrestigeMultiplier + _multiplierIncrement;

            if (_confirmationText != null)
                _confirmationText.text =
                    $"Reset all progress and gain a permanent x{nextMultiplier:F2} income bonus?\nThis cannot be undone.";

            if (_confirmationPanel != null)
                _confirmationPanel.SetActive(true);
        }

        /// <summary>Called by the Confirm button. Performs the prestige reset.</summary>
        public void OnConfirmPrestigeClicked()
        {
            GameManager.Instance?.PrestigeReset();

            if (_confirmationPanel != null)
                _confirmationPanel.SetActive(false);

            LoadCurrentMultiplier();
            RefreshUI();
        }

        /// <summary>Called by the Cancel button. Hides the confirmation panel.</summary>
        public void OnCancelPrestigeClicked()
        {
            if (_confirmationPanel != null)
                _confirmationPanel.SetActive(false);
        }

        #endregion
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleEmpire.UI
{
    /// <summary>
    /// Reusable modal confirmation dialog.
    /// Usage: ConfirmDialog.Instance.Show("Title", "Message", onConfirm, onCancel);
    /// </summary>
    public class ConfirmDialog : MonoBehaviour
    {
        #region Singleton

        /// <summary>Shared instance of the ConfirmDialog.</summary>
        public static ConfirmDialog Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Hide();
        }

        #endregion

        #region Inspector Fields

        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private TextMeshProUGUI _confirmButtonText;
        [SerializeField] private TextMeshProUGUI _cancelButtonText;

        #endregion

        #region Private Fields

        private Action _onConfirm;
        private Action _onCancel;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            if (_confirmButton != null)
                _confirmButton.onClick.AddListener(OnConfirmClicked);

            if (_cancelButton != null)
                _cancelButton.onClick.AddListener(OnCancelClicked);
        }

        #endregion

        #region Public API

        /// <summary>
        /// Displays the confirmation dialog with the specified content.
        /// </summary>
        /// <param name="title">Dialog title.</param>
        /// <param name="message">Dialog body message.</param>
        /// <param name="onConfirm">Callback invoked when the user confirms.</param>
        /// <param name="onCancel">Optional callback invoked when the user cancels.</param>
        /// <param name="confirmText">Label for the confirm button.</param>
        /// <param name="cancelText">Label for the cancel button.</param>
        public void Show(string title, string message, Action onConfirm, Action onCancel = null,
                         string confirmText = "Confirm", string cancelText = "Cancel")
        {
            _onConfirm = onConfirm;
            _onCancel  = onCancel;

            if (_titleText != null)   _titleText.text   = title;
            if (_messageText != null) _messageText.text = message;
            if (_confirmButtonText != null) _confirmButtonText.text = confirmText;
            if (_cancelButtonText  != null) _cancelButtonText.text  = cancelText;

            if (_panel != null)
                _panel.SetActive(true);
        }

        /// <summary>Hides the confirmation dialog.</summary>
        public void Hide()
        {
            if (_panel != null)
                _panel.SetActive(false);

            _onConfirm = null;
            _onCancel  = null;
        }

        #endregion

        #region Button Callbacks

        private void OnConfirmClicked()
        {
            Action callback = _onConfirm;
            Hide();
            callback?.Invoke();
        }

        private void OnCancelClicked()
        {
            Action callback = _onCancel;
            Hide();
            callback?.Invoke();
        }

        #endregion
    }
}

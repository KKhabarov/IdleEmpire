using UnityEngine;
using IdleEmpire.Business;

namespace IdleEmpire.UI
{
    /// <summary>
    /// Shows a small "AUTO" badge on business cards that have a manager assigned.
    /// </summary>
    public class AutoCollectIndicator : MonoBehaviour
    {
        #region Inspector Fields

        [SerializeField] private GameObject _autoLabel;
        [SerializeField] private BusinessController _business;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            UpdateIndicator();
        }

        private void OnEnable()
        {
            if (_business != null)
                _business.OnLevelChanged += OnBusinessChanged;
        }

        private void OnDisable()
        {
            if (_business != null)
                _business.OnLevelChanged -= OnBusinessChanged;
        }

        #endregion

        #region Private Methods

        private void OnBusinessChanged(BusinessController business)
        {
            UpdateIndicator();
        }

        private void UpdateIndicator()
        {
            if (_autoLabel == null) return;
            _autoLabel.SetActive(_business != null && _business.HasManager);
        }

        #endregion
    }
}

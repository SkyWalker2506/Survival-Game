using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    /// <summary>
    /// Basic UI Panel that can be toggled on and off.
    /// </summary>
    public abstract class PanelUI : MonoBehaviour, IPanel
    {
        [SerializeField, BeginGroup("Settings")]
        [Tooltip("Should this panel automatically show itself on start?")]
        private bool _showOnStart;

        [SerializeField]
        [Tooltip("Can the user press escape to hid this panel?")]
        private bool _canEscape;

        [SerializeField, Range(-1, 12), EndGroup]
        [Tooltip("Only one panel with the same layer can be visible at the same time. This can be ignored by setting the layer to -1.")]
        private int _panelLayer;
        
        private bool _isDestroyed;
        private bool _isVisible;
        private bool _isActive;


        public bool IsActive => _isActive;
        public bool IsVisible => _isVisible;
        public int PanelLayer => _panelLayer;
        public bool CanEscape => _canEscape;

        public event UnityAction<bool> PanelToggled;

        /// <summary>
        /// Shows the panel.
        /// </summary>
        public void Show() => Toggle(true);
        
        /// <summary>
        /// Hides the panel.
        /// </summary>
        public void Hide() => Toggle(false);

        void IPanel.ChangeVisibility(bool show)
        {
            if (_isVisible == show || _isDestroyed || !UnityUtils.IsPlayMode)
                return;
            
            OnShowPanel(show);
            _isVisible = show;
        }

        protected abstract void OnShowPanel(bool show);

        protected virtual void OnDestroy()
        {
            _isDestroyed = true;
            Toggle(false);
        }
        
        /// <summary>
        /// Show/Hide the panel.
        /// </summary>
        private void Toggle(bool show)
        {
            if (_isActive || _isVisible != show)
            {
                bool wasActive = _isActive;
                
                if (show)
                {
                    PanelManagerUI.ShowPanel(this);
                    _isActive = true;
                }
                else
                {
                    PanelManagerUI.HidePanel(this);
                    _isActive = false;
                }
                
                if (_isActive != wasActive)
                    PanelToggled?.Invoke(_isActive);
            }
        }

        private void Start()
        {
            if (_showOnStart)
                Toggle(true);
        }
    }
}
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [RequireComponent(typeof(PanelUI))]
    public abstract class UserOptionsUI<T> : MonoBehaviour where T : UserOptions<T>
    {
        [SerializeField, BeginGroup("Settings")]
        private ButtonUI _restoreBtn;

        [SerializeField, BeginHorizontal]
        private bool _requiresApply;
        
        [ShowIf(nameof(_requiresApply), true)]
        [SerializeField, EndHorizontal, HideLabel, EndGroup]
        private ButtonUI _applyBtn;


        protected T Settings => UserOptions<T>.Instance;

        protected abstract void InitializeWidgets();
        protected abstract void ResetWidgets();

        private void Start()
        {
            var panel = GetComponent<PanelUI>();
            panel.PanelToggled += OnPanelToggled;
            InitializeWidgets();
        }

        private void OnPanelToggled(bool isEnabled)
        {
            HandleApplying(isEnabled);
            ConnectRestoreDefaultsButton(isEnabled);
            ConnectApplyButton(isEnabled);
        }

        private void HandleApplying(bool isEnabled)
        {
            if (isEnabled)
                ResetWidgets();
            else
            {
                if (_requiresApply)
                    Settings.LoadFromFileAndApply();
                else
                    Settings.SaveToFileAndApply();
            }
        }

        private void ConnectApplyButton(bool connect)
        {
            if (_applyBtn == null)
                return;
            
            if (connect)
            {
                Settings.Changed += _applyBtn.gameObject.SetActive;
                _applyBtn.OnSelected += Settings.SaveToFileAndApply;
                _applyBtn.gameObject.SetActive(false);
            }
            else
            {
                _applyBtn.OnSelected -= Settings.SaveToFileAndApply;
                Settings.Changed -= _applyBtn.gameObject.SetActive;
                _applyBtn.gameObject.SetActive(false);
            }
        }

        private void ConnectRestoreDefaultsButton(bool connect)
        {
            if (_restoreBtn == null)
                return;
            
            if (connect)
                _restoreBtn.OnSelected += RestoreDefaults;
            else
                _restoreBtn.OnSelected -= RestoreDefaults;

            void RestoreDefaults()
            {
                Settings.RestoreDefaults();
                ResetWidgets();
            }
        }
    }
}
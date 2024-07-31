using PolymindGames.InventorySystem;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(IFirearm))]
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Index-Based Attachments")]
    public sealed class FirearmIndexBasedAttachmentsManager : WieldableItemBehaviour, IFirearmIndexModeHandler
    {
        [SerializeField, Range(0f, 10f), BeginGroup("Settings")]
        [Tooltip("The cooldown duration between mode changes.")]
        private float _changeModeCooldown = 0.35f;

        [SerializeField, InLineEditor]
        [Tooltip("The audio data for the mode change.")]
        private AudioDataSO _changeModeAudio;

        [SerializeField, EndGroup]
        [Tooltip("Determines if there's an animation for mode change.")]
        private bool _changeModeAnimation = true;

        [SerializeField, BeginGroup("Modes")]
        [Tooltip("Reference to the property definition identifying the mode index.")]
        private DataIdReference<ItemPropertyDefinition> _indexProperty;

        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false), EndGroup]
        [Tooltip("The array of firearm attachment behaviors representing different modes.")]
        private FirearmAttachmentBehaviour[] _modes;

        private ItemProperty _attachedProperty;
        private IFirearm _firearm;
        private int _selectedIndex;
        private float _toggleTimer;
        
        
        public FirearmAttachmentBehaviour CurrentMode => _modes[_selectedIndex];
        public IFirearm Firearm => _firearm;

        public event UnityAction<FirearmAttachmentBehaviour> ModeChanged;

        public void ToggleNextMode()
        {
            if (_attachedProperty == null || Time.time < _toggleTimer || _firearm.Magazine.IsReloading)
                return;

            _toggleTimer = Time.time + _changeModeCooldown;

            int lastIndex = _attachedProperty.Integer;
            _attachedProperty.Integer = (int)Mathf.Repeat(_attachedProperty.Integer + 1, _modes.Length - 1 + 0.01f);

            if (_attachedProperty.Integer != lastIndex)
            {
                WieldableItem.Wieldable.AudioPlayer.PlaySafe(_changeModeAudio);
                ModeChanged?.Invoke(CurrentMode);
            }

            if (_changeModeAnimation)
                WieldableItem.Wieldable.Animation.SetTrigger(WieldableAnimationConstants.CHANGE_MODE);
        }

        protected override void OnItemChanged(IItem item)
        {
            if (_attachedProperty != null)
                _attachedProperty.Changed -= OnPropertyChanged;

            if (item != null && item.TryGetPropertyWithId(_indexProperty, out _attachedProperty))
            {
                AttachConfigurationWithIndex(_attachedProperty.Integer);
                _attachedProperty.Changed += OnPropertyChanged;
            }
        }

        private void AttachConfigurationWithIndex(int index)
        {
            for (int i = 0; i < _modes.Length; i++)
            {
                if (i == index || i == _modes.Length - 1)
                {
                    _selectedIndex = i;
                    _modes[_selectedIndex].Attach();
                    return;
                }
            }
        }

        private void OnPropertyChanged(ItemProperty property) => AttachConfigurationWithIndex(property.Integer);

        private void Start() => _firearm = GetComponent<IFirearm>();
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_indexProperty.IsNull)
            {
                UnityUtils.SafeOnValidate(this, () =>
                {
                    if (ItemPropertyDefinition.TryGetWithName("Fire Mode", out var def))
                        _indexProperty = def.Id;
                });
            }
        }
#endif
    }
}
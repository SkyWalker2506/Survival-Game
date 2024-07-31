using System;
using PolymindGames.InventorySystem;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(IFirearm))]
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Item-Based Attachments")]
    public sealed class FirearmItemBasedAttachmentsManager : WieldableItemBehaviour
    {
        [SerializeField, BeginGroup]
        [Tooltip("The audio data for the mode change.")]
        private AudioDataSO _changeModeAudio;

        [SerializeField, EndGroup]
        [Tooltip("Determines if there's an animation for mode change.")]
        private bool _changeModeAnimation = true;

        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("The configurations for attachment items.")]
        [ReorderableList(ListStyle.Lined, elementLabel: "Config", Foldable = false)]
        private AttachmentItemConfigurations[] _configurations;

        private IItem _prevItem;


        protected override void OnItemChanged(IItem item)
        {
            if (_prevItem != null)
            {
                for (int i = 0; i < _configurations.Length; i++)
                    _configurations[i].DetachFromItem(_prevItem);
            }

            if (item != null)
            {
                _prevItem = item;
                for (int i = 0; i < _configurations.Length; i++)
                    _configurations[i].AttachToItem(item);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            foreach (var config in _configurations)
                config.AttachmentChangedCallback = OnAttachmentChanged;
        }

        private void OnAttachmentChanged()
        {
            WieldableItem.Wieldable.AudioPlayer.PlaySafe(_changeModeAudio);
            
            if (_changeModeAnimation)
                WieldableItem.Wieldable.Animation.SetTrigger(WieldableAnimationConstants.CHANGE_MODE);
        }

        #region Internal
        [Serializable]
        public sealed class AttachmentItemConfigurations
        {
            [SerializeField, NewLabel("Property")]
            private DataIdReference<ItemPropertyDefinition> _attachmentTypeProperty;

            [SerializeField, SpaceArea]
            [ReorderableList(ListStyle.Lined), LabelFromChild("_attachment")]
            private AttachmentItemConfiguration[] _configurations;
            
            
            public UnityAction AttachmentChangedCallback;

            public void AttachToItem(IItem item)
            {
                if (item.TryGetPropertyWithId(_attachmentTypeProperty, out var property))
                {
                    AttachConfigurationWithID(property.ItemId);
                    property.Changed += OnPropertyChanged;
                }
            }

            public void DetachFromItem(IItem item)
            {
                if (item.TryGetPropertyWithId(_attachmentTypeProperty, out var property))
                    property.Changed -= OnPropertyChanged;
            }

            private void OnPropertyChanged(ItemProperty property)
            {
                AttachConfigurationWithID(property.ItemId);
                AttachmentChangedCallback?.Invoke();
            }

            private void AttachConfigurationWithID(int id)
            {
                foreach (var config in _configurations)
                {
                    if (config.CorrespondingItem != id)
                        continue;

                    config.Attach();
                    return;
                }
            }
        }

        [Serializable]
        public sealed class AttachmentItemConfiguration
        {
            [SerializeField]
            private DataIdReference<ItemDefinition> _item;

            [SerializeField]
            private FirearmAttachmentBehaviour _attachment;
            
            public int CorrespondingItem => _item;
            public void Attach() => _attachment.Attach();
        }
        #endregion
    }
}
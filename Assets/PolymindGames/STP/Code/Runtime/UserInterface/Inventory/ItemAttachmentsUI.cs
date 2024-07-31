using System;
using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class ItemAttachmentsUI : MonoBehaviour
    {
        [SerializeField, BeginGroup]
        private DataIdReference<ItemTagDefinition> _wieldableTag;

        [SerializeField, NotNull]
        private ItemSlotUI _attachmentTemplate;

        [SerializeField, EndGroup]
        private RectTransform _attachmentSlotsRoot;

        [SerializeField, ReorderableList(ListStyle.Boxed, "Attachment Type Slot")]
        private PropertySlot[] _attachmentSlots = Array.Empty<PropertySlot>();

        private ItemSlotUI _inspectedSlot;


        private void UpdatePropertySlots(ItemSlotUI inspected)
        {
            _inspectedSlot = inspected;
            bool inspectedIsWieldable = inspected != null && inspected.HasItem && inspected.Item.Definition.Tag == _wieldableTag;
            foreach (var attachmentSlot in _attachmentSlots)
            {
                var slotUI = attachmentSlot.SlotUI;

                if (inspectedIsWieldable && inspected.Item.TryGetPropertyWithId(attachmentSlot.Property, out var property))
                {
                    int attachmentId = property.ItemId;
                    int currentAttachmentId = slotUI.HasItem
                        ? slotUI.Item.Id
                        : DataIdReference<ItemDefinition>.NullRef;

                    if (attachmentId != currentAttachmentId)
                    {
                        IItem attachedItem = attachmentId != DataIdReference<ItemDefinition>.NullRef
                            ? new Item(ItemDefinition.GetWithId(attachmentId))
                            : null;

                        slotUI.ItemSlot.Item = attachedItem;
                    }

                    if (!slotUI.gameObject.activeSelf)
                    {
                        slotUI.gameObject.SetActive(true);
                        slotUI.enabled = true;
                        slotUI.ItemSlot.ItemChanged += OnSlotChanged;
                    }
                }
                else
                {
                    if (slotUI.enabled)
                    {
                        slotUI.ItemSlot.ItemChanged -= OnSlotChanged;
                        slotUI.gameObject.SetActive(false);
                        slotUI.enabled = false;
                    }
                }
            }
        }

        private void OnSlotChanged(ItemSlot.CallbackArgs args)
        {
            if (_inspectedSlot == null || args.Type == ItemSlot.CallbackType.PropertyChanged)
                return;

            var itemSlot = args.Slot;
            var propertySlot = GetPropertySlotWithItemSlot(itemSlot);

            if (_inspectedSlot.Item.TryGetPropertyWithId(propertySlot.Property, out var itemProperty))
                itemProperty.ItemId = itemSlot.HasItem ? itemSlot.Item.Id : DataIdReference<ItemDefinition>.NullRef;
        }

        private PropertySlot GetPropertySlotWithItemSlot(ItemSlot itemSlot)
        {
            foreach (var slot in _attachmentSlots)
            {
                if (slot.SlotUI.ItemSlot == itemSlot)
                    return slot;
            }

            return null;
        }

        private void OnEnable()
        {
            ItemSelectorUI.Instance.SelectedSlotChanged += UpdatePropertySlots;
            if (ItemSelectorUI.Instance.SelectedSlot != null)
                UpdatePropertySlots(ItemSelectorUI.Instance.SelectedSlot);
        }

        private void OnDisable() => ItemSelectorUI.Instance.SelectedSlotChanged -= UpdatePropertySlots;

        private void Awake()
        {
            var root = _attachmentSlotsRoot != null ? _attachmentSlotsRoot : transform;
            var tagRestriction = new ItemTagRestriction(ItemTagRestriction.AllowType.OnlyWithoutTags, _wieldableTag);
            foreach (var attachmentSlot in _attachmentSlots)
            {
                ItemSlotUI slot = Instantiate(_attachmentTemplate, root);
                slot.IconInfo.BgIconImage.sprite = attachmentSlot.BackgroundIcon;
                attachmentSlot.WieldableTag = _wieldableTag;
                attachmentSlot.SlotUI = slot;
                slot.gameObject.SetActive(false);

                var container = new ItemContainer(null, string.Empty, 1,
                    new ItemPropertyRestriction(attachmentSlot.Property), tagRestriction);
                slot.SetItemSlot(container[0]);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_wieldableTag.IsNull)
                UnityUtils.SafeOnValidate(this, () => _wieldableTag = "Wieldable");

            // Check for duplicates...
            foreach (var attachmentSlot in _attachmentSlots)
            {
                foreach (var slot in _attachmentSlots)
                {
                    if (slot == attachmentSlot)
                        continue;

                    if (slot.Property == attachmentSlot.Property)
                    {
                        slot.Property = new DataIdReference<ItemPropertyDefinition>(string.Empty);
                        return;
                    }
                }
            }
        }
#endif
        
        #region Internal
        [Serializable]
        private sealed class PropertySlot
        {

            public Sprite BackgroundIcon;
            public DataIdReference<ItemPropertyDefinition> Property;

            [NonSerialized]
            public ItemSlotUI SlotUI;

            [NonSerialized]
            public DataIdReference<ItemTagDefinition> WieldableTag;


            public bool ValidateItem(IItem item)
            {
                if (item != null)
                    return item.Definition.Tag == WieldableTag && item.HasPropertyWithId(Property);

                return true;
            }
        }
        #endregion
    }
}
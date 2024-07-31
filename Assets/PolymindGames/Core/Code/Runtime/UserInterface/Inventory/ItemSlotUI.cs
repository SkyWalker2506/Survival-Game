using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [AddComponentMenu("Polymind Games/User Interface/Slots/Item Slot")]
    public class ItemSlotUI : SlotUI
    {
        [SerializeField, IgnoreParent, BeginGroup]
        private ItemIconInfo _iconInfo;

        [SerializeField, IgnoreParent]
        private ItemStackInfo _itemStackInfo;

        [SerializeField, IgnoreParent, EndGroup]
        private ItemPropertyFillBarInfo _propertyFillBarInfo;

        private ItemSlot _itemSlot;
        

        public ItemSlot ItemSlot
        {
            get
            {
#if DEBUG
                if (_itemSlot == null)
                    Debug.LogError("No item slot is linked to this interface!");
#endif

                return _itemSlot;
            }
        }

        public bool HasItem => Item != null;
        public bool HasItemSlot => _itemSlot != null;
        public bool HasContainer => HasItemSlot && _itemSlot.HasContainer;
        public IItem Item => _itemSlot?.Item;
        public IItemContainer Container => _itemSlot?.Container;
        public ItemIconInfo IconInfo => _iconInfo;
        public ItemStackInfo ItemStackInfo => _itemStackInfo;
        public ItemPropertyFillBarInfo PropertyFillBarInfo => _propertyFillBarInfo;
        
        public void SetItemSlot(ItemSlot itemSlot)
        {
            if (_itemSlot == itemSlot)
                return;

            if (_itemSlot != null)
                _itemSlot.ItemChanged -= OnSlotChanged;

            _itemSlot = itemSlot;

            if (itemSlot != null)
            {
                itemSlot.ItemChanged += OnSlotChanged;
                UpdateInfo(itemSlot.Item);
            }
            else
                UpdateInfo(null);

            return;

            void OnSlotChanged(ItemSlot.CallbackArgs args) => UpdateInfo(_itemSlot.Item);
        }

        protected virtual void UpdateInfo(IItem item)
        {
            _iconInfo.UpdateInfo(item);
            _itemStackInfo.UpdateInfo(item);
            _propertyFillBarInfo.UpdateInfo(item);
        }

        private void OnDestroy() => SetItemSlot(null);
    }
}
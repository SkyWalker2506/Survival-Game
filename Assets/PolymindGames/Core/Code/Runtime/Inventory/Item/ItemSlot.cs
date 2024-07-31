using PolymindGames.OdinSerializer;
using UnityEngine;
using System;

namespace PolymindGames.InventorySystem
{
    /// <summary>
    /// Holds a reference to an item and listens to changes made to it.
    /// </summary>
    [Serializable]
    public sealed class ItemSlot
    {
        public enum CallbackType : byte
        {
            ItemAdded,
            ItemRemoved,
            StackChanged,
            PropertyChanged
        }
        
        public readonly struct CallbackArgs
        {
            public readonly ItemSlot Slot;
            public readonly CallbackType Type;

            public CallbackArgs(ItemSlot slot, CallbackType type)
            {
                Slot = slot;
                Type = type;
            }

            public override string ToString() => $"{Slot} | Context: {Type.ToString()}";
        }

        [OdinSerialize]
        private IItem _item;
        

        public ItemSlot()
            : this(null)
        { }

        public ItemSlot(IItem item)
        {
            _item = item;
            Container = null;
        }

        public ItemSlot(IItem item, IItemContainer container)
        {
            _item = item;
            Initialize(container);
        }

        public bool HasItem => _item != null;
        public bool HasContainer => Container != null;
        public IItemContainer Container { get; private set; }

        public IItem Item
        {
            get => _item;
            set
            {
                if (_item == value)
                    return;

                // Stop listening for changes to the previously attached item.
                if (_item != null)
                {
                    _item.PropertyChanged -= OnPropertyChanged;
                    _item.StackCountChanged -= OnStackChanged;
                }

                _item = value;

                // Start listening for changes to the newly attached item.
                if (_item != null)
                {
                    _item.PropertyChanged += OnPropertyChanged;
                    _item.StackCountChanged += OnStackChanged;

                    ItemChanged?.Invoke(new CallbackArgs(this, CallbackType.ItemAdded));
                }
                else
                    ItemChanged?.Invoke(new CallbackArgs(this, CallbackType.ItemRemoved));
            }
        }
        

        /// <summary> Sent when this slot has changed (e.g. when the attached item has changed).</summary>
        public event ItemSlotChangedDelegate ItemChanged;

        public void Initialize(IItemContainer container)
        {
            if (Container != null)
            {
                Debug.LogError("Container already initialized.");
                return;
            }

            if (container == null)
            {
                Debug.LogError("Cannot initialize slot with a null container.");
                return;
            }

            // Stop listening for changes to the previously attached item.
            if (_item != null)
            {
                _item.PropertyChanged += OnPropertyChanged;
                _item.StackCountChanged += OnStackChanged;
            }

            Container = container;
        }

        public override string ToString() => _item != null ? _item.ToString() : "No Item";
        
        private void OnStackChanged()
        {
            if (_item.StackCount == 0)
            {
                Item = null;
                return;
            }

            ItemChanged?.Invoke(new CallbackArgs(this, CallbackType.StackChanged));
        }

        private void OnPropertyChanged() => ItemChanged?.Invoke(new CallbackArgs(this, CallbackType.PropertyChanged));
    }

    public delegate void ItemSlotChangedDelegate(ItemSlot.CallbackArgs args);
}
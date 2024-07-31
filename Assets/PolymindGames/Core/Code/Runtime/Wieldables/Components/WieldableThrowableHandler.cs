using System.Collections.Generic;
using PolymindGames.InventorySystem;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    /// <summary>
    /// TODO: Implement
    /// </summary>
    public sealed class WieldableThrowableHandler : CharacterBehaviour, IWieldableThrowableHandlerCC
    {
        private sealed class ThrowableSlot
        {
            public readonly int ItemId;
            public readonly List<ItemSlot> Slots;


            public ThrowableSlot(int itemId, ItemSlot slot)
            {
                ItemId = itemId;
                Slots = new List<ItemSlot> { slot };
            }
        }

        [SerializeField, ReorderableList(ListStyle.Boxed)]
        [DataReferenceDetails(NullElementName = ItemTagDefinition.UNTAGGED, HasLabel = false)]
        private DataIdReference<ItemTagDefinition>[] _containerTags;

        private readonly List<IItemContainer> _containers = new();
        private readonly List<ThrowableSlot> _throwableSlots = new();
        private IWieldableInventory _selection;
        private IWieldableControllerCC _controller;
        private int _selectedIndex;


        public int SelectedIndex
        {
            get => _selectedIndex;
            private set
            {
                if (_selectedIndex != value)
                {
                    _selectedIndex = Mathf.Clamp(value, 0, _throwableSlots.Count - 1);
                    ThrowableIndexChanged?.Invoke();
                }
            }
        }

        public event UnityAction ThrowableCountChanged;
        public event UnityAction ThrowableIndexChanged;
        public event UnityAction<Throwable> OnThrow;
        
        public bool TryThrow()
        {
            if (!enabled || _controller.State != WieldableControllerState.None || _throwableSlots.Count == 0 || _throwableSlots[_selectedIndex].Slots.Count == 0)
                return false;

            var selectedSlot = _throwableSlots[_selectedIndex].Slots[0];
            var throwable = (Throwable)_selection.GetWieldableInstanceWithId(selectedSlot.Item.Id);
            if (_controller.TryEquipWieldable(throwable))
            {
                throwable.Use(WieldableInputPhase.Start);
                OnThrow?.Invoke(throwable);
                return true;
            }

            return false;
        }

        public void SelectNext(bool next)
        {
            float delta = next ? 1f : -1f;
            SelectedIndex = (int)Mathf.Repeat(_selectedIndex + delta, _throwableSlots.Count);
        }

        public Throwable GetThrowableAtIndex(int index)
        {
            if (_throwableSlots.Count == 0)
                return null;

            if (index >= _throwableSlots.Count || index < 0)
            {
                Debug.LogError("Index outside of range");
                return null;
            }

            if (_throwableSlots[index].Slots.Count > 0)
                return _selection.GetWieldableInstanceWithId(_throwableSlots[index].Slots[0].Item.Id) as Throwable;

            return null;
        }

        public int GetThrowableCountAtIndex(int index)
        {
            if (_throwableSlots.Count > index)
            {
                int count = 0;

                foreach (var slot in _throwableSlots[index].Slots)
                    count += slot.Item.StackCount;

                return count;
            }

            return 0;
        }

        protected override void OnBehaviourStart(ICharacter character)
        {
            _selection = character.GetCC<IWieldableInventory>();
            _controller = character.GetCC<IWieldableControllerCC>();

            for (int i = 0; i < _containerTags.Length; i++)
            {
                var containersWithTag = character.Inventory.GetContainersWithTag(_containerTags[i]);

                for (int j = 0; j < containersWithTag.Count; j++)
                    _containers.Add(containersWithTag[j]);
            }

            for (int i = 0; i < _containers.Count; i++)
            {
                var slots = _containers[i].Slots;

                for (int j = 0; j < slots.Count; j++)
                {
                    if (slots[j].HasItem)
                    {
                        int itemId = slots[j].Item.Id;
                        if (_selection.GetWieldableInstanceWithId(itemId) != null)
                            AddSlot(slots[j]);
                    }
                }
            }
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            for (var i = 0; i < _containers.Count; i++)
                _containers[i].SlotChanged += OnSlotChanged;
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            for (var i = 0; i < _containers.Count; i++)
                _containers[i].SlotChanged -= OnSlotChanged;
        }

        private void OnSlotChanged(ItemSlot.CallbackArgs args)
        {
            if (args.Slot.HasItem && _selection.GetWieldableWithId(args.Slot.Item.Id) is Throwable)
            {
                if (args.Type == ItemSlot.CallbackType.StackChanged)
                    OnThrowableCountChanged();

                if (AddSlot(args.Slot))
                    OnThrowableCountChanged();
            }
            else if (RemoveSlot(args.Slot))
                OnThrowableCountChanged();
        }

        private bool AddSlot(ItemSlot slot)
        {
            int itemId = slot.Item.Id;
            for (int i = 0; i < _throwableSlots.Count; i++)
            {
                if (_throwableSlots[i].ItemId == itemId)
                {
                    if (_throwableSlots[i].Slots.Contains(slot))
                        return false;

                    _throwableSlots[i].Slots.Add(slot);
                    return true;
                }
            }

            _throwableSlots.Add(new ThrowableSlot(itemId, slot));

            return true;
        }

        private bool RemoveSlot(ItemSlot slot)
        {
            for (int i = 0; i < _throwableSlots.Count; i++)
            {
                if (_throwableSlots[i].Slots.Remove(slot))
                    return true;
            }

            return false;
        }

        private void OnThrowableCountChanged()
        {
            ThrowableCountChanged?.Invoke();
            SelectedIndex = Mathf.Min(_selectedIndex, _throwableSlots.Count);
        }
    }
}
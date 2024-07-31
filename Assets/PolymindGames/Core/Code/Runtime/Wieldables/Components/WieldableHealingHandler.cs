using System;
using System.Collections.Generic;
using PolymindGames.InventorySystem;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    [RequireCharacterComponent(typeof(IWieldableInventory), typeof(IWieldableControllerCC))]
    [DefaultExecutionOrder(ExecutionOrderConstants.AFTER_DEFAULT_1)]
    public sealed class WieldableHealingHandler : CharacterBehaviour, IWieldableHealingHandlerCC
    {
        [SerializeField, ReorderableList(ListStyle.Boxed)]
        [DataReferenceDetails(NullElementName = ItemTagDefinition.UNTAGGED, HasLabel = false)]
        private DataIdReference<ItemTagDefinition>[] _containerTags;

        private readonly List<IItemContainer> _containers = new();
        private readonly List<ItemSlot> _healSlots = new();
        private IWieldableInventory _selection;
        private IWieldableControllerCC _controller;
        private HealingWieldable _healingWieldable;

        
        public int HealsCount { get; private set; }

        public event UnityAction<int> HealsCountChanged;

        public bool TryHeal()
        {
            if (!enabled
                || _healSlots.Count == 0
                || _healingWieldable != null
                || _controller.State != WieldableControllerState.None)
                return false;

            // No need to heal if the health is already full.
            float health = Character.HealthManager.Health;
            float maxHealth = Character.HealthManager.MaxHealth;
            if (maxHealth - health < 0.01f)
                return false;

            var selectedItem = _healSlots[0].Item;
            if (selectedItem != null)
            {
                _healingWieldable = (HealingWieldable)_selection.GetWieldableInstanceWithId(selectedItem.Id);
                if (_controller.TryEquipWieldable(_healingWieldable, 1f, Heal))
                    return true;
            }

            return false;
        }

        private void Heal()
        {
            // Start healing.
            _healingWieldable.Heal(OnHeal);
            var wieldableItem = _healingWieldable.GetComponent<WieldableItem>();
            wieldableItem.SetItem(_healSlots[0].Item);
            return;

            void OnHeal()
            {
                // Remove the healing item from the inventory.
                _healSlots[0].Item = null;

                // Holster the healing wieldable.
                _controller.TryHolsterWieldable(_healingWieldable);
            }
        }

        protected override void OnBehaviourStart(ICharacter character)
        {
            _selection = character.GetCC<IWieldableInventory>();
            _controller = character.GetCC<IWieldableControllerCC>();

            for (int i = 0; i < _containerTags.Length; i++)
                _containers.AddRange(character.Inventory.GetContainersWithTag(_containerTags[i]));

            for (int i = 0; i < _containers.Count; i++)
            {
                var containerSlots = _containers[i].Slots;

                for (int j = 0; j < containerSlots.Count; j++)
                {
                    var containerSlot = containerSlots[j];

                    if (containerSlot.HasItem && _selection.GetWieldableWithId(containerSlot.Item.Id) is HealingWieldable)
                        _healSlots.Add(containerSlots[j]);
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
            var callbackType = args.Type;

            switch (callbackType)
            {
                case ItemSlot.CallbackType.PropertyChanged:
                    return;
                case ItemSlot.CallbackType.ItemAdded:
                    var slot = args.Slot;
                    if (slot.HasItem && _selection.GetWieldableWithId(slot.Item.Id) is HealingWieldable)
                    {
                        _healSlots.Add(slot);
                        HealsCountChanged?.Invoke(CalculateHealsCount());
                    }
                    break;
                case ItemSlot.CallbackType.ItemRemoved:
                    if (_healSlots.Remove(args.Slot))
                        HealsCountChanged?.Invoke(CalculateHealsCount());
                    break;
                case ItemSlot.CallbackType.StackChanged:
                    if (_healSlots.Contains(args.Slot))
                        HealsCountChanged?.Invoke(CalculateHealsCount());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int CalculateHealsCount()
        {
            int count = 0;

            for (int i = 0; i < _healSlots.Count; i++)
            {
                var item = _healSlots[i].Item;
                if (item != null)
                    count += item.StackCount;
            }

            HealsCount = count;
            return count;
        }
    }
}
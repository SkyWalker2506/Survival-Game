using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class ItemActionsUI : CharacterUIBehaviour
    {
        [SerializeField, PrefabObjectOnly, BeginGroup]
        private ItemActionUI _template;

        [SerializeField, Range(1, 24), EndGroup]
        private int _templateCount = 5;

        private ItemActionUI[] _actionsUI;
        private bool _allDisabled;


        private void UpdateEnabledActions(ItemSlotUI slot)
        {
            // If the slot is null or doesn't have an item, clear all slots and return.
            if (slot == null || !slot.HasItem)
            {
                ClearSlots(0);
                return;
            }
    
            // Get the item definition from the slot's item.
            var definition = slot.Item.Definition;
    
            // Update the UI actions based on the item definition.
            UpdateUIActions(slot.ItemSlot, definition.Actions, definition.ParentGroup.BaseActions);
        }

        private void UpdateUIActions(ItemSlot slot, ItemAction[] mainActions, ItemAction[] baseActions)
        {
            // Update main actions
            SetActions(slot, mainActions, 0);

            // Update base actions
            SetActions(slot, baseActions, mainActions.Length);

            // Clear remaining UI slots
            ClearSlots(mainActions.Length + baseActions.Length);
        }

        private void SetActions(ItemSlot slot, ItemAction[] actions, int startIndex)
        {
            int endIndex = Mathf.Min(startIndex + actions.Length, _templateCount);
            for (int i = startIndex; i < endIndex; i++)
                _actionsUI[i].SetAction(actions[i - startIndex], slot, Character);
        }

        private void ClearSlots(int startIndex)
        {
            for (int i = startIndex; i < _actionsUI.Length; i++)
                _actionsUI[i].SetAction(null, null, null);
        }

        private void OnEnable() => ItemSelectorUI.Instance.SelectedSlotChanged += UpdateEnabledActions;
        private void OnDisable() => ItemSelectorUI.Instance.SelectedSlotChanged -= UpdateEnabledActions;

        protected override void Awake()
        {
            base.Awake();
            
            _actionsUI = new ItemActionUI[_templateCount];
            for (int i = 0; i < _templateCount; i++)
                _actionsUI[i] = Instantiate(_template, transform, false);
        }
    }
}
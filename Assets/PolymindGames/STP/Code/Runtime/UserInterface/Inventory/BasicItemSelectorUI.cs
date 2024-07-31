using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_1)]
    public sealed class BasicItemSelectorUI : ItemSelectorUI
    {
        [SerializeField, NotNull, BeginGroup, EndGroup]
        private SelectableGroupBaseUI _itemSlotsGroup;


        protected override void OnCharacterAttached(ICharacter character)
        {
            _itemSlotsGroup.SelectedChanged += SetSelectedSlot;
            _itemSlotsGroup.HighlightedChanged += SetHighlightedSlot;

            var inspection = character.GetCC<IInventoryInspectManagerCC>();
            inspection.AfterInspectionStarted += OnInspectionStarted;
            inspection.InspectionEnded += OnInspectionStopped;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            _itemSlotsGroup.SelectedChanged -= SetSelectedSlot;
            _itemSlotsGroup.HighlightedChanged -= SetHighlightedSlot;

            var inspection = character.GetCC<IInventoryInspectManagerCC>();
            inspection.AfterInspectionStarted -= OnInspectionStarted;
            inspection.InspectionEnded -= OnInspectionStopped;
        }

        private void OnInspectionStarted()
        {
            SetSelectedSlot(_itemSlotsGroup.Selected);
            RaiseSelectedEvent();
        }

        private void OnInspectionStopped() => SetSelectedSlot(null);

        private void SetSelectedSlot(SelectableUI selectable)
        {
            var slot = selectable == null ? null : selectable.GetComponent<ItemSlotUI>();

            if (SelectedSlot != null)
                SelectedSlot.ItemSlot.ItemChanged -= OnSlotChanged;

            if (slot != null && slot.HasItemSlot)
            {
                slot.ItemSlot.ItemChanged += OnSlotChanged;
                SelectedSlot = slot;
            }
            else
                SelectedSlot = null;
        }

        private void SetHighlightedSlot(SelectableUI highlighted)
        {
            var slot = highlighted == null ? null : highlighted.GetComponent<ItemSlotUI>();
            HighlightedSlot = slot;
        }

        private void OnSlotChanged(ItemSlot.CallbackArgs args) => RaiseSelectedEvent();
    }
}
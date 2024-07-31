using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.UserInterface
{
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_1)]
    public abstract class ItemSelectorUI : CharacterUIBehaviour
    {
        private ItemSlotUI _highlightedSlot;
        private ItemSlotUI _selectedSlot;
        
        
        public static ItemSelectorUI Instance { get; private set; }

        public ItemSlotUI SelectedSlot
        {
            get => _selectedSlot;
            protected set
            {
                if (_selectedSlot == value)
                    return;

                _selectedSlot = value;
                SelectedSlotChanged?.Invoke(value);
            }
        }

        public ItemSlotUI HighlightedSlot
        {
            get => _highlightedSlot;
            protected set
            {
                if (_highlightedSlot == value)
                    return;

                _highlightedSlot = value;
                HighlightedSlotChanged?.Invoke(value);
            }
        }

        public event UnityAction<ItemSlotUI> SelectedSlotChanged;
        public event UnityAction<ItemSlotUI> HighlightedSlotChanged;

        protected void RaiseSelectedEvent() => SelectedSlotChanged?.Invoke(SelectedSlot);

        protected override void Awake()
        {
            if (Instance == null)
                Instance = this;

            base.Awake();
        }
    }
}
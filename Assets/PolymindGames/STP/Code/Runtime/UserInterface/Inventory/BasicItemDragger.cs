using PolymindGames.InventorySystem;
using UnityEngine.UI;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/user-interface/behaviours/ui_inventory#item-drag-handler")]
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_1)]
    public sealed class BasicItemDragger : ItemDragger
    {
        [SerializeField, PrefabObjectOnly, BeginGroup]
        [Tooltip("Slot template prefab that will be instantiate when an item gets dragged.")]
        private ItemSlotUI _dragTemplate;

        [SerializeField, Range(1f, 100f), EndGroup]
        private float _dragSpeed = 15f;

        private RectTransform _parentRect;
        private ItemSlotUI _draggedSlot;
        private bool _splitItemStack;
        private Vector2 _dragOffset;
        private IItem _draggedItem;


        protected override void Awake()
        {
            base.Awake();
            
#if DEBUG
            if (_dragTemplate == null)
            {
                Debug.LogError("Drag template is null, you need to assign a prefab in the inspector.", gameObject);
                return;
            }
#endif

            _parentRect = (RectTransform)transform.parent;

            _draggedSlot = Instantiate(_dragTemplate, _parentRect, true);
            _draggedSlot.SetItemSlot(new ItemSlot());
            _draggedSlot.gameObject.SetActive(false);

            var selectable = _draggedSlot.Selectable;
            if (selectable.TryGetComponent<Graphic>(out var graphic))
                graphic.raycastTarget = false;

            selectable.IsSelectable = false;

            _dragTemplate = null;
        }

        public override void CancelDrag(ItemSlotUI initialSlot)
        {
            if (!IsDragging)
                return;
            
            _draggedSlot.gameObject.SetActive(false);
            _draggedItem = null;
            IsDragging = false;
        }

        public override void OnDragStart(ItemSlotUI initialSlot, Vector2 pointerPosition, bool splitItemStack = false)
        {
            if (IsDragging || !initialSlot.HasItem)
                return;

            IItem startSlotItem = initialSlot.Item;

            // Item Stack splitting
            if (splitItemStack && startSlotItem.StackCount > 1)
            {
                int initialAmount = startSlotItem.StackCount;
                int half = Mathf.FloorToInt(initialAmount / 2f);
                startSlotItem.StackCount = initialAmount - half;
                _splitItemStack = true;

                _draggedItem = new Item(startSlotItem, half);
            }
            else
            {
                _draggedItem = startSlotItem;
                _splitItemStack = false;
            }

            _draggedSlot.ItemSlot.Item = _draggedItem;
            _draggedSlot.gameObject.SetActive(true);

            if (RectTransformUtility.ScreenPointToWorldPointInRectangle(_parentRect, pointerPosition, null, out Vector3 worldPoint))
                _dragOffset = initialSlot.transform.position - worldPoint;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, pointerPosition, null, out Vector2 localPoint))
                _draggedSlot.transform.localPosition = localPoint + (Vector2)_parentRect.InverseTransformVector(_dragOffset);

            IsDragging = true;
        }

        public override void OnDrag(Vector2 pointerPosition)
        {
            if (!IsDragging)
                return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_parentRect, pointerPosition, null, out Vector2 localPoint))
            {
                float delta = Time.deltaTime * _dragSpeed;
                _draggedSlot.transform.localPosition = Vector3.Lerp(_draggedSlot.transform.localPosition, localPoint + (Vector2)_parentRect.InverseTransformVector(_dragOffset), delta);
                _dragOffset = Vector2.Lerp(_dragOffset, Vector2.zero, delta * 0.5f);
            }
        }

        public override void OnDragEnd(ItemSlotUI initialSlot, ItemSlotUI dropSlot, GameObject dropObject)
        {
            if (!IsDragging)
                return;

            // Is there a slot under our pointer?
            if (dropSlot != null)
            {
                // If dropped on the initial slot.
                if (dropSlot == initialSlot)
                    PutItemBack(initialSlot);

                // See if the slot allows this type of item.
                else if (!dropSlot.HasContainer || dropSlot.Container.AllowsItem(_draggedItem))
                {
                    bool removeOriginalItem = !_splitItemStack;

                    if (!dropSlot.HasItem) // If the slot is empty...
                        dropSlot.ItemSlot.Item = _draggedItem;
                    else // If the slot is not empty...
                    {
                        IItem itemUnderPointer = dropSlot.Item;

                        // Can we stack the items?
                        bool canStackItems = itemUnderPointer.Id == _draggedItem.Id &&
                                             itemUnderPointer.Definition.StackSize > 1 &&
                                             itemUnderPointer.StackCount < itemUnderPointer.Definition.StackSize;

                        if (canStackItems)
                            StackItems(dropSlot, initialSlot);
                        else
                        {
                            removeOriginalItem = false;
                            SwapItems(dropSlot, initialSlot);
                        }
                    }
                    
                    if (removeOriginalItem)
                        initialSlot.ItemSlot.Item = null;

                    if (ItemSelectorUI.Instance.SelectedSlot == initialSlot)
                        dropSlot.Selectable.Select();
                    else
                    {
                        if (ItemSelectorUI.Instance.SelectedSlot != null)
                            ItemSelectorUI.Instance.SelectedSlot.Selectable.Select();
                    }
                }
                else
                    PutItemBack(initialSlot);
            }

            // If the player dropped it on a UI object.
            else if (dropObject != null)
                PutItemBack(initialSlot);

            // Drop the item from the inventory...
            else
            {
                var slot = _splitItemStack ? _draggedSlot.ItemSlot : initialSlot.ItemSlot;
                PlayerUI.LocalUI.Character.Inventory.DropItem(slot);
            }

            _draggedSlot.gameObject.SetActive(false);
            _draggedItem = null;
            IsDragging = false;
        }

        // Stack the items.
        private void StackItems(ItemSlotUI underPointer, ItemSlotUI firstSlot)
        {
            int added = underPointer.ItemSlot.Item.AdjustStack(_draggedItem.StackCount);

            // Try to add the remaining items in the parent container.
            int remainedToAdd = _draggedItem.StackCount - added;
            if (remainedToAdd > 0)
            {
                if (firstSlot.HasItem)
                    underPointer.Container.AddItem(_draggedItem.Id, remainedToAdd);
                else
                    firstSlot.ItemSlot.Item = new Item(_draggedItem);
            }
        }

        // Swap the items because they are of different kinds / not stackable / reached maximum stack size.
        private void SwapItems(ItemSlotUI underPointer, ItemSlotUI firstSlot)
        {
            if (!firstSlot.Container.AllowsItem(underPointer.Item) || _splitItemStack)
            {
                PutItemBack(firstSlot);
                return;
            }

            IItem temp = underPointer.Item;
            underPointer.ItemSlot.Item = _draggedItem;
            firstSlot.ItemSlot.Item = temp;
        }

        private void PutItemBack(ItemSlotUI initialSlot)
        {
            if (_splitItemStack && initialSlot.HasItem)
                initialSlot.Item.AdjustStack(_draggedItem.StackCount);
            else
                initialSlot.ItemSlot.Item = _draggedItem;
        }
    }
}
namespace PolymindGames.InventorySystem
{
    public static class ItemSlotUtility
    {
        public static bool MoveItemToInventory(this ItemSlot slot, IInventory inventory)
        {
            if (!slot.HasItem)
                return false;
            
            int stack = slot.Item.StackCount;
            int addedCount = inventory.AddItem(slot.Item);

            if (addedCount == stack)
            {
                slot.Item = null;
                return true;
            }

            return false;
        }

        public static bool AddOrSwapItemWithContainer(this ItemSlot originalSlot, IItemContainer targetContainer)
        {
            if (!originalSlot.HasContainer)
                return false;

            bool isSingleStackItem = originalSlot.Item.Definition.StackSize == 1;
            return isSingleStackItem ? SwapSingleStackItem(originalSlot, targetContainer) : SwapMultipleStackItem(originalSlot, targetContainer);
        }

        public static bool SwapItemWithSlot(this ItemSlot originalSlot, ItemSlot targetSlot)
        {
            if (!originalSlot.HasContainer || !targetSlot.HasContainer)
                return false;

            return SwapSingleStackItem(originalSlot, targetSlot);
        }

        private static bool SwapSingleStackItem(ItemSlot originalSlot, IItemContainer targetContainer)
        {
            if (targetContainer.AddItem(originalSlot.Item) > 0)
            {
                originalSlot.Item = null;
                return true;
            }

            var targetSlot = targetContainer.GetFirstEmptySlotOrDefault();
            return SwapSingleStackItem(originalSlot, targetSlot);
        }

        /// <summary>
        /// TODO: Implement
        /// </summary>
        /// <param name="originalSlot"></param>
        /// <param name="targetContainer"></param>
        /// <returns></returns>
        private static bool SwapMultipleStackItem(ItemSlot originalSlot, IItemContainer targetContainer)
        {
            return SwapSingleStackItem(originalSlot, targetContainer);
        }

        private static bool SwapSingleStackItem(ItemSlot originalSlot, ItemSlot targetSlot)
        {
            var targetSlotItem = targetSlot.Item;
            var originalSlotItem = originalSlot.Item;

            // Clear the slots first (this is useful for not breaking the container restrictions for the next step)
            originalSlot.Item = null;
            targetSlot.Item = null;

            // Swap the items if the containers allow it
            if ((targetSlotItem == null || originalSlot.Container.AllowsItem(targetSlotItem)) &&
                (originalSlotItem == null || targetSlot.Container.AllowsItem(originalSlotItem)))
            {
                originalSlot.Item = targetSlotItem;
                targetSlot.Item = originalSlotItem;
                return true;
            }

            // Reverse the process if the containers don't allow it
            originalSlot.Item = originalSlotItem;
            targetSlot.Item = targetSlotItem;

            return false;
        }
    }
}
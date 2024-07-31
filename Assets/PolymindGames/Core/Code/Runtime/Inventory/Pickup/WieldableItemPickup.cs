using PolymindGames.WieldableSystem;

namespace PolymindGames.InventorySystem
{
    public class WieldableItemPickup : ItemPickup
    {
        protected override void OnInteracted(IInteractable interactable, ICharacter character)
        {
            var selection = character.GetCC<IWieldableInventory>();
            
            var holsterContainer = character.Inventory.GetContainerWithTag(WieldableItemConstants.WIELDABLE_TAG);
            if (holsterContainer != null && holsterContainer.IsFull())
                selection.DropWieldable(true);

            if (TryPickUpItem(character, AttachedItem))
            {
                if (holsterContainer != null)
                {
                    int index = holsterContainer.GetIndexOfItem(AttachedItem);
                    if (index != selection.SelectedIndex)
                        selection.SelectAtIndex(index);
                }
            }
        }

        protected override int TryAddItem(IInventory inventory, IItem item, out string rejectReason)
        {
            var holsterContainer = inventory.GetContainerWithTag(WieldableItemConstants.WIELDABLE_TAG);
            return holsterContainer != null
                ? holsterContainer.TryAddItem(item, out rejectReason) 
                : base.TryAddItem(inventory, item, out rejectReason);
        }
    }
}
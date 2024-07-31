using System.Collections;
using System.Linq;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Items/Actions/Equip Action", fileName = "ItemAction_Equip")]
    public sealed class EquipAction : ItemAction
    {
        [SerializeField, BeginGroup("Equipping"), EndGroup]
        private DataIdReference<ItemTagDefinition> _wieldableTag;


        public override float GetDuration(ItemSlot itemSlot, ICharacter character) => 0f;
        public override float GetDuration(IItem item, ICharacter character) => 0f;

        /// <summary>
        /// Checks if this item can be equipped.
        /// </summary>
        public override bool IsPerformable(ItemSlot itemSlot, ICharacter character)
        {
            bool isItemValid = itemSlot.HasItem && itemSlot.Item.Definition.Tag == _wieldableTag;
            bool isContainerValid = !itemSlot.HasContainer || !itemSlot.Container.HasTag(_wieldableTag);

            return isItemValid && isContainerValid;
        }

        public override bool IsPerformable(IItem item, ICharacter character)
        {
            return item != null && item.Definition.Tag == _wieldableTag;
        }

        protected override IEnumerator C_PerformAction(ICharacter character, ItemSlot itemSlot, float duration)
        {
            if (!TryGetHolsterContainer(character.Inventory, out var holsterContainer))
                yield break;

            EquipSlot(character, holsterContainer, itemSlot);
        }

        protected override IEnumerator C_PerformAction(ICharacter character, IItem item, float duration)
        {
            if (!TryGetHolsterContainer(character.Inventory, out var holsterContainer))
                yield break;
            
            if (character.Inventory.TryGetSlotOfItem(item, out var itemSlot))
            {
                // The item is part of the character's inventory.
                EquipSlot(character, holsterContainer, itemSlot);
            }
            else
            {
                // The item is NOT part of the character's inventory.
                if (holsterContainer.AddItem(item) > 0)
                {
                    int index = holsterContainer.GetIndexOfItem(item);
                    var selection = character.GetCC<IWieldableInventory>();
                    selection.SelectAtIndex(index);
                }
            }
        }

        private bool TryGetHolsterContainer(IInventory inventory, out IItemContainer container)
        {
            container = inventory.GetContainersWithTag(_wieldableTag).FirstOrDefault();
            return container != null;
        }

        private static void EquipSlot(ICharacter character, IItemContainer holsterContainer, ItemSlot itemSlot)
        {
            // If the parent container is null, return early.
            if (itemSlot.Container == null)
                return;
    
            // Get the wieldable selection handler from the character.
            var selection = character.GetCC<IWieldableInventory>();
    
            // If the parent container is the holster container.
            if (itemSlot.Container == holsterContainer)
            {
                // Get the index of the item in the holster container.
                int i = holsterContainer.GetIndexOfItem(itemSlot.Item);
        
                // If the item is in the holster container and it's not already selected, select it.
                if (i != -1 && i != selection.SelectedIndex)
                    selection.SelectAtIndex(i);
            }
            else
            {
                // Attempt to add the item to the holster container.
                if (holsterContainer.AddItem(itemSlot.Item) > 0)
                {
                    // Select the item in the holster container and clear the item slot.
                    selection.SelectAtIndex(holsterContainer.GetIndexOfItem(itemSlot.Item));
                    itemSlot.Item = null;
                }
                else
                {
                    // Swap the item slot's item with the selected item in the holster container.
                    itemSlot.SwapItemWithSlot(holsterContainer[selection.SelectedIndex]);
                }
            }
        }
    }
}
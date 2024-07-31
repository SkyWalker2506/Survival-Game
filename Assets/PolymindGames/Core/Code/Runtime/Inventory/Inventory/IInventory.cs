using System.Collections.Generic;
using UnityEngine.Events;

namespace PolymindGames.InventorySystem
{
    /// <summary> Represents an inventory system. </summary>
    public interface IInventory
    {
        /// <summary> Gets the list of item containers in the inventory. </summary>
        IReadOnlyList<IItemContainer> Containers { get; }

        /// <summary> Gets the list of item restrictions in the inventory. </summary>
        IReadOnlyList<ItemRestriction> Restrictions { get; }

        /// <summary> Event triggered when the inventory changes. </summary>
        event UnityAction InventoryChanged;

        /// <summary> Event triggered when an item in the inventory changes. </summary>
        event ItemSlotChangedDelegate ItemChanged;

        /// <summary> Event triggered when the containers in the inventory change. </summary>
        event ItemContainersChangedDelegate ContainersChanged;
        
        /// <summary>
        /// Adds a container to this inventory.
        /// </summary>
        void AddContainer(IItemContainer itemContainer);

        /// <summary>
        /// Removes a container from this inventory.
        /// </summary>
        void RemoveContainer(IItemContainer itemContainer);
        
        /// <summary>
        /// Removes and drops the item in the world.
        /// </summary>
        void DropItem(ItemSlot slot, float dropDelay = 0f);
        
        /// <summary>
        /// Drops the item into the world.
        /// </summary>
        void DropItem(IItem item, float dropDelay = 0f);
        
        /// <summary>
        /// Adds an item to the inventory.
        /// </summary>
		/// <returns> stack Added Count. </returns>
        int AddItem(IItem item);

        /// <summary>
        /// Adds an amount of items with the given id to the inventory.
        /// </summary>
        int AddItems(int id, int amountToAdd);

        /// <summary>
        /// Removes an item from the inventory.
        /// </summary>
        bool RemoveItem(IItem item);

        /// <summary>
        /// Removes an amount of items with the given id from the inventory.
        /// </summary>
        int RemoveItems(int id, int amount);

        /// <summary>
        /// Counts all the items with the given id, in all containers.
        /// </summary>
        int GetItemsCount(int id);

        /// <summary>
        /// Returns true if the item is found in any of the child containers.
        /// </summary>
        bool ContainsItem(IItem item);
        
        /// <summary>
        /// Checks if there's at least one item of the given id in the inventory.
        /// </summary>
        bool ContainsItem(int id);
    }
    
    public delegate void ItemContainersChangedDelegate(IItemContainer container, bool added);
}
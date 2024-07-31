using System.Collections.Generic;
using UnityEngine.Events;

namespace PolymindGames.InventorySystem
{
	/// <summary>
	/// Represents a container for items.
	/// </summary>
	public interface IItemContainer : IEnumerable<ItemSlot>
	{
		/// <summary> Gets the item slot at the specified index. </summary>
		ItemSlot this[int i] { get; }
    
		/// <summary> Gets the list of item slots in the container. </summary>
		IReadOnlyList<ItemSlot> Slots { get; }

		/// <summary> Gets the inventory that the container belongs to. </summary>
		IInventory Inventory { get; }

		/// <summary> Gets the name of the item container. </summary>
		string Name { get; }

		/// <summary> Gets the capacity of the item container. </summary>
		int Capacity { get; }

		/// <summary> Gets the array of item restrictions for the container. </summary>
		ItemRestriction[] Restrictions { get; }
		
		/// <summary>
		/// Raised when any of the attached items change, property changes will be ignored (no arguments).
		/// </summary>
		event UnityAction ContainerChanged;
		
		/// <summary>
		/// Raised when any of the attached items change, property changes will be ignored.
		/// </summary>
		event ItemSlotChangedDelegate SlotChanged;

		/// <summary>
		/// Raised right after the capacity changes.
		/// </summary>
		event ItemContainerCapacityChangedDelegate CapacityChanged;
		
		/// <summary>
		/// Sets the parent inventory.
		/// </summary>
		/// <param name="inventory"></param>
		void Initialize(IInventory inventory);

		/// <summary>
		/// Allows for resizing containers at runtime (e.g. increase the backpack slot count after upgrading it).
		/// </summary>
		/// <param name="capacity"></param>
		void SetCapacity(int capacity);
		
		/// <summary>
		/// Adds an item to this container.
		/// </summary>
		/// <param name="item"> Item to add. </param>
		/// <returns> Added count. </returns>
		int AddItem(IItem item);

		/// <summary>
		/// Creates and adds an amount of items with the given id to this container.
		/// </summary>
		/// <param name="id"> Item definition id. </param>
		/// <param name="amount"> Amount to add. </param>
		/// <returns> Added count. </returns>
		int AddItem(int id, int amount);

		/// <summary>
		/// Removes an item from this container (if found).
		/// </summary>
		/// <param name="item"> Item to remove. </param>
		/// <returns> True if removed, false otherwise. </returns>
		bool RemoveItem(IItem item);
		
		/// <summary>
		/// Removes an amount of items with the given id from this container.
		/// </summary>
		/// <param name="id"> Item definition name </param>
		/// <param name="amount"> Amount to remove </param>
		/// <returns> Removed count. </returns>
		int RemoveItem(int id, int amount);

		/// <summary>
		/// Counts and returns the amount of items with the given id.
		/// </summary>
		/// <param name="id"> Item definition id to search for. </param>
		/// <returns> Count. </returns>
		int GetItemCount(int id);

		/// <summary>
		/// Returns true if the item is found in any of the child slots.
		/// </summary>
		/// <param name="item"> Item to search for. </param>
		/// <returns></returns>
		bool ContainsItem(IItem item);

		/// <summary>
		/// Returns true if the item if an item of the given id is found in any of the child slots.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		bool ContainsItemWithId(int id);

		/// <summary>
		/// Checks if the given item can be added to this container.
		/// </summary>
		/// <param name="item"> Item to check. </param>
		/// <param name="count"></param>
		/// <returns> Allowed count. </returns>
		int GetAllowedCount(IItem item, int count);

		/// <summary>
		/// Checks if the given item can be added to this container.
		/// </summary>
		/// <param name="item"> Item to check. </param>
		/// <param name="count"></param>
		/// <param name="rejectReason"></param>
		/// <returns> Allowed count and a string rejection if the item is not allowed. </returns>
		int GetAllowedCount(IItem item, int count, out string rejectReason);
	}
	
	public delegate void ItemContainerCapacityChangedDelegate(int prevCapacity, int newCapacity);
}
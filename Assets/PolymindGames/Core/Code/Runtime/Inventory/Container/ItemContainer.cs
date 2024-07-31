using PolymindGames.OdinSerializer;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using System;

namespace PolymindGames.InventorySystem
{
    [Serializable]
	public sealed class ItemContainer : IItemContainer
	{
		[OdinSerialize]
		private string _name = DEFAULT_NAME;

		[OdinSerialize]
		private ItemSlot[] _slots;

		[OdinSerialize]
		private ItemRestriction[] _restrictions;

		private IInventory _inventory;
		private bool _isInitialized;
		
		private static readonly DummyItem s_DummyItem = new();
		private const string ITEM_IS_NULL_REJECTION = "Item Is NULL";
		private const string DEFAULT_NAME = "Container";
		private const int MAX_CAPACITY = 1024;

		
		#region Initialization
		/// <summary>
		/// Initializes a new instance of the ItemContainer class with the specified parameters.
		/// </summary>
		/// <param name="inventory">The inventory to which this container belongs.</param>
		/// <param name="name">The name of the container.</param>
		/// <param name="size">The size of the container.</param>
		/// <param name="restrictions">Optional item restrictions for the container.</param>
		public ItemContainer(IInventory inventory, string name, int size, params ItemRestriction[] restrictions)
		{
		    if (!string.IsNullOrEmpty(name))
		        _name = name;

		    // Initialize and populate the slots.
		    size = Mathf.Clamp(size, 0, MAX_CAPACITY);
		    _slots = CreateSlots(size);

		    _restrictions = restrictions;

		    Initialize(inventory);
		}

		/// <summary>
		/// Initializes and sets the inventory to which this container belongs.
		/// </summary>
		/// <param name="inventory">The inventory instance.</param>
		public void Initialize(IInventory inventory)
		{
		    if (_isInitialized)
		        return;

		    _inventory = inventory;

		    for (int i = 0; i < _slots.Length; i++)
		    {
		        var slot = _slots[i];
		        if (_slots[i].Container != this)
		            _slots[i].Initialize(this);
		        slot.ItemChanged += OnSlotChanged;
		    }

		    _restrictions ??= Array.Empty<ItemRestriction>();
		    for (int i = 0; i < _restrictions.Length; i++)
		    {
		        if (_restrictions[i].Container != this)
		            _restrictions[i].InitializeWithContainer(this);
		    }

		    _isInitialized = true;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the item slots in the container.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the item slots.</returns>
		IEnumerator<ItemSlot> IEnumerable<ItemSlot>.GetEnumerator() => ((IEnumerable<ItemSlot>)_slots).GetEnumerator();

		/// <summary>
		/// Returns an enumerator that iterates through the item slots in the container.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the item slots.</returns>
		IEnumerator IEnumerable.GetEnumerator() => _slots.GetEnumerator();

		/// <summary>
		/// Creates and returns an array of item slots with the specified length.
		/// </summary>
		/// <param name="length">The length of the array.</param>
		/// <returns>An array of item slots.</returns>
		private ItemSlot[] CreateSlots(int length)
		{
		    var slots = length > 0 ? new ItemSlot[length] : Array.Empty<ItemSlot>();
		    for (int i = 0; i < slots.Length; i++)
		        slots[i] = CreateSlot();

		    return slots;
		}

		/// <summary>
		/// Creates and returns a new item slot.
		/// </summary>
		/// <returns>The newly created item slot.</returns>
		private ItemSlot CreateSlot()
		{
		    var slot = new ItemSlot(null, this);
		    slot.ItemChanged += OnSlotChanged;
		    return slot;
		}

		/// <summary>
		/// Handles the slot change event and invokes the corresponding events.
		/// </summary>
		/// <param name="args">The event arguments.</param>
		private void OnSlotChanged(ItemSlot.CallbackArgs args)
		{
		    if (args.Type != ItemSlot.CallbackType.PropertyChanged)
		        ContainerChanged?.Invoke();

		    SlotChanged?.Invoke(args);
		}
		#endregion

		public ItemSlot this[int i] => _slots[i];
		public IReadOnlyList<ItemSlot> Slots => _slots;
		public int Capacity => _slots.Length;
		public string Name => _name;

		public ItemRestriction[] Restrictions => _restrictions;
		public IInventory Inventory => _inventory;

		public event UnityAction ContainerChanged;
		public event ItemSlotChangedDelegate SlotChanged;
		public event ItemContainerCapacityChangedDelegate CapacityChanged;
		
		public void SetCapacity(int capacity)
		{
			if (capacity == Capacity)
				return;

			var newSlotsArray = new ItemSlot[capacity];
			
			if (capacity > Capacity)
			{
				Array.Copy(_slots, newSlotsArray, Capacity);

				for (int i = Capacity; i < newSlotsArray.Length; i++)
					newSlotsArray[i] = CreateSlot();
			}
			else
			{
				for (int i = 0; i < newSlotsArray.Length; i++)
					newSlotsArray[i] = _slots[i];
			}

			int prevCapacity = _slots.Length;
			_slots = newSlotsArray;
			CapacityChanged?.Invoke(prevCapacity, capacity);
		}

		public int AddItem(int id, int amount)
		{
			if (ItemDefinition.TryGetWithId(id, out var itemDef))
				return AddItemWithDefinition(itemDef, amount);

			return 0;
		}

		public int AddItem(IItem item)
		{
			if (item == null || GetAllowedCount(item, item.StackCount) < 1)
				return 0;

			// If the item is stackable try to stack it with other items.
			if (item.Definition.StackSize > 1)
			{
				int stackAddedCount = AddItemWithDefinition(item.Definition, item.StackCount);
				item.StackCount -= stackAddedCount;
				
				return stackAddedCount;
			}
			
			// The item's not stackable, try find an empty slot for it.
			foreach (var slot in _slots)
			{
				if (!slot.HasItem)
				{
					slot.Item = item;
					return 1;
				}
			}

			return 0;
		}

		private int AddItemWithDefinition(ItemDefinition itemDef, int amount)
		{
			int allowedAmount = GetAllowedCount(itemDef, amount);
			int added = 0;
			
			// Go through each slot and see where we can add the item(s)
			for (var i = 0; i < _slots.Length; i++)
			{
				added += AddItemToSlot(_slots[i], itemDef, allowedAmount - added);

				// We've added all the items, we can stop now
				if (added == allowedAmount)
					return added;
			}

			return added;
		}

		private int AddItemToSlot(ItemSlot slot, ItemDefinition itemDef, int amount)
		{
			if (slot.HasItem)
			{
				// Return if the slot already has an item in it or the item is not of the same type.
				return itemDef.Id == slot.Item.Id
					? slot.Item.AdjustStack(amount) // Add to stack.
					: 0;
			}
			
			// If the slot is empty, create a new item.
			slot.Item = new Item(itemDef, amount);
			return slot.Item.StackCount;
		}

		public bool RemoveItem(IItem item)
		{
			for (var i = 0; i < _slots.Length; i++)
			{
				var slot = _slots[i];
				if (slot.Item == item)
				{
					slot.Item = null;
					return true;
				}
			}

			return false;
		}

		public int RemoveItem(int id, int amount)
		{
			int removed = 0;
			
			for (var i = 0; i < _slots.Length; i++)
			{
				var slot = _slots[i];
				
				if (!slot.HasItem || slot.Item.Id != id)
					continue;
				
				removed += slot.Item.AdjustStack(-(amount - removed));

				// We've removed all the items, we can stop now
				if (removed == amount)
					return removed;
			}

			return removed;
		}

		public bool ContainsItem(IItem item)
		{
			foreach (var slot in _slots)
			{
				if (slot.Item == item)
					return true;
			}

			return false;
		}

		public bool ContainsItemWithId(int id)
		{
			for (var i = 0; i < _slots.Length; i++)
			{
				var slot = _slots[i];
				if (slot.HasItem && slot.Item.Id == id)
					return true;
			}

			return false;
		}

		public int GetItemCount(int id)
		{
			int count = 0;

			for (var i = 0; i < _slots.Length; i++)
			{
				var slot = _slots[i];
				if (slot.HasItem && slot.Item.Id == id)
					count += slot.Item.StackCount;
			}

			return count;
		}

		public int GetAllowedCount(IItem item, int count)
		{
			if (item == null)
				return 0;

			int allowAmount = count;
			
			if (_inventory != null)
			{
				foreach (var restriction in _inventory.Restrictions)
				{
					allowAmount = restriction.GetAllowedAddAmount(item, allowAmount);
					if (allowAmount <= 0)
						return 0;
				}
			}

			foreach (var restriction in _restrictions)
			{
				allowAmount = restriction.GetAllowedAddAmount(item, allowAmount);
				if (allowAmount <= 0)
					return 0;
			}

			return allowAmount;
		}

		public int GetAllowedCount(IItem item, int count, out string rejectReason)
		{
			if (item == null)
			{
				rejectReason = ITEM_IS_NULL_REJECTION;
				return 0;
			}

			int allowAmount = count;
            foreach (var restriction in _inventory.Restrictions)
            {
                allowAmount = restriction.GetAllowedAddAmount(item, allowAmount);
                if (allowAmount <= 0)
                {
                    rejectReason = restriction.GetRejectionString();
                    return 0;
                }
            }

            foreach (var restriction in _restrictions)
			{
				allowAmount = restriction.GetAllowedAddAmount(item, allowAmount);

				if (allowAmount <= 0)
				{
					rejectReason = restriction.GetRejectionString();
					return 0;
				}
			}

			rejectReason = string.Empty;
			return allowAmount;
		}

		private int GetAllowedCount(ItemDefinition itemDef, int count)
		{
			if (itemDef == null)
				return 0;
			
			s_DummyItem.Definition = itemDef;
			return GetAllowedCount(s_DummyItem, count);
		}
    }
}
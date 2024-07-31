using System;
using System.Collections;
using System.Collections.Generic;
using PolymindGames.InventorySystem;

namespace PolymindGames
{
    public static partial class InventoryUtility
    {
        /// <summary>
        /// Returns a container with the given name.
        /// </summary>
        public static IItemContainer GetContainerWithName(this IInventory inventory, string name)
        {
            var containers = inventory.Containers;

            foreach (var container in containers)
            {
                if (container.Name == name)
                    return container;
            }

            return default(IItemContainer);
        }

        /// <summary>
        /// Adds an amount of items with the given name to the inventory.
        /// </summary>
        public static int AddItem(this IInventory inventory, string itemName, int amountToAdd)
        {
            if (amountToAdd <= 0)
                return 0;

            int addedInTotal = 0;
            var containers = inventory.Containers;

            foreach (var container in containers)
            {
                int added = container.AddItem(itemName, amountToAdd);
                addedInTotal += added;

                if (added == addedInTotal)
                    return addedInTotal;
            }

            return addedInTotal;
        }

        /// <summary>
        /// Removes an amount of items with the given name from the inventory.
        /// </summary>
        public static int RemoveItem(this IInventory inventory, string itemName, int amountToRemove)
        {
            if (amountToRemove <= 0)
                return 0;

            int removedInTotal = 0;
            var containers = inventory.Containers;

            foreach (var container in containers)
            {
                int removedNow = container.RemoveItem(itemName, amountToRemove);
                removedInTotal += removedNow;

                if (removedNow == removedInTotal)
                    return removedInTotal;
            }

            return removedInTotal;
        }

        /// <summary>
        /// Counts all the items with the given name, in all containers.
        /// </summary>
        public static int GetItemCount(this IInventory inventory, string itemName)
        {
            int count = 0;
            var containers = inventory.Containers;

            foreach (var container in containers)
                count += container.GetItemCount(itemName);

            return count;
        }

        /// <summary>
        /// Returns a list of the containers with the given container restriction type and a matching list of the restrictions.
        /// </summary>
        public static List<IItemContainer> GetContainersWithRestriction<T>(this IInventory inventory, out List<T> restrictions) where T : ItemRestriction
        {
            var containersWithRestriction = new List<IItemContainer>();
            restrictions = new List<T>();

            foreach (var container in inventory.Containers)
            {
                if (container.TryGetRestriction(out T restriction))
                {
                    containersWithRestriction.Add(container);
                    restrictions.Add(restriction);
                }
            }

            return containersWithRestriction;
        }

        /// <summary>
        /// Returns a list of the containers with the given container restriction type and a matching list of the restrictions.
        /// </summary>
        public static List<IItemContainer> GetContainersWithRestriction<T>(this IInventory inventory, out List<T> restrictions, Func<T, bool> validator) where T : ItemRestriction
        {
            var containersWithRestriction = new List<IItemContainer>();
            restrictions = new List<T>();

            foreach (var container in inventory.Containers)
            {
                if (container.TryGetRestriction(out T restriction) && validator(restriction))
                {
                    containersWithRestriction.Add(container);
                    restrictions.Add(restriction);
                }
            }

            return containersWithRestriction;
        }

        /// <summary>
        /// Returns the first container with the given tag.
        /// </summary>
        public static IItemContainer GetContainerWithTag(this IInventory inventory, DataIdReference<ItemTagDefinition> tag)
        {
            if (tag == DataIdReference<ItemTagDefinition>.NullRef)
                return GetContainerWithoutTags(inventory);

            foreach (var container in inventory.Containers)
            {
                if (container.TryGetRestriction(out ItemTagRestriction restriction) && ((IList)restriction.Tags).Contains(tag))
                    return container;
            }

            return null;
        }

        /// <summary>
        /// Returns a list of containers with the given tag.
        /// </summary>
        public static List<IItemContainer> GetContainersWithTag(this IInventory inventory, DataIdReference<ItemTagDefinition> tag)
        {
            if (tag == DataIdReference<ItemTagDefinition>.NullRef)
                return GetContainersWithoutTags(inventory);

            var containers = inventory.GetContainersWithRestriction<ItemTagRestriction>(out var tagRestrictions);

            int index = 0;

            while (index < containers.Count)
            {
                if (!((IList)tagRestrictions[index].Tags).Contains(tag))
                {
                    containers.RemoveAt(index);
                    tagRestrictions.RemoveAt(index);
                }
                else
                    index++;
            }

            return containers;
        }

        /// <summary>
        /// Returns the first container without any tags.
        /// </summary>
        public static IItemContainer GetContainerWithoutTags(this IInventory inventory)
        {
            foreach (var container in inventory.Containers)
            {
                if (!container.TryGetRestriction(out ItemTagRestriction tagRestriction) || tagRestriction.Tags.Length == 0)
                    return container;
            }

            return null;
        }

        /// <summary>
        /// Returns a list of containers without any tags.
        /// </summary>
        public static List<IItemContainer> GetContainersWithoutTags(this IInventory inventory)
        {
            var containersWithoutTag = new List<IItemContainer>();
            foreach (var container in inventory.Containers)
            {
                if (!container.TryGetRestriction(out ItemTagRestriction tagRestriction) || tagRestriction.Tags.Length == 0)
                    containersWithoutTag.Add(container);
            }

            return containersWithoutTag;
        }

        /// <summary>
        /// Returns the parent slot of the the item (if found).
        /// </summary>
        public static bool TryGetSlotOfItem(this IInventory inventory, IItem item, out ItemSlot itemSlot)
        {
            foreach (var container in inventory.Containers)
            {
                foreach (var slot in container.Slots)
                {
                    if (slot.Item == item)
                    {
                        itemSlot = slot;
                        return true;
                    }
                }
            }

            itemSlot = null;
            return false;
        }

        /// <summary>
        /// Tries to get a restriction of the given type from this inventory.
        /// </summary>
        /// <param name="inventory"> Target inventory. </param>
        /// <param name="restriction"> Output variable for the restriction. </param>
        /// <typeparam name="T"> Restriction type. </typeparam>
        /// <returns> True if found, false otherwise. </returns>
        public static bool TryGetRestriction<T>(this IInventory inventory, out T restriction) where T : ItemRestriction
        {
            Type type = typeof(T);
            var restrictions = inventory.Restrictions;

            foreach (var rest in restrictions)
            {
                if (rest.GetType() == type)
                {
                    restriction = (T)rest;
                    return true;
                }
            }

            restriction = null;
            return false;
        }

        public static int TryAddItem(this IInventory inventory, IItem item, out string rejectReason)
        {
            if (item.StackCount == 1)
            {
                rejectReason = string.Empty;
                
                foreach (var container in inventory.Containers)
                {
                    if (container.TryAddItem(item, out rejectReason) > 0)
                        return 1;
                }

                return 0;
            }

            int addedCount = 0;
            rejectReason = string.Empty;
            
            foreach (var container in inventory.Containers)
            {
                addedCount += container.TryAddItem(item, out rejectReason);
                if (addedCount == item.StackCount)
                    break;
            }

            return addedCount;
        }
    }
}
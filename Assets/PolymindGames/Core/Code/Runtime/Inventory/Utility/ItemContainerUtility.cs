using System;
using System.Collections;
using PolymindGames.InventorySystem;

namespace PolymindGames
{
    public static partial class ItemContainerUtility
    {
        /// <summary>
        /// Checks if this container has a restriction of the given type.
        /// </summary>
        /// <param name="container"> Target container. </param>
        /// <typeparam name="T"> Restriction type. </typeparam>
        /// <returns> True if found, false otherwise. </returns>
        public static bool HasRestriction<T>(this IItemContainer container) where T : ItemRestriction
        {
            Type type = typeof(T);
            var restrictions = container.Restrictions;

            foreach (var restriction in restrictions)
            {
                if (restriction.GetType() == type)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the restriction of the given type from this container.
        /// </summary>
        /// <param name="container"> Target container. </param>
        /// <typeparam name="T"> Restriction type. </typeparam>
        /// <returns> Restriction. </returns>
        public static T GetRestriction<T>(this IItemContainer container) where T : ItemRestriction
        {
            Type type = typeof(T);
            var restrictions = container.Restrictions;

            foreach (var restriction in restrictions)
            {
                if (restriction.GetType() == type)
                    return (T)restriction;
            }

            return null;
        }

        /// <summary>
        /// Tries to get a restriction of the given type from this container.
        /// </summary>
        /// <param name="container"> Target container. </param>
        /// <param name="restriction"> Output variable for the restriction. </param>
        /// <typeparam name="T"> Restriction type. </typeparam>
        /// <returns> True if found, false otherwise. </returns>
        public static bool TryGetRestriction<T>(this IItemContainer container, out T restriction) where T : ItemRestriction
        {
            Type type = typeof(T);
            var restrictions = container.Restrictions;

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

        public static bool HasTag(this IItemContainer container, DataIdReference<ItemTagDefinition> tag)
        {
            if (container.TryGetRestriction<ItemTagRestriction>(out var tagRestriction))
                return ((IList)tagRestriction.Tags).Contains(tag);

            return false;
        }

        /// <summary>
        /// Creates and adds an amount of items with the given name to this container.
        /// </summary>
        /// <param name="container"> Target container. </param>
        /// <param name="name"> Item definition name. </param>
        /// <param name="amount"> Amount to add. </param>
        /// <returns> Added count. </returns>
        public static int AddItem(this IItemContainer container, string name, int amount)
        {
            if (ItemDefinition.TryGetWithName(name, out var itemDef))
                return container.AddItem(itemDef.Id, amount);

            return 0;
        }

        /// <summary>
        /// Removes an amount of items with the given name from this container.
        /// </summary>
        /// <param name="container"> Target container. </param>
        /// <param name="name"> Item definition name </param>
        /// <param name="amount"> Amount to remove </param>
        /// <returns> Removed count. </returns>
        public static int RemoveItem(this IItemContainer container, string name, int amount)
        {
            int removed = 0;
            var slots = container.Slots;

            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.Item.Name == name)
                {
                    removed += slot.Item.AdjustStack(-(amount - removed));

                    // We've removed all the items, we can stop now
                    if (removed == amount)
                        return removed;
                }
            }

            return removed;
        }

        /// <summary>
        /// Counts and returns the amount of items with the given name.
        /// </summary>
        /// <param name="container"> Target container. </param>
        /// <param name="name"> Item definition name to search for. </param>
        /// <returns> Count. </returns>
        public static int GetItemCount(this IItemContainer container, string name)
        {
            int count = 0;
            var slots = container.Slots;

            foreach (var slot in slots)
            {
                if (slot.HasItem && slot.Item.Name == name)
                    count += slot.Item.StackCount;
            }

            return count;
        }

        public static int TryAddItem(this IItemContainer container, IItem item, out string rejectReason)
        {
            if (container.GetAllowedCount(item, item.StackCount, out rejectReason) > 0)
                return container.AddItem(item);

            return 0;
        }

        /// <summary>
        /// Finds and returns the last empty slot, if there's no empty slot, it'll return the first slot instead.
        /// </summary>
        /// <param name="container"></param>
        /// <returns></returns>
        public static ItemSlot GetFirstEmptySlotOrDefault(this IItemContainer container)
        {
            var slots = container.Slots;
            int count = slots.Count;

            for (int i = 0; i < count; i++)
            {
                if (!slots[i].HasItem)
                    return slots[i];
            }

            return slots[0];
        }

        public static int GetIndexOfItem(this IItemContainer container, IItem item)
        {
            if (item == null)
                return -1;

            var slots = container.Slots;
            for (var i = 0; i < slots.Count; i++)
            {
                if (slots[i].Item == item)
                    return i;
            }

            return -1;
        }

        public static bool IsFull(this IItemContainer container)
        {
            foreach (var slot in container.Slots)
            {
                if (!slot.HasItem)
                    return false;
            }

            return true;
        }
        
        public static bool IsEmpty(this IItemContainer container)
        {
            foreach (var slot in container.Slots)
            {
                if (slot.HasItem)
                    return false;
            }

            return true;
        }

        public static bool AllowsItem(this IItemContainer container, IItem item)
        {
            if (item == null)
                return false;

            return container.GetAllowedCount(item, item.StackCount) > 0;
        }
    }
}
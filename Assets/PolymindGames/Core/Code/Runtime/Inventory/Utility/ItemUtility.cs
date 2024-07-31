using UnityEngine;

namespace PolymindGames.InventorySystem
{
    public static partial class ItemUtility
    {
        /// <summary>
        /// Adjusts the stack count of the given item by a given amount.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="amount"></param>
        /// <returns>How much has the stack count changed (0-"amount").</returns>
        public static int AdjustStack(this IItem item, int amount)
        {
            int prevStack = item.StackCount;
            item.StackCount += amount;

            return Mathf.Abs(prevStack - item.StackCount);
        }

        /// <summary>
        /// Returns true if the item has a property with the given id.
        /// </summary>
        public static bool HasPropertyWithId(this IItem item, int id)
        {
            var properties = item.Properties;

            foreach (var prop in properties)
            {
                if (prop.Id == id)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Returns true if the item has a property with the given name.
        /// </summary>
        public static bool HasPropertyWithName(this IItem item, string name)
        {
            var properties = item.Properties;

            foreach (var prop in properties)
            {
                if (prop.Name == name)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Use this if you are sure the item has this property.
        /// </summary>
        public static ItemProperty GetPropertyWithId(this IItem item, int id)
        {
            var properties = item.Properties;

            foreach (var prop in properties)
            {
                if (prop.Id != id)
                    continue;

                return prop;
            }

            return null;
        }

        /// <summary>
        /// Use this if you are sure the item has this property.
        /// </summary>
        public static ItemProperty GetPropertyWithName(this IItem item, string name)
        {
            var properties = item.Properties;

            foreach (var prop in properties)
            {
                if (prop.Name != name)
                    continue;

                return prop;
            }

            return null;
        }

        /// <summary>
        /// Use this if you are NOT sure the item has this property.
        /// </summary>
        public static bool TryGetPropertyWithId(this IItem item, int id, out ItemProperty itemProperty)
        {
            var properties = item.Properties;

            foreach (var prop in properties)
            {
                if (prop.Id != id)
                    continue;

                itemProperty = prop;
                return true;
            }

            itemProperty = null;
            return false;
        }

        /// <summary>
        /// Use this if you are NOT sure the item has this property.
        /// </summary>
        public static bool TryGetPropertyWithName(this IItem item, string name, out ItemProperty itemProperty)
        {
            var properties = item.Properties;

            foreach (var prop in properties)
            {
                if (prop.Name != name)
                    continue;

                itemProperty = prop;
                return true;
            }

            itemProperty = null;
            return false;
        }
    }
}
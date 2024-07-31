using System;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [Serializable]
    public abstract class ItemRestriction
    {
        protected enum RestrictionMode : byte
        {
            Container = 1,
            Inventory = 2
        }
        
        public IItemContainer Container { get; private set; }
        public IInventory Inventory { get; private set; }
        public bool IsInitialized { get; private set; }

        protected RestrictionMode RestrictionType => Container != null
            ? RestrictionMode.Container
            : RestrictionMode.Inventory;
        

        public void InitializeWithContainer(IItemContainer container)
        {
            if (IsInitialized)
            {
                Debug.LogError("This restriction has already been initialized.");
                return;
            }

            if (Container != null)
            {
                Debug.LogError("This restriction has already been initialized; invoke this method if the restriction has not been created via its constructor (i.e., Save & Load)");
                return;
            }

            if (container == null)
            {
                Debug.LogError("Cannot initialize restriction with a null container.");
                return;
            }

            IsInitialized = true;
            Container = container;
            OnInitializedWithContainer(container);
        }

        public void InitializeWithInventory(IInventory inventory)
        {
            if (IsInitialized)
            {
                Debug.LogError("This restriction has already been initialized.");
                return;
            }

            if (Inventory != null)
            {
                Debug.LogError("Ensure that this restriction is initialized; invoke this method if the restriction has not been created via its constructor (i.e., Save & Load)");
                return;
            }

            if (inventory == null)
            {
                Debug.LogError("Cannot initialize restriction with a null inventory.");
                return;
            }

            IsInitialized = true;
            Inventory = inventory;
            OnInitializedWithInventory(inventory);
        }

        /// <summary>
        /// Returns the amount of items that can be added of the given item and count.
        /// </summary>
        public abstract int GetAllowedAddAmount(IItem item, int count);

        /// <summary>
        /// Returns a string that specifies why the item cannot be added.
        /// </summary>
        public virtual string GetRejectionString() => "Inventory Is Full";

        protected virtual void OnInitializedWithContainer(IItemContainer container) { }
        protected virtual void OnInitializedWithInventory(IInventory inventory) { }
    }
}
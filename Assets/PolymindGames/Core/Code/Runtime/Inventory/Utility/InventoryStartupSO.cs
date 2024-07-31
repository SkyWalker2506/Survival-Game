using System;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Items/Inventory Startup", fileName = "InventoryStartup_", order = 10)]
    public sealed class InventoryStartupSO : ScriptableObject
    {
        [SerializeField, LabelFromChild("Container.Name")]
        [ReorderableList(ListStyle.Lined, HasHeader = false)]
        private Data[] _data;


        public void AddContainersForInventory(IInventory inventory)
        {
            if (inventory == null)
            {
                Debug.LogError("Inventory is null.", this);
                return;
            }

            foreach (var data in _data)
            {
                var container = data.Container.GenerateContainer(inventory);
                inventory.AddContainer(container);

                foreach (var item in data.Items)
                    container.AddItem(item.GenerateItem());
            }
        }

        [Serializable]
        private sealed class Data
        {
            [IgnoreParent, BeginGroup]
            public ContainerGenerator Container;

            [IgnoreParent, EndGroup] 
            [ReorderableList(ListStyle.Lined)]
            public ItemGenerator[] Items;
        }
    }
}
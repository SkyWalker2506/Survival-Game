using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class StorageStationUI : WorkstationInspectorBaseUI<StorageStation>
    {
        [SerializeField, BeginGroup("Storage")]
        private ItemContainerUI _itemContainer;

        [SerializeField, NotNull, EndGroup]
        private ButtonUI _takeAllButton;


        protected override void OnInspectionStarted(StorageStation workstation)
        {
            var container = workstation.GetContainers()[0];
            container.ContainerChanged += OnContainerChanged;
            _itemContainer.AttachToContainer(container);
            OnContainerChanged();
        }

        protected override void OnInspectionEnded(StorageStation workstation)
        {
            _itemContainer.Container.ContainerChanged -= OnContainerChanged;
            _itemContainer.DetachFromContainer();
        }

        private void OnContainerChanged() => _takeAllButton.IsSelectable = !_itemContainer.Container.IsEmpty();
        private void Start() => _takeAllButton.OnSelected += TakeAllItems;

        private void TakeAllItems()
        {
            var container = _itemContainer.Container;
            var inventory = Character.Inventory;

            foreach (var itemSlot in container)
                itemSlot.MoveItemToInventory(inventory);
        }
    }
}
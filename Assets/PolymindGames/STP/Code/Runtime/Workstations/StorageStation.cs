using System.Collections.Generic;
using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/interaction/interactable/demo-interactables")]
    public sealed class StorageStation : Workstation, ISaveableComponent
    {
        [SerializeField, Range(0, 100), BeginGroup("Storage")]
        [Tooltip("How many slots should this storage crate have.")]
        private int _storageSpots;

        [SerializeField, Tooltip("Can a character add items to this storage.")]
        private bool _canAddItems = true;

        [SerializeField, IgnoreParent, SpaceArea, EndGroup]
        [ReorderableList(ListStyle.Lined, Foldable = true)]
        private ItemGenerator[] _initialItems;

        private IItemContainer[] _containers;


        public override IItemContainer[] GetContainers()
        {
            _containers ??= new[]
            {
                GenerateContainer()
            };

            return _containers;
        }

        private IItemContainer GenerateContainer()
        {
            var container = new ItemContainer(null, nameof(StorageStation), _storageSpots, GetContainerRestrictions());

            foreach (var itemGenerator in _initialItems)
                container.AddItem(itemGenerator.GenerateItem());

            _initialItems = null;
            return container;
        }

        private ItemRestriction[] GetContainerRestrictions()
        {
            var restrictions = new List<ItemRestriction>();

            if (!_canAddItems)
                restrictions.Add(new ItemAddRestriction());

            return restrictions.ToArray();
        }

        #region Save & Load
        void ISaveableComponent.LoadMembers(object data)
        {
            if (data is ItemContainer container)
            {
                _containers = new IItemContainer[]
                {
                    container
                };
                _initialItems = null;
                container.Initialize(null);
            }
        }

        object ISaveableComponent.SaveMembers() => _containers?[0] as ItemContainer;
        #endregion
    }
}
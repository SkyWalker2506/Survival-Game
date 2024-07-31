using System;
using System.Collections.Generic;
using PolymindGames.InventorySystem;
using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    public sealed class FuelSelectorUI : MonoBehaviour
    {
        [SerializeField, BeginGroup("References")]
        private ButtonUI _nextBtn;

        [SerializeField]
        private ButtonUI _previousBtn;

        [SerializeField, EndGroup]
        private Image _iconImg;

        private FuelItem[] _fuelItems = Array.Empty<FuelItem>();
        private IInventory _inventory;
        private int _selectedFuelIdx;

        
        public FuelItem SelectedFuel { get; private set; }

        public void AttachToInventory(IInventory inventory)
        {
            _inventory = inventory;
            inventory.InventoryChanged += OnInventoryChanged;
            OnInventoryChanged();
        }

        public void DetachFromInventory()
        {
            _inventory.InventoryChanged -= OnInventoryChanged;
            _inventory = null;
        }

        private void Awake()
        {
            CacheFuelItems();
            _selectedFuelIdx = 0;

            _nextBtn.OnSelected += () => SelectNextFuel(true);
            _previousBtn.OnSelected += () => SelectNextFuel(false);

            SelectFuelAtIndex(0);
        }

        private void OnInventoryChanged()
        {
            RefreshFuelList();

            if (_selectedFuelIdx == -1 || _fuelItems[_selectedFuelIdx].Count == 0)
                SelectNextFuel(true);
        }

        private void SelectNextFuel(bool selectNext)
        {
            RefreshFuelList();

            bool foundValidFuel = false;
            int iterations = 0;
            int i = _selectedFuelIdx;

            do
            {
                i = (int)Mathf.Repeat(i + (selectNext ? 1 : -1), _fuelItems.Length);
                iterations++;

                if (_fuelItems[i].Count > 0)
                {
                    foundValidFuel = true;
                    _selectedFuelIdx = i;
                }
            } while (!foundValidFuel && iterations < _fuelItems.Length);

            _selectedFuelIdx = foundValidFuel ? i : -1;
            SelectFuelAtIndex(_selectedFuelIdx);
        }

        private void SelectFuelAtIndex(int index)
        {
            if (_fuelItems == null || _fuelItems.Length < 1)
                return;

            _iconImg.enabled = index > -1;

            if (index > -1)
            {
                SelectedFuel = _fuelItems[index];

                if (ItemDefinition.TryGetWithId(SelectedFuel.Item, out var itemDef))
                    _iconImg.sprite = itemDef.Icon;
            }
        }

        private void CacheFuelItems()
        {
            var fuelItems = new List<FuelItem>();

            foreach (var itemDef in ItemDefinition.Definitions)
            {
                if (itemDef.TryGetDataOfType(out FuelData fuel))
                    fuelItems.Add(new FuelItem(itemDef.Id, 0, fuel.FuelEfficiency));
            }

            _fuelItems = fuelItems.ToArray();
        }

        private void RefreshFuelList()
        {
            foreach (var item in _fuelItems)
                item.Count = 0;

            var containers = _inventory.Containers;
            foreach (var container in containers)
            {
                if (container.HasRestriction<ItemTagRestriction>())
                    return;

                for (int i = 0; i < container.Capacity; i++)
                {
                    IItem item = container.Slots[i].Item;
                    if (item != null && item.Definition.HasDataOfType(typeof(FuelData)) && TryGetFuelItem(item.Id, out FuelItem fuelItem))
                        fuelItem.Count += item.StackCount;
                }
            }
        }

        private bool TryGetFuelItem(int itemId, out FuelItem fuelItem)
        {
            foreach (var item in _fuelItems)
            {
                if (item.Item == itemId)
                {
                    fuelItem = item;
                    return true;
                }
            }

            fuelItem = null;
            return false;
        }

        #region Internal
        public sealed class FuelItem
        {
            public readonly int Duration;

            public int Count;
            public DataIdReference<ItemDefinition> Item;


            public FuelItem(int id, int count, int duration)
            {
                Item = id;
                Count = count;
                Duration = duration;
            }
        }
        #endregion
    }
}
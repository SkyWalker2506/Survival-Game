using UnityEngine.Events;
using UnityEngine;
using System;

namespace PolymindGames.InventorySystem
{
    [Serializable]
    public sealed class ItemWeightRestriction : ItemRestriction
    {
        [SerializeField]
        private float _totalWeight;

        [SerializeField]
        private float _maxWeight;
        
        
        public ItemWeightRestriction(float maxWeight)
        {
            _maxWeight = maxWeight;
        }

        public ItemWeightRestriction(float maxWeight, IItemContainer container)
        {
            _maxWeight = maxWeight;
            InitializeWithContainer(container);
        }

        public ItemWeightRestriction(float maxWeight, IInventory inventory)
        {
            _maxWeight = maxWeight;
            InitializeWithInventory(inventory);
        }
        
        public float MaxWeight => _maxWeight;
        public float TotalWeight
        {
            get => _totalWeight;
            private set
            {
                _totalWeight = value;
                WeightChanged?.Invoke(_totalWeight);
            }
        }
        
        public event UnityAction<float> WeightChanged;
        
        public override int GetAllowedAddAmount(IItem item, int count)
        {
            int allowCount = count;

            if (count == 1)
            {
                if (_totalWeight + item.TotalWeight * count > _maxWeight)
                    return 0;
            }
            else
            {
                allowCount = (int)Mathf.Clamp(count, 0f, (_maxWeight - _totalWeight) / item.TotalWeight);

                if (allowCount == 0)
                    return 0;
            }

            return allowCount;
        }

        public override string GetRejectionString()
        {
            return RestrictionType switch
            {
                RestrictionMode.Container => "Weight capacity reached.",
                RestrictionMode.Inventory => "Can't carry more weight.",
                _ => string.Empty
            };
        }

        protected override void OnInitializedWithContainer(IItemContainer container)
        {
            container.ContainerChanged += RecalculateWeight;
            RecalculateWeight();

            void RecalculateWeight()
            {
                float weight = 0f;
                var slots = Container.Slots;
                foreach (var slot in slots)
                {
                    if (slot.HasItem)
                        weight += slot.Item.TotalWeight;
                }

                TotalWeight = weight;
            }
        }

        protected override void OnInitializedWithInventory(IInventory inventory)
        {
            inventory.InventoryChanged += RecalculateWeight;
            RecalculateWeight();

            void RecalculateWeight()
            {
                float weight = 0f;
                foreach (var container in Inventory.Containers)
                {
                    foreach (var slot in container.Slots)
                    {
                        if (slot.HasItem)
                            weight += slot.Item.TotalWeight;
                    }
                }

                TotalWeight = weight;
            }
        }
    }
}
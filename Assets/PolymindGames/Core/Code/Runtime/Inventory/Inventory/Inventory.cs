using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System;

namespace PolymindGames.InventorySystem
{
    [DisallowMultipleComponent]
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/player/modules-and-behaviours/inventory")]
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_3)]
    public sealed class Inventory : CharacterBehaviour, IInventory, ISaveableComponent
    {
        [SerializeField, Range(0f, 1000f), BeginGroup, EndGroup]
        [Tooltip("The maximum weight capacity of the inventory.")]
        private float _maxWeight = 30f;

        [SerializeField, InLineEditor, BeginGroup, EndGroup]
        [Tooltip("The startup data for initializing the inventory.")]
        private InventoryStartupSO _startupData;
        
        [SerializeField, BeginGroup, EndGroup]
        private DropSettings _dropSettings;

        private List<IItemContainer> _containers;
        private ItemRestriction[] _restrictions;
        
        
        public IReadOnlyList<IItemContainer> Containers => _containers;
        public IReadOnlyList<ItemRestriction> Restrictions => _restrictions;

        public event UnityAction InventoryChanged;
        public event ItemSlotChangedDelegate ItemChanged;
        public event ItemContainersChangedDelegate ContainersChanged;
        
        public void AddContainer(IItemContainer container)
        {
#if DEBUG
            if (container == null)
            {
                Debug.LogError("Container is null.", gameObject);
                return;
            }

            if (_containers.Contains(container))
            {
                Debug.LogWarning("Container is already added.", gameObject);
                return;
            }
#endif

            _containers.Add(container);

            container.ContainerChanged += OnInventoryChanged;
            container.SlotChanged += OnItemChanged;

            InventoryChanged?.Invoke();
            ContainersChanged?.Invoke(container, true);
        }

        public void RemoveContainer(IItemContainer container)
        {
#if DEBUG
            if (!_containers.Remove(container))
            {
                Debug.LogWarning("Container is not added.", gameObject);
                return;
            }
#endif

            container.ContainerChanged -= OnInventoryChanged;
            container.SlotChanged -= OnItemChanged;

            InventoryChanged?.Invoke();
            ContainersChanged?.Invoke(container, false);
        }

        public void DropItem(ItemSlot slot, float dropDelay)
        {
            CoroutineUtils.InvokeDelayed(this, DropItem, slot.Item, dropDelay);
            slot.Item = null;
        }

        public void DropItem(IItem item, float dropDelay) =>
            CoroutineUtils.InvokeDelayed(this, DropItem, item, dropDelay);

        public int AddItem(IItem item)
        {
            int addedInTotal = 0;

            foreach (var container in _containers)
            {
                addedInTotal += container.AddItem(item);

                if (addedInTotal >= item.StackCount)
                    break;
            }

            return addedInTotal;
        }

        public int AddItems(int id, int amountToAdd)
        {
            if (amountToAdd <= 0)
                return 0;

            int addedInTotal = 0;

            foreach (var container in _containers)
            {
                int added = container.AddItem(id, amountToAdd);
                addedInTotal += added;

                if (added == addedInTotal)
                    break;
            }

            return addedInTotal;
        }

        public bool RemoveItem(IItem item)
        {
            foreach (var container in _containers)
            {
                if (container.RemoveItem(item))
                    return true;
            }

            return false;
        }

        public int RemoveItems(int id, int amount)
        {
            if (amount <= 0)
                return 0;

            int removeCount = 0;

            foreach (var container in _containers)
            {
                int removedNow = container.RemoveItem(id, amount);
                removeCount += removedNow;

                if (removeCount == amount)
                    break;
            }
            
            return removeCount;
        }

        public int GetItemsCount(int id)
        {
            int count = 0;
            foreach (var container in _containers)
                count += container.GetItemCount(id);

            return count;
        }

        public bool ContainsItem(IItem item)
        {
            foreach (var container in _containers)
            {
                if (container.ContainsItem(item))
                    return true;
            }

            return false;
        }
        
        public bool ContainsItem(int itemId)
        {
            foreach (var container in _containers)
            {
                if (container.ContainsItemWithId(itemId))
                    return true;
            }

            return false;
        }

        private void RemoveAllContainers()
        {
            if (_containers == null)
                return;

            foreach (var container in _containers)
            {
                container.ContainerChanged -= OnInventoryChanged;
                container.SlotChanged -= OnItemChanged;
            }

            _containers.Clear();
        }
        
        private void DropItem(IItem item)
        {
            // Determine the pickup prefab based on item stack count.
            var pickupPrefab = item.StackCount < 2 ? item.Definition.Pickup : item.Definition.StackPickup;
            if (pickupPrefab == null)
                pickupPrefab = _dropSettings.GenericPickup;

            // Check if the pickup prefab is null.
            if (pickupPrefab == null)
            {
                Debug.LogError("The generic pickup cannot be null.", this);
                return;
            }

            // Drop the item and link it with the pickup instance.
            var pickupInstance = Instantiate(pickupPrefab);
            Character.DropObject(pickupInstance, _dropSettings.DropPoint, _dropSettings.DropForce);
            pickupInstance.LinkWithItem(item);

            // Play drop audio.
            Character.AudioPlayer.PlaySafe(_dropSettings.DropAudio, BodyPoint.Torso);
        }

        protected override void OnBehaviourStart(ICharacter character)
        {
            // Don't create new containers if they were already created by the save system.
            if (_containers != null)
                return;

            _containers = new List<IItemContainer>();

            _restrictions ??= new ItemRestriction[]
            {
                new ItemWeightRestriction(_maxWeight, this)
            };

            if (_startupData != null)
            {
                _startupData.AddContainersForInventory(this);
                _startupData = null;
            }
        }

        private void OnInventoryChanged() => InventoryChanged?.Invoke();
        private void OnItemChanged(ItemSlot.CallbackArgs args) => ItemChanged?.Invoke(args);

        #region Save & Load
        [Serializable]
        private sealed class SaveData
        {
            public List<IItemContainer> Containers;
            public ItemRestriction[] Restrictions;
        }

        void ISaveableComponent.LoadMembers(object data)
        {
            var saveData = (SaveData)data;

            RemoveAllContainers();
            _containers ??= new List<IItemContainer>();
            
            foreach (var savedContainer in saveData.Containers)
            {
                savedContainer.Initialize(this);
                AddContainer(savedContainer);
            }

            _restrictions = saveData.Restrictions;
            foreach (var restriction in _restrictions)
                restriction.InitializeWithInventory(this);
        }

        object ISaveableComponent.SaveMembers()
        {
            return new SaveData
            {
                Containers = _containers,
                Restrictions = _restrictions
            };
        }
        #endregion

        #region Internal
        [Serializable]
        private struct DropSettings
        {
            public Transform DropPoint;
            
            [Range(0f, 1000f)] 
            public float DropForce;
            
            [InLineEditor]
            public AudioDataSO DropAudio;
            
            [AssetPreview, NotNull, Line]
            [Tooltip("The prefab used when an item that's being dropped doesn't have a pickup (e.g. clothes) or when dropping multiple items at once.")]
            public ItemPickup GenericPickup;
        }
        #endregion
    }
}
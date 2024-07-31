using PolymindGames.InventorySystem;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine.Events;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolymindGames.WieldableSystem
{
    using Debug = UnityEngine.Debug;
    
    /// <summary>
    /// Takes care of selecting wieldables based on inventory items.
    /// </summary>
    [RequireCharacterComponent(typeof(IWieldableControllerCC))]
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_1)]
    public sealed class WieldableInventory :CharacterBehaviour, IWieldableInventory, ISaveableComponent, IEditorFixable
    {
        [SerializeField, BeginGroup]
        [Help("Found in the Inventory Component.")]
        [Tooltip("The corresponding inventory container (e.g. holster, backpack etc.) that this behaviour will use for selecting items.")]
        private string _holsterContainer = "Holster";

        [SerializeField, Range(0, 10), EndGroup]
        private int _startingSlotIndex = 0;

        [SerializeField, Range(0.5f, 5f), BeginGroup]
        private float _dropHolsterSpeed = 1.45f;

        [SerializeField, Range(0f, 10f)]
        private float _dropDelay = 0.35f;
        
        [SerializeField, EndGroup]
        private bool _dropOnDeath;

        [SerializeField, DisableInPlayMode]
        [ReorderableList(ListStyle.Boxed, HasLabels = false, Foldable = true), PrefabObjectOnly]
        [EditorButton(nameof(LoadAllWieldables), "Find all wieldable prefabs", ButtonActivityType.OnEditMode)]
        private WieldableItem[] _wieldablePrefabs = Array.Empty<WieldableItem>();

        private Dictionary<DataIdReference<ItemDefinition>, WieldableItem> _wieldableItems;
        private IWieldableControllerCC _controller;
        private IWieldable _equippedWieldable;
        private IItemContainer _holster;
        private int _selectedIndex = -1;


        public int SelectedIndex
        {
            get => _selectedIndex;
            private set
            {
                if (_selectedIndex == value)
                    return;
                
                PreviousIndex = _selectedIndex;
                _selectedIndex = value;
                SelectedIndexChanged?.Invoke(value);
            }
        }
        
        public int PreviousIndex { get; private set; } = -1;

        public event UnityAction<int> SelectedIndexChanged;

        public void SelectAtIndex(int index)
        {
            index = Mathf.Clamp(index, -1, _holster.Capacity - 1);
            if (index == SelectedIndex && _equippedWieldable != null)
            {
                ReequipWieldable();
                return;
            }

            UnsubscribeFromItemChanged(PreviousIndex);
            SubscribeToItemChanged(index);
            SelectedIndex = index;
            
            IItem item = index != -1 ? _holster[index].Item : null;
            EquipWieldable(item);

            void SubscribeToItemChanged(int idx)
            {
                if (idx != -1)
                    _holster[idx].ItemChanged += OnItemChanged;
            }

            void UnsubscribeFromItemChanged(int idx)
            {
                if (idx != -1)
                    _holster[idx].ItemChanged -= OnItemChanged;
            }

            void OnItemChanged(ItemSlot.CallbackArgs args)
            {
                if (args.Type != ItemSlot.CallbackType.PropertyChanged)
                {
                    int indexToSelect = Mathf.Clamp(index, -1, _holster.Capacity - 1);
                    EquipWieldable(_holster[indexToSelect]?.Item, _dropHolsterSpeed);
                }
            }
            
            void ReequipWieldable()
            {
                if (_controller.ActiveWieldable != _equippedWieldable)
                    EquipWieldable(SelectedIndex != -1 ? _holster[SelectedIndex].Item : null);
            }
        }

        public void DropWieldable(bool forceDrop = false)
        {
            if (SelectedIndex == -1 || !forceDrop && _controller.State != WieldableControllerState.None)
                return;
            
            ItemSlot slot = _holster[SelectedIndex];
            if (slot.HasItem && _wieldableItems.TryGetValue(slot.Item.Id, out var wieldable))
            {
                // Drop inventory wieldable.
                if (wieldable.Wieldable == _controller.ActiveWieldable)
                {
                    EquipWieldable(null, _dropHolsterSpeed);
                    Character.Inventory.DropItem(slot, _dropDelay);
                }
            }
        }

        public IWieldable GetWieldableWithId(int id)
        {
            if (_wieldableItems.TryGetValue(id, out var wieldableItem))
                return wieldableItem.Wieldable;

            return null;
        }

        public IWieldable GetWieldableInstanceWithId(int id)
        {
            return TryGetWieldableWithId(id, out var wieldableItem) ? wieldableItem.Wieldable : default(IWieldable);
        }

        private void EquipWieldable(IItem item, float holsterSpeed = 1f)
        {
            if (_equippedWieldable != null)
            {
                _controller.TryHolsterWieldable(_equippedWieldable, holsterSpeed);
                _equippedWieldable = null;
            }
            
            if (item != null && TryGetWieldableWithId(item.Id, out var wieldableItem))
            {
                _equippedWieldable = wieldableItem.Wieldable;
                _controller.TryEquipWieldable(_equippedWieldable, holsterSpeed, () => wieldableItem.SetItem(item));
            }
        }

        private bool TryGetWieldableWithId(DataIdReference<ItemDefinition> id, out WieldableItem wieldableItem)
        {
            if (id.IsNull)
            {
                wieldableItem = null;
                return false;
            }

            if (_wieldableItems.TryGetValue(id, out wieldableItem))
            {
                // If it's a prefab, spawn and register it first.
                if (wieldableItem.IsPrefab())
                {
                    wieldableItem = _controller.RegisterWieldable(wieldableItem.GetComponent<IWieldable>())
                        .gameObject.GetComponent<WieldableItem>();
                    
                    _wieldableItems[id] = wieldableItem;
                }

                return true;
            }

            return false;
        }

        #region Initialization
        protected override void OnBehaviourStart(ICharacter character)
        {
            _controller = character.GetCC<IWieldableControllerCC>();
            _holster ??= Character.Inventory.GetContainerWithName(_holsterContainer);
            
            InitializeWieldablesCache();

            var health = Character.HealthManager;
            health.Respawn += OnRespawn;
            health.Death += OnDeath;
            
            // If the index is -1, we can use the starting index since it indicates this is the first time this component has been active.
            SelectAtIndex(_selectedIndex == -1 ? _startingSlotIndex : _selectedIndex);
        }

        protected override void OnBehaviourDestroy(ICharacter character)
        {
            var health = Character.HealthManager;
            health.Respawn -= OnRespawn;
            health.Death -= OnDeath;
        }

        private void OnDeath(in DamageArgs args)
        {
            if (_dropOnDeath)
                DropWieldable(true);
            
            SelectAtIndex(-1);
        }
        
        private void OnRespawn()
        {
            SelectAtIndex(PreviousIndex);
        }

        private void InitializeWieldablesCache()
        {
            var existingWieldables = _controller.WieldableParent.gameObject.GetComponentsInFirstChildren<WieldableItem>();
            int allWieldablesCount = existingWieldables.Count + _wieldablePrefabs.Length;
            _wieldableItems = new Dictionary<DataIdReference<ItemDefinition>, WieldableItem>(allWieldablesCount);

            foreach (var wieldableItem in existingWieldables)
            {
                if (IsValid(wieldableItem))
                    Add(wieldableItem);
            }

            foreach (var prefab in _wieldablePrefabs)
            {
                if (IsValid(prefab))
                    Add(prefab);
            }

            _wieldablePrefabs = null;
            return;

            static bool IsValid(WieldableItem wieldableItem)
            {
                if (wieldableItem == null || !wieldableItem.gameObject.HasComponent<IWieldable>() ||
                    wieldableItem.ReferencedItem.IsNull)
                {
                    Debug.LogWarning("Wieldable Item or Wieldable is null.", wieldableItem);
                    return false;
                }

                return true;
            }

            void Add(WieldableItem wieldableItem)
            {
                if (wieldableItem == null)
                {
                    Debug.LogError("You're trying to instantiate a null wieldable.", gameObject);
                    return;
                }

                if (_wieldableItems.ContainsKey(wieldableItem.ReferencedItem))
                {
                    Debug.LogWarning("You're trying to instantiate a wieldable with an id that has already been added.", gameObject);
                    return;
                }

                _wieldableItems.Add(wieldableItem.ReferencedItem, wieldableItem);

                if (!wieldableItem.IsPrefab())
                    _controller.RegisterWieldable(wieldableItem.Wieldable);
            }
        }
        #endregion

        #region Save & Load
        [Serializable]
        private sealed class SaveData
        {
            public int SelectedIndex;
            public int PreviousIndex;
        }

        void ISaveableComponent.LoadMembers(object data)
        {
            var saveData = (SaveData)data;
            _selectedIndex = saveData.SelectedIndex;
            PreviousIndex = saveData.PreviousIndex;
        }

        object ISaveableComponent.SaveMembers() => new SaveData()
        {
            SelectedIndex = SelectedIndex,
            PreviousIndex = PreviousIndex
        };
        #endregion

        #region Editor
        [Conditional("UNITY_EDITOR")]
        private void LoadAllWieldables()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;

            var allPrefabs = AssetDatabase.FindAssets("t: prefab");

            var wieldables = new List<WieldableItem>();
            foreach (var guid in allPrefabs)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (obj.TryGetComponent(out WieldableItem wItem))
                    wieldables.Add(wItem);
            }

            _wieldablePrefabs = wieldables.ToArray();
            EditorUtility.SetDirty(this);
#endif
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_wieldablePrefabs != null && Array.IndexOf(_wieldablePrefabs, null) != -1)
            {
                UnityUtils.SafeOnValidate(this, () =>
                    ArrayUtility.RemoveAt(ref _wieldablePrefabs, Array.IndexOf(_wieldablePrefabs, null)));
            }
        }
#endif
        #endregion

        void IEditorFixable.Fix()
        {
#if UNITY_EDITOR
            LoadAllWieldables();
#endif
        }
    }
}
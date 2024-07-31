using System.Collections.Generic;
using PolymindGames.InventorySystem;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    [DefaultExecutionOrder(ExecutionOrderConstants.AFTER_DEFAULT_2)]
    public sealed class CraftingUI : CharacterUIBehaviour
    {
        [SerializeField, NotNull, BeginGroup("Levels")]
        private SelectableUI _defaultLevel;

        [SerializeField, NotNull]
        private SelectableUI _customLevel;

        [SerializeField, NotNull, EndGroup]
        private TextMeshProUGUI _customLevelText;

        [SerializeField, NotNull, BeginGroup("Items")]
        private Transform _itemsSpawnRoot;

        [SerializeField, NotNull]
        private CraftingSlotUI _itemSlotTemplate;

        [SerializeField, Range(5, 24), EndGroup]
        private int _maxTemplateInstanceCount = 12;

        /// <summary>
        /// <para> Key: Crafting level. </para>
        /// Value: List of items that correspond to the crafting level.
        /// </summary>
        private readonly Dictionary<int, List<ItemDefinition>> _craftableItemsDictionary = new();
        
        private CraftingSlotUI[] _cachedSlots;
        private int _craftingItemsCount;
        private int _customCraftingLevel;


        public void SetCustomLevel(int level, string levelName)
        {
            _customCraftingLevel = level;
            _customLevelText.text = levelName;
            _customLevel.gameObject.SetActive(true);
            _customLevel.Select();
        }

        public void DisableCustomLevel()
        {
            _customLevel.gameObject.SetActive(false);
        }

        protected override void Awake()
        {
            base.Awake();

            // Initialize the crafting dictionary and slots
            InitializeCraftableItems();
            InitializeCraftingSlots();

            // Subscribe to the events for selected crafting level change
            _defaultLevel.OnSelected += () => SetCraftingLevel(0);
            _customLevel.OnSelected += () => SetCraftingLevel(_customCraftingLevel);
            
            _defaultLevel.gameObject.SetActive(true);
            _customLevel.gameObject.SetActive(false);
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            var inventoryInspection = character.GetCC<IInventoryInspectManagerCC>();
            inventoryInspection.InspectionStarted += OnInspectionStarted;
            inventoryInspection.InspectionEnded += OnInspectionEnded;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            var inventoryInspection = character.GetCC<IInventoryInspectManagerCC>();
            inventoryInspection.InspectionStarted -= RefreshSlots;
            inventoryInspection.InspectionEnded -= RefreshSlots;
        }

        private void OnInspectionStarted()
        {
            Character.Inventory.InventoryChanged += RefreshSlots;
            
            if (!_customLevel.isActiveAndEnabled)
                _defaultLevel.ForceSelect();
        }

        private void OnInspectionEnded()
        {
            Character.Inventory.InventoryChanged -= RefreshSlots;
        }

        private void RefreshSlots()
        {
            foreach (var slot in _cachedSlots)
            {
                // Check if the slot is active
                if (slot.gameObject.activeSelf)
                {
                    // Update the displayed item in the slot
                    slot.SetItem(slot.ItemDef);
                }
            }
        }

        private void SetCraftingLevel(int level)
        {
            if (_craftableItemsDictionary.TryGetValue(level, out var items))
                SetCurrentlyCraftableItems(items);
        }

        private void SetCurrentlyCraftableItems(List<ItemDefinition> items)
        {
            // Determine how many slots to enable based on the number of items and available UI slots
            int enableCount = Mathf.Min(items.Count, _cachedSlots.Length);

            // Set the items to the enabled slots and activate them
            for (int i = 0; i < enableCount; i++)
            {
                var cachedSlot = _cachedSlots[i];
                cachedSlot.SetItem(items[i]);
                cachedSlot.gameObject.SetActive(true);
            }

            // Hide any remaining slots that are not used
            for (int i = enableCount; i < _cachedSlots.Length; i++)
            {
                var cachedSlot = _cachedSlots[i];
                cachedSlot.SetItem(null);
                cachedSlot.gameObject.SetActive(false);
            }
        }

        private void InitializeCraftableItems()
        {
            _craftableItemsDictionary.Clear();
            _craftingItemsCount = 0;

            foreach (var item in ItemDefinition.Definitions)
            {
                if (item.TryGetDataOfType<CraftingData>(out var data) && data.IsCraftable)
                {
                    if (!_craftableItemsDictionary.TryGetValue(data.CraftLevel, out var list))
                    {
                        list = new List<ItemDefinition>();
                        _craftableItemsDictionary[data.CraftLevel] = list;
                    }

                    list.Add(item);
                    _craftingItemsCount++;
                }
            }
        }
        
        private void InitializeCraftingSlots()
        {
            int instancesCount = Mathf.Min(_maxTemplateInstanceCount, _craftingItemsCount);
            _cachedSlots = new CraftingSlotUI[instancesCount];

            var spawnRoot = _itemsSpawnRoot.transform;
            for (int i = 0; i < instancesCount; i++)
            {
                var slot = Instantiate(_itemSlotTemplate, spawnRoot);
                slot.SetItem(null);
                slot.Selectable.OnSelectableSelected += _ => StartCrafting(slot);
                _cachedSlots[i] = slot;
            }

            return;

            void StartCrafting(CraftingSlotUI slot)
            {
                if (Character == null)
                {
                    Debug.LogWarning("This behaviour is not attached to a character.", gameObject);
                    return;
                }

                Character.GetCC<ICraftingManagerCC>().Craft(slot.ItemDef);
            }
        }
    }
}
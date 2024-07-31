using PolymindGames.InventorySystem;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace PolymindGames.BuildingSystem
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/interaction/interactable/demo-interactables")]
    public sealed class RepairStation : Workstation, ISaveableComponent
    {
        [SerializeField, Range(0f, 25f), BeginGroup("Repairing")]
        [Tooltip("The time it takes to repair an item at this station.")]
        private float _repairDuration = 1f;

        [SerializeField]
        [Tooltip("The id of the durability property. After repairing an item the workbench will increase the value of that property for the repaired item.")]
        private DataIdReference<ItemPropertyDefinition> _durabilityProperty;

        [SerializeField, EndGroup]
        [Tooltip("Repair sound to be played after successfully repairing an item.")]
        private AudioDataSO _repairAudio;

        private const float MAX_DURABILITY = 100f;
        
        private List<CraftRequirement> _repairRequirements;
        private IItemContainer[] _containers;
        private ItemSlot _itemSlot;


        public IReadOnlyList<CraftRequirement> RepairRequirements => _repairRequirements;
        public IItem ItemToRepair => _itemSlot.Item;
        public float RepairDuration => _repairDuration;

        public override IItemContainer[] GetContainers()
        {
            _containers ??= new[]
            {
                GenerateContainer()
            };

            return _containers;
        }

        /// <summary>
        /// Checks if the attached item can be repaired
        /// </summary>
        public bool CanRepairItem()
        {
            if (ItemToRepair == null)
                return false;

            // Get the durability property of the item
            ItemProperty durability = ItemToRepair.GetPropertyWithId(_durabilityProperty);

            // Check if the item's durability is less than MAX_DURABILITY
            return durability.Float < MAX_DURABILITY;
        }

        public void RepairItem(ICharacter character)
        {
            // Remove required items from the character's inventory
            foreach (var req in _repairRequirements)
                character.Inventory.RemoveItems(req.Item, req.Amount);

            // Set the durability of the item to maximum (100)
            ItemToRepair.GetPropertyWithId(_durabilityProperty).Float = MAX_DURABILITY;

            // Play repair audio if available
            if (_repairAudio != null)
                AudioManager.Instance.PlayClipAtPoint(_repairAudio.Clip, transform.position, _repairAudio.Volume);
        }

        private IItemContainer GenerateContainer()
        {
            var container = new ItemContainer(null, nameof(RepairStation), 1, new ItemPropertyRestriction(_durabilityProperty), new ItemDataRestriction(typeof(CraftingData)));
            container.SlotChanged += OnSlotChanged;
            
            _repairRequirements = new List<CraftRequirement>();
            _itemSlot = container[0];

            return container;
        }

        private void OnSlotChanged(ItemSlot.CallbackArgs args)
        {
            // Clear the list of repair requirements
            _repairRequirements.Clear();
            
            var item = args.Slot.Item;
            if (item == null)
                return;

            // Get the current durability of the item
            var durabilityProperty = item.GetPropertyWithId(_durabilityProperty);
            float durability = durabilityProperty.Float;

            if (Math.Abs(durability - MAX_DURABILITY) < 0.01f)
                return;

            // Calculate and add repair requirements based on crafting data blueprint
            var craftData = item.Definition.GetDataOfType<CraftingData>();
            foreach (var requirement in craftData.Blueprint)
            {
                // Calculate the required amount based on the current durability
                int requiredAmount = Mathf.Max(Mathf.RoundToInt(requirement.Amount * Mathf.Clamp01((100f - durability) / 100f)), 1);
            
                // Add the repair requirement to the list
                if (requiredAmount > 0)
                    _repairRequirements.Add(new CraftRequirement(requirement.Item, requiredAmount));
            }
        }

        #region Save & Load
        void ISaveableComponent.LoadMembers(object data)
        {
            if (data is ItemContainer container)
            {
                _repairRequirements = new List<CraftRequirement>();

                _containers = new IItemContainer[]
                {
                    container
                };

                container.Initialize(null);
                container.SlotChanged += OnSlotChanged;
                OnSlotChanged(new ItemSlot.CallbackArgs(container[0], ItemSlot.CallbackType.ItemAdded));
            }
        }

        object ISaveableComponent.SaveMembers() => _containers?[0];
        #endregion
    }
}
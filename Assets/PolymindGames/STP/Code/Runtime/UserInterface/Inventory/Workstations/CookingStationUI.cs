using PolymindGames.BuildingSystem;
using PolymindGames.WorldManagement;
using TMPro;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class CookingStationUI : WorkstationInspectorBaseUI<CookingStation>
    {
        [SerializeField, BeginGroup("References")]
        private FuelSelectorUI _fuelSelector;

        [SerializeField]
        private ItemContainerUI _itemContainer;

        [SerializeField]
        private ButtonUI _startFireBtn;

        [SerializeField]
        private ButtonUI _addFuelBtn;

        [SerializeField]
        private ButtonUI _extinguishBtn;

        [SerializeField, EndGroup]
        private TextMeshProUGUI _descriptionText;

        private const string STARTING_FIRE = "Starting Fire...";
        private const string EXTINGUISH_FIRE = "Extinguishing Fire...";
        

        protected override void OnInspectionStarted(CookingStation workstation)
        {
            // Attach item container to the workstation container
            _itemContainer.AttachToContainer(workstation.GetContainers()[0]);
            
            // Attach fuel selector to the character's inventory
            _fuelSelector.AttachToInventory(Character.Inventory);

            // Subscribe to events for updating description and buttons
            workstation.CookingUpdated += UpdateDescription;
            workstation.CookingStarted += UpdateButtons;
            workstation.CookingStopped += UpdateButtons;

            // Update description and buttons
            UpdateDescription();
            UpdateButtons();
        }

        protected override void OnInspectionEnded(CookingStation workstation)
        {
            // Detach item container from the workstation container
            _itemContainer.DetachFromContainer();
            
            // Detach fuel selector from the character's inventory
            _fuelSelector.DetachFromInventory();

            // Unsubscribe from events
            workstation.CookingUpdated -= UpdateDescription;
            workstation.CookingStarted -= UpdateButtons;
            workstation.CookingStopped -= UpdateButtons;
        }

        private void Start()
        {
            // Subscribe button events
            _startFireBtn.OnSelected += StartCooking;
            _extinguishBtn.OnSelected += StopCooking;
            _addFuelBtn.OnSelected += AddFuel;
        }

        /// <summary>
        /// Starts the cooking process if fuel is selected.
        /// </summary>
        private void StartCooking()
        {
            if (_fuelSelector.SelectedFuel == null)
                return;

            if (Character.Inventory.ContainsItem(_fuelSelector.SelectedFuel.Item))
            {
                float delay = Workstation.QueueStartCooking() + 0.01f;
                CustomActionManagerUI.Instance.StartAction(new CustomActionArgs(STARTING_FIRE, delay, true, AddFuel, Workstation.CancelQueues));
            }
        }

        /// <summary>
        /// Stops the cooking process.
        /// </summary>
        private void StopCooking()
        {
            float delay = Workstation.QueueStopCooking() + 0.01f;
            CustomActionManagerUI.Instance.StartAction(new CustomActionArgs(EXTINGUISH_FIRE, delay, true, UpdateDescription, Workstation.CancelQueues));
        }

        /// <summary>
        /// Updates the visibility of buttons based on the cooking status.
        /// </summary>
        private void UpdateButtons()
        {
            bool isCookingActive = Workstation.IsCookingActive;
            _startFireBtn.gameObject.SetActive(!isCookingActive);
            _addFuelBtn.gameObject.SetActive(isCookingActive);
            _extinguishBtn.IsSelectable = isCookingActive;
        }

        /// <summary>
        /// Updates the description text based on the cooking station description.
        /// </summary>
        private void UpdateDescription()
        {
            _descriptionText.text = Workstation.IsCookingActive
                ? $"Duration: {WorldExtensions.FormatMinuteWithSuffixes(Workstation.CookingTimeLeft)}"
                : string.Empty;
        }

        /// <summary>
        /// Adds fuel to the cooking station.
        /// </summary>
        private void AddFuel()
        {
            if (_fuelSelector.SelectedFuel == null)
                return;

            if (Character.Inventory.RemoveItems(_fuelSelector.SelectedFuel.Item, 1) > 0)
            {
                Workstation.AddFuel(_fuelSelector.SelectedFuel.Duration);
                UpdateDescription();
            }
        }
    }
}
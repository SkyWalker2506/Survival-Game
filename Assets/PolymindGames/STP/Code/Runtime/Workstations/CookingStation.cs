using PolymindGames.InventorySystem;
using PolymindGames.WorldManagement;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.BuildingSystem
{
    /// <summary>
    /// Convert the cooking to item actions.
    /// </summary>
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/interaction/interactable/demo-interactables")]
    public sealed class CookingStation : Workstation, ISaveableComponent
    {
        [BeginGroup("Cooking")]
        [SerializeField, Range(1, 10), DisableInPlayMode]
        [Tooltip("How many cooking spots this campfire has.")]
        private int _cookingSpots = 3;

        [SerializeField, Range(0f, 10f)]
        private float _cookingStartDuration = 2.5f;

        [SerializeField, Range(0f, 10f)]
        private float _cookingStopDuration = 1.5f;
        
        [SerializeField, Range(1f, 1000f)]
        private int _maxFuelTime = 500;
        
        [SerializeField, Range(0.01f, 10f)]
        [Tooltip("Multiplies the effects of any fuel added.")]
        private float _fuelDurationMod = 1f;

        [SerializeField, Range(0.01f, 10f)]
        [Tooltip("Multiplies the cooking speed.")]
        private float _cookingSpeedMod = 1f;

        [SerializeField, SpaceArea, EndGroup]
        [Tooltip("The property that tells the campfire how cooked an item is.")]
        private DataIdReference<ItemPropertyDefinition> _cookedAmountProperty;
        
        [SerializeField, BeginGroup("Audio")]
        private AudioDataSO _startCookingAudio;

        [SerializeField]
        private AudioDataSO _stopCookingAudio;
        
        [SerializeField, EndGroup]
        private AudioDataSO _fuelAddAudio;

        private IItemContainer[] _cookingContainers;
        private CookingSlot[] _cookingSlots;
        private bool _isCookingActive;
        private int _cookingStopMinute = -1;
        

        /// <summary>
        /// Gets a value indicating whether cooking is currently active.
        /// </summary>
        public bool IsCookingActive => _isCookingActive;

        /// <summary>
        /// Gets the cooking strength, represented as the ratio of remaining cooking time to maximum fuel time.
        /// </summary>
        public float CookingStrength => CookingTimeLeft / (float)_maxFuelTime;

        public int CookingTimeLeft
        {
            get
            {
                int totalMinutesPassed = World.Instance.Time.TotalMinutes;
                return Mathf.Max(_cookingStopMinute - totalMinutesPassed, 0);
            }
            private set
            {
                int totalMinutesPassed = World.Instance.Time.TotalMinutes;
                _cookingStopMinute = totalMinutesPassed + Mathf.Clamp(value, 0, _maxFuelTime);
            }
        }

        /// <summary>
        /// Event triggered when cooking starts.
        /// </summary>
        public event UnityAction CookingStarted;

        /// <summary>
        /// Event triggered when cooking progress is updated.
        /// </summary>
        public event UnityAction CookingUpdated;

        /// <summary>
        /// Event triggered when cooking is stopped.
        /// </summary>
        public event UnityAction CookingStopped;

        /// <summary>
        /// Event triggered when fuel is added to the cooking process.
        /// </summary>
        public event UnityAction<int> FuelAdded;

        /// <summary>
        /// Gets the containers associated with the cooking station.
        /// </summary>
        /// <returns>An array of item containers.</returns>
        public override IItemContainer[] GetContainers()
        {
            // Lazily initialize cooking containers if not already initialized
            _cookingContainers ??= new[]
            {
                GenerateContainer()
            };

            return _cookingContainers;
        }

        /// <summary>
        /// Queues the start of the cooking process after a delay.
        /// </summary>
        /// <returns>The duration of the delay before the cooking process starts.</returns>
        public float QueueStartCooking()
        {
            if (_isCookingActive)
                return 0f;
            
            // Play audio for queuing the cooking process
            PlayAudio(_startCookingAudio);

            // Invoke the StartCooking method after the specified delay
            CoroutineUtils.InvokeDelayed(this, StartCooking, _cookingStartDuration);

            return _cookingStartDuration;
        }

        /// <summary>
        /// Queues the stop of the cooking process after a delay.
        /// </summary>
        /// <returns>The duration of the delay before the cooking process stops.</returns>
        public float QueueStopCooking()
        {
            if (!_isCookingActive)
                return 0f;

            // Invoke the StopCooking method after the specified delay
            CoroutineUtils.InvokeDelayed(this, () => StopCooking(true), _cookingStopDuration);

            return _cookingStopDuration;
        }
        
        /// <summary>
        /// Stops all coroutine queues.
        /// </summary>
        public void CancelQueues() => StopAllCoroutines();

        /// <summary>
        /// Starts the cooking process.
        /// </summary>
        public void StartCooking()
        {
            // Check if cooking is already active
            if (_isCookingActive)
            {
                Debug.LogError("Cooking is already active.");
                return;
            }

            // Start cooking
            _isCookingActive = true;

            // Trigger event for cooking started
            CookingStarted?.Invoke();

            // Subscribe to minute change event for updating cooking
            World.Instance.Time.MinuteChanged += MinuteChanged;
        }

        /// <summary>
        /// Adds fuel to the cooking process.
        /// </summary>
        /// <param name="fuelDuration">The duration of fuel to add for cooking, in game minutes.</param>
        public void AddFuel(int fuelDuration)
        {
            // Check if cooking is active
            if (!_isCookingActive)
            {
                Debug.LogError("Cooking is not active.");
                return;
            }

            // Increment cooking time left by the adjusted fuel duration
            CookingTimeLeft += (int)(fuelDuration * _fuelDurationMod);

            // Trigger event for fuel added
            FuelAdded?.Invoke(fuelDuration);
            
            // Play audio for adding fuel
            PlayAudio(_fuelAddAudio);
        }

        /// <summary>
        /// Stops the cooking process.
        /// </summary>
        public void StopCooking(bool playAudio = true)
        {
            // Check if cooking is active
            if (!_isCookingActive)
            {
                Debug.LogError("Cooking is not active.");
                return;
            }

            // Reset end timer and mark cooking as inactive
            _cookingStopMinute = -1;
            _isCookingActive = false;

            // Unsubscribe from minute change event for cooking updates
            World.Instance.Time.MinuteChanged -= MinuteChanged;

            // Trigger event for cooking stopped
            CookingStopped?.Invoke();
            
            // Play audio for stopping the cooking process
            if (playAudio)
                PlayAudio(_stopCookingAudio);
        }

        private void MinuteChanged(int totalMinutes, int passedMinutes)
        {
            // If the time went backwards, return (this can happen in the editor)
            if (passedMinutes < 0)
                return;
            
            // Update cooking progress
            UpdateCooking(passedMinutes);

            // Trigger event for cooking progress updated
            CookingUpdated?.Invoke();

            // Check if cooking time has run out and stop cooking if so
            if (CookingTimeLeft <= 0)
                StopCooking();
        }
        
        private IItemContainer GenerateContainer()
        {
            // Create a new item container for the cooking station
            var container = new ItemContainer(null, nameof(CookingStation), _cookingSpots,
                new ItemDataRestriction(typeof(CookData)), new ItemPropertyRestriction(_cookedAmountProperty));
            
            // Subscribe to the container changed event
            container.ContainerChanged += OnCookingContainerChanged;

            // Generate cooking slots for the container
            _cookingSlots = GenerateCookingSlots();
            return container;
        }

        private void OnCookingContainerChanged()
        {
            // Get the cooking container
            var container = _cookingContainers[0];

            // Iterate through each cooking spot
            for (int i = 0; i < _cookingSpots; i++)
            {
                var cookingSlot = _cookingSlots[i];
                var item = container[i].Item;
                UpdateCookingSlot(cookingSlot, item);
            }
        }

        private void UpdateCookingSlot(CookingSlot cookingSlot, IItem item)
        {
            // Set the item of the cooking slot
            cookingSlot.Item = item;

            // If the item is null, reset the slot data and property
            if (item == null)
            {
                cookingSlot.Data = null;
                cookingSlot.Property = null;
            }
            else
            {
                // If the item is not null, set the slot data and property based on item properties
                var cookData = item.Definition.GetDataOfType<CookData>();
                cookingSlot.Data = cookData;
                cookingSlot.Property = item.GetPropertyWithId(_cookedAmountProperty);
            }
        }

        private CookingSlot[] GenerateCookingSlots()
        {
            // Create an array to store the cooking slots
            var slots = new CookingSlot[_cookingSpots];

            // Populate the array with new CookingSlot instances
            for (int i = 0; i < _cookingSpots; i++)
                slots[i] = new CookingSlot();

            return slots;
        }

        private void UpdateCooking(int passedMinutes)
        {
            // Get the cooking container
            var cookingContainer = GetContainers()[0];

            // Iterate through each cooking spot
            for (int spotIndex = 0; spotIndex < _cookingSpots; spotIndex++)
            {
                // Skip if cooking is not allowed in the slot
                if (!_cookingSlots[spotIndex].CanCook)
                    continue;

                var cookingSlot = _cookingSlots[spotIndex];
                var cookingProperty = cookingSlot.Property;
                int stackCount = cookingSlot.Item.StackCount;
                var cookData = cookingSlot.Data;

                // Calculate cooking progress based on passed time
                // float cookingProgress = (1f / stackCount * ) * _cookingSpeedMod * passedMinutes / cookData.CookTimeInMinutes;
                float cookingProgress = 1f / cookData.CookTimeInMinutes / stackCount * passedMinutes * _cookingSpeedMod;
                cookingProperty.Float += cookingProgress;

                // Check if cooking is completed
                if (cookingProperty.Float >= 1f)
                {
                    // Replace the item with the cooked output, if available
                    cookingContainer[spotIndex].Item = cookData.CookedOutput.Def == null ? null : new Item(cookData.CookedOutput.Def, stackCount);
                }
            }
        }

        private void OnDestroy()
        {
            if (_isCookingActive && UnityUtils.IsPlayMode)
                StopCooking(false);
        }

        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_cookedAmountProperty.IsNull)
            {
                UnityUtils.SafeOnValidate(this, () =>
                {
                    _cookedAmountProperty =
                        new DataIdReference<ItemPropertyDefinition>(
                            ItemPropertyDefinition.GetWithName("Cooked Amount"));
                });
            }
        }
#endif
        #endregion
        
        #region Internal
        private sealed class CookingSlot
        {
            public IItem Item;
            public CookData Data;
            public ItemProperty Property;

            public bool CanCook => Property != null;
        }
        #endregion

        #region Save & Load
        private sealed class SaveData
        {
            public ItemContainer Container;
            public int CookingTimeLeft;
        }

        void ISaveableComponent.LoadMembers(object data)
        {
            var saveData = (SaveData)data;
            var container = saveData.Container;

            if (container == null)
                return;
            
            _cookingContainers = new IItemContainer[]
            {
                container
            };
            
            container.Initialize(null);
            
            saveData.Container.ContainerChanged += OnCookingContainerChanged;
            _cookingSlots = GenerateCookingSlots();

            if (saveData.CookingTimeLeft <= 0)
                return;

            CookingTimeLeft = saveData.CookingTimeLeft;
            StartCooking();
        }

        object ISaveableComponent.SaveMembers()
        {
            return new SaveData
            {
                Container = _cookingContainers?[0] as ItemContainer,
                CookingTimeLeft = CookingTimeLeft
            };
        }

        #endregion
    }
}
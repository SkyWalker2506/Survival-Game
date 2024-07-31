using System.Collections.Generic;
using PolymindGames.InputSystem;
using UnityEngine.Events;
using UnityEngine;
using System;

namespace PolymindGames.WieldableSystem
{
    /// <summary>
    /// Controller responsible for carrying and managing carriable items.
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_1)]
    [RequireCharacterComponent(typeof(IWieldableControllerCC), typeof(IConstructableBuilderCC))]
    public sealed class CarriableController : CharacterBehaviour, ICarriableControllerCC, ISaveableComponent
    {
        [SerializeField, BeginGroup("Settings")]
        private InputContext _carryContext;
        
        [SerializeField, PrefabObjectOnly]
        private WieldableCarriable _wieldablePrefab;

        [SerializeField, NotNull, EndGroup]
        private Transform _dropPoint;

        [SerializeField, BeginGroup("Audio")]
        private AudioDataSO _carryAudio;

        [SerializeField, EndGroup]
        private AudioDataSO _dropAudio;

        private const float HOLSTER_WIELDABLE_SPEED = 1.25f;
        
        private readonly Stack<CarriablePickup> _pickups = new();
        private IWieldableControllerCC _controller;
        private WieldableCarriable _wieldableCarriable;


        /// <summary>
        /// Gets a value indicating whether the character is currently carrying an object.
        /// </summary>
        public bool IsCarrying => _wieldableCarriable.isActiveAndEnabled;

        /// <summary>
        /// Gets a value indicating whether the character is currently carrying an object.
        /// </summary>
        private bool HasCarriables => _pickups.Count > 0;

        /// <summary>
        /// Gets the number of objects currently being carried.
        /// </summary>
        public int CarryCount => _pickups.Count;

        /// <summary>
        /// Gets the definition of the currently carried object.
        /// </summary>
        private CarriableDefinition ActiveDefinition => _pickups.TryPeek(out var pickup) ? pickup.Definition : null;

        /// <summary>
        /// Event raised when carrying an object starts.
        /// </summary>
        public event UnityAction<CarriablePickup> ObjectCarryStarted;

        /// <summary>
        /// Event raised when carrying an object stops.
        /// </summary>
        public event UnityAction ObjectCarryStopped;

        /// <summary>
        /// Tries to carry the given carriable object.
        /// </summary>
        /// <param name="pickup">The carriable object to carry.</param>
        /// <returns>True if the object can be carried; otherwise, false.</returns>
        public bool TryCarryObject(CarriablePickup pickup)
        {
            if (!HasCarriables && TryStartObjectCarry(pickup))
            {
                _pickups.Push(pickup);
                Character.AudioPlayer.PlaySafe(_carryAudio);
                pickup.OnPickUp(Character);
                return true;
            }

            if (HasCarriables && TryAddCarriable(pickup))
            {
                _pickups.Push(pickup);
                Character.AudioPlayer.PlaySafe(_carryAudio);
                pickup.OnPickUp(Character);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempts to use the carried object.
        /// </summary>
        public void UseCarriedObject()
        {
            // If not carrying an object or in the process of holstering, do nothing.
            if (!HasCarriables || _controller.State == WieldableControllerState.Holstering)
                return;

            // Peek at the topmost object on the stack without removing it.
            var pickup = _pickups.Peek();
            
            // Try using the topmost object.
            if (pickup.TryUse(Character))
            {
                _wieldableCarriable.RemoveCarriable(pickup);
                
                // If the object was successfully used, remove it from the stack.
                _pickups.Pop();

                // If no longer carrying any objects, stop carrying.
                if (!HasCarriables)
                    StopObjectCarry();
            }
        }

        /// <summary>
        /// Drops a specified number of carried objects.
        /// </summary>
        /// <param name="amount">Number of objects to drop.</param>
        public void DropCarriedObjects(int amount)
        {
            // If not carrying any objects or in the process of holstering, do nothing.
            if (!HasCarriables || _controller.State == WieldableControllerState.Holstering)
                return;

            // Attempt to drop the specified number of carried objects.
            TryDropCarriable(amount);
            
            // If no longer carrying any objects, stop carrying.
            if (!HasCarriables)
                StopObjectCarry();
        }

        /// <summary>
        /// Attempts to start carrying the given object.
        /// </summary>
        /// <param name="pickup">The carriable object to carry.</param>
        /// <returns>True if the object can be carried; otherwise, false.</returns>
        private bool TryStartObjectCarry(CarriablePickup pickup)
        {
            if (pickup.Definition.MaxCarryCount == 0)
                return false;
            
            // Try to start carrying the object.
            if (_controller.TryEquipWieldable(_wieldableCarriable.Wieldable, HOLSTER_WIELDABLE_SPEED))
            {
                _wieldableCarriable.AddCarriable(pickup);
                
                // Object carry has started.
                _controller.HolsteringStarted += ForceDropAllCarriables;
                InputManager.Instance.PushContext(_carryContext);
                ObjectCarryStarted?.Invoke(pickup);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to add a carriable object to the currently carried object.
        /// </summary>
        /// <param name="pickup">The carriable object to add.</param>
        /// <returns>True if the object can be added; otherwise, false.</returns>
        private bool TryAddCarriable(CarriablePickup pickup)
        {
            if (_pickups.Count == ActiveDefinition.MaxCarryCount || ActiveDefinition != pickup.Definition)
                return false;
            
            // Add to the already carried object.
            _wieldableCarriable.AddCarriable(pickup);
            return true;
        }

        /// <summary>
        /// Stops carrying objects.
        /// </summary>
        private void StopObjectCarry()
        {
            // Unsubscribe from events, holster the wieldable, and notify listeners that object carrying has stopped.
            _controller.HolsteringStarted -= ForceDropAllCarriables;
            _controller.TryHolsterWieldable(_wieldableCarriable.Wieldable);
            InputManager.Instance.PopContext(_carryContext);
            ObjectCarryStopped?.Invoke();
        }
        
        /// <summary>
        /// Attempts to drop a specified number of carried objects.
        /// </summary>
        /// <param name="amount">Number of objects to drop.</param>
        /// <returns>True if objects were dropped, false otherwise.</returns>
        private bool TryDropCarriable(int amount)
        {
            // If the specified amount is invalid or not carrying any objects, return false.
            if (amount <= 0 || !HasCarriables)
                return false;

            // Play the drop audio.
            Character.AudioPlayer.PlaySafe(_dropAudio);
            
            // Drop the specified number of carried objects.
            int i = 0;
            do
            {
                var pickup = _pickups.Pop();
                
                _wieldableCarriable.RemoveCarriable(pickup);
                pickup.OnDrop(Character);

                var definition = pickup.Definition;
                Vector3 dropForce = _dropPoint.TransformVector(definition.DropForce);
                Character.ThrowObject(pickup.Rigidbody, dropForce, definition.DropTorque);

                i++;
            } while (i < amount && HasCarriables);

            return true;
        }

        private void ForceDropAllCarriables(IWieldable wieldable)
        {
            // If carrying objects, attempt to drop them all and stop carrying.
            if (_wieldableCarriable.Wieldable == wieldable && TryDropCarriable(CarryCount))
                StopObjectCarry();
        }

        protected override void OnBehaviourStart(ICharacter character)
        {
            _controller = character.GetCC<IWieldableControllerCC>();
            _wieldableCarriable = _controller.RegisterWieldable(_wieldablePrefab.Wieldable).gameObject.GetComponent<WieldableCarriable>();
            
            character.HealthManager.Death += OnDeath;

#if !DEBUG
            _wieldablePrefab = null;
#endif
        }

        protected override void OnBehaviourDestroy(ICharacter character)
        {
            character.HealthManager.Death -= OnDeath;
            
            if (HasCarriables && UnityUtils.IsPlayMode)
                InputManager.Instance.PopContext(_carryContext);
        }
        
        private void OnDeath(in DamageArgs args) => _controller.TryHolsterWieldable(_wieldableCarriable.Wieldable);

        #region Save & Load
        [Serializable]
        private sealed class SaveData
        {
            public DataIdReference<CarriableDefinition> CarryDef;
            public int CarryCount;
        }

        void ISaveableComponent.LoadMembers(object data)
        {
            var saveData = (SaveData)data;
            if (saveData.CarryDef.IsNull)
                return;

            CoroutineUtils.InvokeDelayed(this, CarryObjects, 0.1f);

            void CarryObjects()
            {
                var pickupPrefab = saveData.CarryDef.Def.Pickup;
                for (int i = 0; i < saveData.CarryCount; i++)
                {
                    var pickupInstance = Instantiate(pickupPrefab);
                    TryCarryObject(pickupInstance);
                }
            }
        }

        object ISaveableComponent.SaveMembers() => new SaveData
        {
            CarryDef = ActiveDefinition,
            CarryCount = HasCarriables ? CarryCount : 0
        };
        #endregion
    }
}
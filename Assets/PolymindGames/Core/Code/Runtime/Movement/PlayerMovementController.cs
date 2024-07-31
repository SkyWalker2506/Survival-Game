using PolymindGames.InputSystem.Behaviours;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System;

namespace PolymindGames.MovementSystem
{
    using Object = UnityEngine.Object;
    
    [RequireCharacterComponent(typeof(IMotorCC))]
    public sealed class PlayerMovementController : CharacterBehaviour, IMovementControllerCC, ISaveableComponent
    {
        [SerializeField, NotNull, BeginGroup, EndGroup]
        [Tooltip("Input handler for FPS movement.")]
        private FPSMovementInput _inputHandler;

        [SerializeField, Range(0f, 10f), BeginGroup("Settings")]
        [Tooltip("Multiplier for movement speed.")]
        private float _speedMultiplier = 1f;

        [SerializeField, Range(1f, 100f)]
        private float _baseAcceleration = 8f;
    
        [SerializeField, Range(1f, 100f), EndGroup]
        private float _baseDeceleration = 10f;

        [BeginGroup("Step Cycle"), SerializeField, Range(0.1f, 10f)]
        [Tooltip("Speed of transition between different step lengths.")]
        private float _stepLerpSpeed = 1.5f;

        [SerializeField, Range(0f, 10f)]
        [Tooltip("Step length when turning.")]
        private float _turnStepLength = 0.8f;

        [SerializeField, Range(0f, 10f), EndGroup]
        [Tooltip("Maximum step velocity when turning.")]
        private float _maxTurnStepVelocity = 1.75f;

        [SerializeField, BeginGroup("States"), EndGroup]
        [Tooltip("List of default movement states.")]
        [NestedBehaviourListInLine(HasHeader = false)]
        private CharacterMovementState[] _defaultStates;
        
        private readonly UnityAction<MovementStateType>[] _stateEnterEvents = new UnityAction<MovementStateType>[MovementStateTypeUtils.MovementStateTypesCount];
        private readonly UnityAction<MovementStateType>[] _stateExitEvents = new UnityAction<MovementStateType>[MovementStateTypeUtils.MovementStateTypesCount];
        private readonly List<Object>[] _stateLockers = new List<Object>[MovementStateTypeUtils.MovementStateTypesCount];
        private readonly ICharacterMovementState[] _states = new ICharacterMovementState[MovementStateTypeUtils.MovementStateTypesCount];

        private ICharacterMovementState _activeState;
        private float _distMovedSinceLastCycleEnded;
        private float _currentStepLength = 1f;
        private IMotorCC _motor;
        
        
        public MovementStateType ActiveState { get; private set; }
        public MovementModifierGroup SpeedModifier { get; private set; }
        public MovementModifierGroup AccelerationModifier { get; private set; }
        public MovementModifierGroup DecelerationModifier { get; private set; }
        public float StepCycle { get; private set; }

        public event UnityAction StepCycleEnded;

        
        #region Initialization
        private void Awake()
        {
            SpeedModifier = new MovementModifierGroup(_speedMultiplier, SpeedModifier);
            AccelerationModifier = new MovementModifierGroup(_baseAcceleration, AccelerationModifier);
            DecelerationModifier = new MovementModifierGroup(_baseDeceleration, DecelerationModifier);
        }

        protected override void OnBehaviourStart(ICharacter character)
        {
            _motor = character.GetCC<IMotorCC>();

            foreach (var state in _defaultStates)
            {
                int stateIndex = state.StateType.GetValue();
                ((ICharacterMovementState)state).InitializeState(this, _inputHandler, _motor, character);
                _states[stateIndex] = state;
            }

            ActiveState = MovementStateType.Idle;
            EnterState(_states[ActiveState.GetValue()]);
        }

        protected override void OnBehaviourEnable(ICharacter character) => _motor.SetMotionInput(GetMotionInput);
        protected override void OnBehaviourDisable(ICharacter character) => _motor.SetMotionInput(null);
        #endregion
        
        #region State Accessing
        public T GetStateOfType<T>() where T : ICharacterMovementState
        {
            foreach (var state in _states)
            {
                if (state is T matchedState)
                    return matchedState;
            }

            return default(T);
        }
        #endregion

        #region Update Loop
        private Vector3 GetMotionInput(Vector3 velocity, out bool useGravity, out bool snapToGround)
        {
            float deltaTime = Time.deltaTime;
            var activeState = _activeState;

            useGravity = activeState.ApplyGravity;
            snapToGround = activeState.SnapToGround;
            var newVelocity = activeState.UpdateVelocity(velocity, deltaTime);

            // Update the step cycle, mainly used for footsteps
            UpdateStepCycle(deltaTime);

            activeState.UpdateLogic();

            return newVelocity;
        }
        #endregion

        #region Step Cycle
        private void UpdateStepCycle(float deltaTime)
        {
            if (!_motor.IsGrounded)
                return;

            // Advance the step cycle based on the current velocity.
            _distMovedSinceLastCycleEnded += _motor.Velocity.GetHorizontal().magnitude * deltaTime;
            float targetStepLength = Mathf.Max(_activeState.StepCycleLength, 1f);
            _currentStepLength = Mathf.MoveTowards(_currentStepLength, targetStepLength, deltaTime * _stepLerpSpeed);

            // Advance the step cycle based on the character turn.
            _distMovedSinceLastCycleEnded += Mathf.Clamp(_motor.TurnSpeed, 0f, _maxTurnStepVelocity) * deltaTime * _turnStepLength;

            // If the step cycle is complete, reset it, and send a notification.
            if (_distMovedSinceLastCycleEnded > _currentStepLength)
            {
                _distMovedSinceLastCycleEnded -= _currentStepLength;
                StepCycleEnded?.Invoke();
            }

            StepCycle = _distMovedSinceLastCycleEnded / _currentStepLength;
        }
        #endregion
        
        #region State Events
        public void AddStateTransitionListener(MovementStateType stateType, UnityAction<MovementStateType> callback, MovementStateTransitionType transition)
        {
            int index = stateType.GetValue();
            switch (transition)
            {
                case MovementStateTransitionType.Enter:
                    _stateEnterEvents[index] += callback;
                    break;
                case MovementStateTransitionType.Exit:
                    _stateExitEvents[index] += callback;
                    break;
                case MovementStateTransitionType.Both:
                    _stateEnterEvents[index] += callback;
                    _stateExitEvents[index] += callback;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transition), transition, null);
            }
        }

        public void RemoveStateTransitionListener(MovementStateType stateType, UnityAction<MovementStateType> callback, MovementStateTransitionType transition)
        {
            int index = stateType.GetValue();
            switch (transition)
            {
                case MovementStateTransitionType.Enter:
                    _stateEnterEvents[index] -= callback;
                    break;
                case MovementStateTransitionType.Exit:
                    _stateExitEvents[index] -= callback;
                    break;
                case MovementStateTransitionType.Both:
                    _stateEnterEvents[index] -= callback;
                    _stateExitEvents[index] -= callback;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(transition), transition, null);
            }
        }
        #endregion

        #region State Changing
        public bool TrySetState(ICharacterMovementState newState)
        {
            if (newState != null && _activeState != newState && newState.Enabled && newState.IsValid())
            {
                EnterState(newState);
                return true;
            }

            return false;
        }

        public bool TrySetState(MovementStateType stateType) => TrySetState(_states[stateType.GetValue()]);

        private void EnterState(ICharacterMovementState newState)
        {
            var stateType = newState.StateType;

            // Handles state previous state exit.
            if (_activeState != null)
            {
                _activeState.OnExit();
                _stateExitEvents[0]?.Invoke(ActiveState);
                _stateExitEvents[ActiveState.GetValue()]?.Invoke(ActiveState);
            }

            // Handles next state enter.
            _activeState = newState;
            newState.OnEnter(ActiveState);
            ActiveState = stateType;

            _stateEnterEvents[0]?.Invoke(stateType);
            _stateEnterEvents[stateType.GetValue()]?.Invoke(stateType);
        }
        #endregion

        #region State Blocking
        public void AddStateBlocker(Object blocker, MovementStateType stateType)
        {
            int index = stateType.GetValue();

            // Creates a new locker list for the given state type
            if (_stateLockers[index] == null)
            {
                var list = new List<Object>
                {
                    blocker
                };
                _stateLockers[index] = list;

                _states[index].Enabled = false;
            }

            // Gets existing locker list for the given state type if available
            else
            {
                _stateLockers[index].Add(blocker);
                _states[index].Enabled = false;
            }
        }

        public void RemoveStateBlocker(Object blocker, MovementStateType stateType)
        {
            int index = stateType.GetValue();

            // Gets existing locker list for the given state type if available
            var list = _stateLockers[index];
            if (list != null && list.Remove(blocker))
            {
                if (list.Count == 0)
                    _states[index].Enabled = true;
            }
        }
        #endregion

        #region Save & Load
        void ISaveableComponent.LoadMembers(object data) => ActiveState = (MovementStateType)data;
        object ISaveableComponent.SaveMembers() => ActiveState;
        #endregion
        
        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_defaultStates != null)
            {
                foreach (var state in _defaultStates)
                    state.hideFlags = HideFlags.HideInInspector;
            }

            if (Character != null)
                OnBehaviourStart(Character);
        }
#endif
        #endregion

    }
}
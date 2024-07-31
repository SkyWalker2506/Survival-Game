using PolymindGames.MovementSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Movement Input")]
    public class FPSMovementInput : PlayerInputBehaviour, IMovementInputProvider
    {
        public Vector2 RawMovementInput => _movementInputValue;
        public Vector3 MovementInput => Vector3.ClampMagnitude(_rootTransform.TransformVector(new Vector3(_movementInputValue.x, 0f, _movementInputValue.y)), 1f);

        public bool RunInput
        {
            get => _runHold || _runToggle;
            private set
            {
                _runHold = value;
                _runToggle = value;
            }
        }
        
        public bool CrouchInput
        {
            get => _crouchHold || _crouchToggle;
            private set
            {   
                _crouchHold = value;
                _crouchToggle = value;
            }
        }

        public bool JumpInput { get; private set; }

        [SerializeField, BeginGroup("Actions")]
        private InputActionReference _moveInput;

        [SerializeField, SpaceArea]
        private InputActionReference _runHoldInput;

        [SerializeField]
        private InputActionReference _runToggleInput;

        [SerializeField, SpaceArea]
        private InputActionReference _crouchHoldInput;

        [SerializeField]
        private InputActionReference _crouchToggleInput;

        [SerializeField, Range(0f, 1f), SpaceArea]
        private float _jumpReleaseDelay = 0.05f;

        [SerializeField, EndGroup]
        private InputActionReference _jumpInput;

        private Transform _rootTransform;
        private Vector2 _movementInputValue;
        private float _releaseJumpBtnTime;
        private bool _crouchToggle;
        private bool _crouchHold;
        private bool _runToggle;
        private bool _runHold;


        #region Initialization
        protected override void Awake()
        {
            base.Awake();
            _rootTransform = transform.root;
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            ResetAllInputs();

            _moveInput.Enable();
            _crouchToggleInput.Enable();
            _crouchHoldInput.Enable();
            _runToggleInput.Enable();
            _runHoldInput.Enable();
            _jumpInput.Enable();
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            ResetAllInputs();

            _moveInput.TryDisable();
            _crouchToggleInput.TryDisable();
            _crouchHoldInput.TryDisable();
            _runToggleInput.TryDisable();
            _runHoldInput.TryDisable();
            _jumpInput.TryDisable();
        }
        #endregion

        #region Input Handling
        public void ResetAllInputs() 
        {
            RunInput = false;
            JumpInput = false;
            CrouchInput = false;
            _releaseJumpBtnTime = 0f;
            _movementInputValue = Vector2.zero;
        }

        public void UseCrouchInput() => CrouchInput = false;
        public void UseRunInput() => RunInput = false;
        public void UseJumpInput() => JumpInput = false;
        
        private void Update()
        {
            // Handle movement input.
            _movementInputValue = _moveInput.action.ReadValue<Vector2>();

            // Handle run input.
            _runHold = _runHoldInput.action.ReadValue<float>() > 0.1f;
                
            if (_runToggleInput.action.triggered)
                _runToggle = !_runToggle;

            // Handle crouch input.
            _crouchHold = _crouchHoldInput.action.ReadValue<float>() > 0.1f;
            
            if (_crouchToggleInput.action.triggered)
                _crouchToggle = !_crouchToggle;

            // Handle jump input.
            if (Time.time > _releaseJumpBtnTime || !JumpInput)
            {
                bool jumpInput = _jumpInput.action.triggered;
                JumpInput = jumpInput;

                if (jumpInput)
                    _releaseJumpBtnTime = Time.time + _jumpReleaseDelay;
            }
        }
        #endregion
    }
}
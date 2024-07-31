using PolymindGames.PostProcessing;
using PolymindGames.WieldableSystem;
using UnityEngine.InputSystem;
using UnityEngine;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Wieldables Input")]
    [RequireCharacterComponent(typeof(IWieldableControllerCC), typeof(IWieldableInventory))]
    [OptionalCharacterComponent(typeof(IWieldableHealingHandlerCC), typeof(IWieldableThrowableHandlerCC))]
    public class FPSWieldablesInput : PlayerInputBehaviour
    {
        [SerializeField, BeginGroup("Actions")]
        private InputActionReference _useInput;

        [SerializeField]
        private InputActionReference _reloadInput;

        [SerializeField]
        private InputActionReference _dropInput;

        [SerializeField]
        private InputActionReference _aimHoldInput;

        [SerializeField]
        private InputActionReference _aimToggleInput;

        [SerializeField]
        private InputActionReference _selectInput;

        [SerializeField]
        private InputActionReference _holsterInput;

        [SerializeField]
        private InputActionReference _healInput;

        [SerializeField]
        private InputActionReference _throwInput;

        [SerializeField]
        private InputActionReference _firemodeInput;

        [SerializeField, EndGroup]
        private InputActionReference _throwableScrollInput;

        private IWieldableThrowableHandlerCC _throwableHandler;
        private IWieldableHealingHandlerCC _healingHandler;
        private IWieldableInventory _selection;
        private IWieldableControllerCC _controller;
        private IWieldable _activeWieldable;
        private IReloadInputHandler _reloadInputHandler;
        private IUseInputHandler _useInputHandler;
        private IAimInputHandler _aimInputHandler;

        private bool? _aimInputFlag;
        

        #region Initialization
        protected override void OnBehaviourStart(ICharacter character)
        {
            _controller = character.GetCC<IWieldableControllerCC>();
            _selection = character.GetCC<IWieldableInventory>();
            _healingHandler = character.GetCC<IWieldableHealingHandlerCC>();
            _throwableHandler = character.GetCC<IWieldableThrowableHandlerCC>();
            
            _controller.EquippingStopped += OnEquip;
            _controller.HolsteringStarted += OnHolster;
        }

        protected override void OnBehaviourDestroy(ICharacter character)
        {
            if (_controller != null)
            {
                _controller.EquippingStopped -= OnEquip;
                _controller.HolsteringStarted -= OnHolster;
            }
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            _holsterInput.RegisterStarted(OnHolsterAction);
            _selectInput.RegisterStarted(OnSelectAction);
            _dropInput.RegisterStarted(OnDropAction);

            if (_healingHandler != null)
                _healInput.RegisterStarted(OnHealAction);

            if (_throwableHandler != null)
            {
                _throwInput.RegisterStarted(OnThrowAction);
                _throwableScrollInput.RegisterPerformed(OnThrowableScrollAction);
            }

            _useInput.Enable();
            _aimHoldInput.RegisterStarted(OnAimAction);
            _aimHoldInput.RegisterCanceled(OnAimAction);
            _aimToggleInput.RegisterPerformed(OnAimToggleAction);

            _firemodeInput.RegisterStarted(OnFiremodeAction);
            _reloadInput.RegisterStarted(OnReloadAction);
            
            if (_controller.State == WieldableControllerState.None)
                OnEquip(_controller.ActiveWieldable);
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            _holsterInput.UnregisterStarted(OnHolsterAction);
            _selectInput.UnregisterStarted(OnSelectAction);
            _dropInput.UnregisterStarted(OnDropAction);

            if (_healingHandler != null)
                _healInput.UnregisterStarted(OnHealAction);

            if (_throwableHandler != null)
            {
                _throwInput.UnregisterStarted(OnThrowAction);
                _throwableScrollInput.UnregisterPerformed(OnThrowableScrollAction);
            }

            _firemodeInput.UnregisterStarted(OnFiremodeAction);
            _reloadInput.UnregisterStarted(OnReloadAction);
            
            _useInput.TryDisable();
            _aimHoldInput.UnregisterStarted(OnAimAction);
            _aimHoldInput.UnregisterCanceled(OnAimAction);
            _aimToggleInput.UnregisterPerformed(OnAimToggleAction);

            OnHolster(_activeWieldable);
        }

        private void OnHolster(IWieldable wieldable)
        {
            _useInputHandler?.Use(WieldableInputPhase.End);
            _useInputHandler = null;
            
            _aimInputHandler?.Aim(WieldableInputPhase.End);
            _aimInputHandler = null;
            _aimInputFlag = null;
            
            _reloadInputHandler?.Reload(WieldableInputPhase.End);
            _reloadInputHandler = null;
        }

        private void OnEquip(IWieldable wieldable)
        {
            _activeWieldable = wieldable;
            _useInputHandler = wieldable as IUseInputHandler;
            _aimInputHandler = wieldable as IAimInputHandler;
            _reloadInputHandler = wieldable as IReloadInputHandler;
        }
        #endregion

        #region Input Handling
        private void Update()
        {
            if (_useInputHandler != null)
            {
                if (_useInput.action.triggered)
                    _useInputHandler.Use(WieldableInputPhase.Start);
                else if (_useInput.action.ReadValue<float>() > 0.001f)
                    _useInputHandler.Use(WieldableInputPhase.Hold);
                else if (_useInput.action.WasReleasedThisFrame() || !_useInput.action.enabled)
                    _useInputHandler.Use(WieldableInputPhase.End);
            }

            if (_aimInputFlag.HasValue && _aimInputHandler != null)
            {
                bool aimStarted = _aimInputFlag.Value && _aimInputHandler.Aim(WieldableInputPhase.Start);
                bool aimEnded = !_aimInputFlag.Value && _aimInputHandler.Aim(WieldableInputPhase.End);
                if (aimStarted || aimEnded)
                    _aimInputFlag = null;
            }
        }

        private void OnSelectAction(InputAction.CallbackContext context)
        {
            int index = (int)context.ReadValue<float>() - 1;
            _selection.SelectAtIndex(index);
        }

        private void OnFiremodeAction(InputAction.CallbackContext context)
        {
            if (_activeWieldable is IFirearm && _activeWieldable.gameObject.TryGetComponent<IFirearmIndexModeHandler>(out var modeHandler))
                modeHandler.ToggleNextMode();
        }

        private void OnAimAction(InputAction.CallbackContext context) => _aimInputFlag = context.phase == InputActionPhase.Started;
        private void OnAimToggleAction(InputAction.CallbackContext obj)
        {
            if (_aimInputHandler != null)
                _aimInputFlag = !_aimInputHandler.IsAiming;
        }

        private void OnReloadAction(InputAction.CallbackContext context) => _reloadInputHandler?.Reload(WieldableInputPhase.Start);
        private void OnDropAction(InputAction.CallbackContext context) => _selection.DropWieldable();
        private void OnHealAction(InputAction.CallbackContext context) => _healingHandler?.TryHeal();
        private void OnThrowAction(InputAction.CallbackContext context) => _throwableHandler?.TryThrow();
        private void OnThrowableScrollAction(InputAction.CallbackContext context) => _throwableHandler.SelectNext(context.ReadValue<float>() > 0);
        private void OnHolsterAction(InputAction.CallbackContext context) => _selection.SelectAtIndex(_selection.SelectedIndex != -1 ? -1 : _selection.PreviousIndex);
        #endregion
    }
}
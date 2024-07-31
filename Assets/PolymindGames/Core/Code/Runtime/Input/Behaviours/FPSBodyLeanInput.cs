using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Lean Input")]
    [RequireCharacterComponent(typeof(IBodyLeanHandlerCC))]
    public sealed class FPSBodyLeanInput : PlayerInputBehaviour
    {
        [SerializeField, BeginGroup("Actions"), EndGroup]
        private InputActionReference _leanAction;

        private IBodyLeanHandlerCC _leanHandler;


        #region Initialization
        protected override void OnBehaviourStart(ICharacter character)
        {
            _leanHandler = character.GetCC<IBodyLeanHandlerCC>();
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            _leanHandler.SetLeanState(BodyLeanState.Center);
            _leanAction.RegisterStarted(OnBodyLean);
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            _leanHandler.SetLeanState(BodyLeanState.Center);
            _leanAction.UnregisterStarted(OnBodyLean);
        }
        #endregion

        #region Input Handling
        private void OnBodyLean(InputAction.CallbackContext context)
        {
            var leanState = _leanHandler.LeanState;
            var targetLeanState = (BodyLeanState)Mathf.CeilToInt(context.ReadValue<float>());

            if (leanState == targetLeanState)
                _leanHandler.SetLeanState(BodyLeanState.Center);
            else
                _leanHandler.SetLeanState(leanState != BodyLeanState.Center ? BodyLeanState.Center : targetLeanState);
        }
        #endregion
    }
}

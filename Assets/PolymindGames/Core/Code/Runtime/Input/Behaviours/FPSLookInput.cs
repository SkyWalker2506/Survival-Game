using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Look Input")]
    [RequireCharacterComponent(typeof(ILookHandlerCC))]
    public class FPSLookInput : PlayerInputBehaviour
    {
        [SerializeField, BeginGroup("Actions"), EndGroup]
        private InputActionReference _lookInput;

        private ILookHandlerCC _lookHandler;


        #region Initialization
        protected override void OnBehaviourStart(ICharacter character)
        {
            _lookHandler = character.GetCC<ILookHandlerCC>();
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            _lookInput.Enable();
            _lookHandler.SetLookInput(GetInput);
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            _lookInput.TryDisable();
            _lookHandler.SetLookInput(null);
        }
        #endregion

        #region Input Handling
        private Vector2 GetInput()
        {
            Vector2 lookInput = _lookInput.action.ReadValue<Vector2>();
            (lookInput.x, lookInput.y) = (lookInput.y, lookInput.x);
            return lookInput;
        }
        #endregion
    }
}
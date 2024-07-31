using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Object Carry Input")]
    [RequireCharacterComponent(typeof(ICarriableControllerCC))]
    public class FPSObjectCarryInput : PlayerInputBehaviour
    {
        [SerializeField, BeginGroup("Actions")]
        private InputActionReference _useInput;

        [SerializeField, EndGroup]
        private InputActionReference _dropInput;

        private ICarriableControllerCC _objectCarry;


        #region Initialization
        protected override void OnBehaviourStart(ICharacter character)
        {
            _objectCarry = character.GetCC<ICarriableControllerCC>();
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            _useInput.RegisterStarted(OnUseCarriedObject);
            _dropInput.RegisterStarted(OnDropAction);
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            _useInput.UnregisterStarted(OnUseCarriedObject);
            _dropInput.UnregisterStarted(OnDropAction);
        }
        #endregion

        #region Input Handling
        private void OnUseCarriedObject(InputAction.CallbackContext obj) => _objectCarry.UseCarriedObject();
        private void OnDropAction(InputAction.CallbackContext obj) => _objectCarry.DropCarriedObjects(1);
        #endregion
    }
}
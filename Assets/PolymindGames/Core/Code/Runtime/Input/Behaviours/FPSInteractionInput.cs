using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Interaction Input")]
    [RequireCharacterComponent(typeof(IInteractionHandlerCC))]
    public class FPSInteractionInput : PlayerInputBehaviour
    {
        [SerializeField, BeginGroup("Actions"), EndGroup]
        private InputActionReference _interactInput;

        private IInteractionHandlerCC _interaction;
        
        
        #region Initialization
        protected override void OnBehaviourStart(ICharacter character)
        {
            _interaction = character.GetCC<IInteractionHandlerCC>();
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            _interactInput.RegisterStarted(OnInteractStart);
            _interactInput.RegisterCanceled(OnInteractStop);
            CoroutineUtils.InvokeNextFrame(this, () => _interaction.InteractionEnabled = true);
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            _interactInput.UnregisterStarted(OnInteractStart);
            _interactInput.UnregisterCanceled(OnInteractStop);
            _interaction.InteractionEnabled = false;
        }
        #endregion

        #region Input Handling
        private void OnInteractStart(InputAction.CallbackContext obj) => _interaction.StartInteraction();
        private void OnInteractStop(InputAction.CallbackContext obj) => _interaction.StopInteraction();
        #endregion
    }
}

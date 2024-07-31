using UnityEngine.InputSystem;
using UnityEngine;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Building Previews Input")]
    [RequireCharacterComponent(typeof(IConstructableBuilderCC))]
    public class FPSBuildingPreviewsInput : PlayerInputBehaviour
    {
        [SerializeField, BeginGroup("Actions")]
        private InputActionReference _addMaterialInput;

        [SerializeField, EndGroup]
        private InputActionReference _cancelPreviewInput;
        
        private IConstructableBuilderCC _constructableBuilder;
        private ICarriableControllerCC _carriableController;


        #region Initialization
        protected override void OnBehaviourStart(ICharacter character)
        {
            _constructableBuilder = character.GetCC<IConstructableBuilderCC>();
            _carriableController = character.GetCC<ICarriableControllerCC>();
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            _cancelPreviewInput.RegisterStarted(OnCancelPreviewStart);
            _cancelPreviewInput.RegisterCanceled(OnCancelPreviewStop);
            _addMaterialInput.RegisterPerformed(OnAddMaterialAction);

            CoroutineUtils.InvokeNextFrame(this, () => _constructableBuilder.DetectionEnabled = true);
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            _cancelPreviewInput.UnregisterStarted(OnCancelPreviewStart);
            _cancelPreviewInput.UnregisterCanceled(OnCancelPreviewStop);
            _addMaterialInput.UnregisterPerformed(OnAddMaterialAction);

            if (UnityUtils.IsPlayMode)
                _constructableBuilder.DetectionEnabled = false;
        }
        #endregion

        #region Input Handling
        private void OnCancelPreviewStart(InputAction.CallbackContext obj) => _constructableBuilder.StartCancellingPreview();
        private void OnCancelPreviewStop(InputAction.CallbackContext obj) => _constructableBuilder.StopCancellingPreview();
        private void OnAddMaterialAction(InputAction.CallbackContext obj)
        {
            if (!_carriableController.IsCarrying && _carriableController.CarryCount == 0)
                _constructableBuilder.TryAddMaterialFromPlayer();
        }
        #endregion
    }
}

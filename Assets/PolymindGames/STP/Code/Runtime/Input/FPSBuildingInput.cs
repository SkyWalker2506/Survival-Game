using PolymindGames.BuildingSystem;
using UnityEngine.InputSystem;
using UnityEngine;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Building Input")]
    [RequireCharacterComponent(typeof(IBuildControllerCC))]
    public class FPSBuildingInput : PlayerInputBehaviour
    {
        [SerializeField, BeginGroup("Actions")]
        private InputActionReference _placePreviewInput;

        [SerializeField]
        private InputActionReference _buildingRotateInput;

        [SerializeField, EndGroup]
        private InputActionReference _buildingCycleInput;

        private IBuildControllerCC _buildController;


        #region Initialization
        protected override void OnBehaviourStart(ICharacter character)
        {
            _buildController = character.GetCC<IBuildControllerCC>();
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            _buildingCycleInput.RegisterStarted(OnCycleInput);
            _buildingRotateInput.RegisterPerformed(OnRotateInput);
            _placePreviewInput.RegisterStarted(OnPlaceInput);
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            _buildingCycleInput.UnregisterStarted(OnCycleInput);
            _buildingRotateInput.UnregisterPerformed(OnRotateInput);
            _placePreviewInput.UnregisterStarted(OnPlaceInput);
        }
        #endregion

        #region Input Handling
        private void OnCycleInput(InputAction.CallbackContext ctx)
        {
            if (_buildController.BuildingPiece == null || _buildController.BuildingPiece is FreeBuildingPiece)
                return;

            bool next = ctx.ReadValue<float>() > 0.1f;
            var newDef = BuildingPieceDefinition.GetNextGroupBuildingPiece(_buildController.BuildingPiece.Definition, next);
            var newPiece = Instantiate(newDef.Prefab).GetComponent<BuildingPiece>();
            _buildController.SetBuildingPiece(newPiece);
        }
        
        private void OnPlaceInput(InputAction.CallbackContext ctx) => _buildController.TryPlaceBuildingPiece();
        private void OnRotateInput(InputAction.CallbackContext ctx) => _buildController.RotationOffset += ctx.ReadValue<float>() / 120f;
        #endregion
    }
}
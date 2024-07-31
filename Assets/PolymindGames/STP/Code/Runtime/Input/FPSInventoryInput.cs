using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Inventory Input")]
    [RequireCharacterComponent(typeof(IInventoryInspectManagerCC))]
    public class FPSInventoryInput : PlayerInputBehaviour
    {
        [SerializeField, BeginGroup("Actions"), EndGroup]
        private InputActionReference _inventoryToggle;

        private IInventoryInspectManagerCC _inventoryInspection;


        #region Initialization
        protected override void OnBehaviourStart(ICharacter character) => _inventoryInspection = character.GetCC<IInventoryInspectManagerCC>();
        protected override void OnBehaviourEnable(ICharacter character) => _inventoryToggle.RegisterStarted(OnInventoryToggleInput);
        protected override void OnBehaviourDisable(ICharacter character) => _inventoryToggle.UnregisterStarted(OnInventoryToggleInput);
        #endregion

        #region Input Handling
        private void OnInventoryToggleInput(InputAction.CallbackContext context)
        {
            if (!_inventoryInspection.IsInspecting)
                _inventoryInspection.StartInspection(null);
            else
                _inventoryInspection.StopInspection();
        }
        #endregion
    }
}

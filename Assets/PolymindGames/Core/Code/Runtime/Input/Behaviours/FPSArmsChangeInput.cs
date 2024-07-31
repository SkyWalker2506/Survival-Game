using PolymindGames.WieldableSystem;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Change Arms Input")]
    public sealed class FPSArmsChangeInput : PlayerInputBehaviour
    {
        [SerializeField, BeginGroup("Actions"), EndGroup]
        private InputActionReference _changeArmsInput; 

        private WieldableArmsHandler _armsHandler;


        #region Initialization
        protected override void OnBehaviourStart(ICharacter character)
        {
            _armsHandler = GetComponent<WieldableArmsHandler>();
        }
        
        protected override void OnBehaviourEnable(ICharacter character) => _changeArmsInput.RegisterStarted(ToggleArmsAction);
        protected override void OnBehaviourDisable(ICharacter character) => _changeArmsInput.UnregisterStarted(ToggleArmsAction);
        #endregion

        #region Input Handling
        private void ToggleArmsAction(InputAction.CallbackContext obj) => _armsHandler.ToggleNextArmSet();
        #endregion
    }
}

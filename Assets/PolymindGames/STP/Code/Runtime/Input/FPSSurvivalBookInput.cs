using PolymindGames.WieldableSystem;
using UnityEngine.InputSystem;
using UnityEngine;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Survival Book Input")]
    [RequireCharacterComponent(typeof(IWieldableControllerCC))]
    public class FPSSurvivalBookInput : PlayerInputBehaviour
    {
        [SerializeField, PrefabObjectOnly, BeginGroup("Reference"), EndGroup]
        [Tooltip("The prefab object representing the survival book.")]
        private Wieldable _survivalBookPrefab;

        [SerializeField, BeginGroup("Actions"), EndGroup]
        [Tooltip("The input action reference for accessing the survival book.")]
        private InputActionReference _survivalBookInput;

        private IWieldableControllerCC _controller;
        private IWieldable _bookWieldable;


        #region Initialization
        protected override void OnBehaviourStart(ICharacter character)
        {
            _controller = character.GetCC<IWieldableControllerCC>();
            _bookWieldable = _controller.RegisterWieldable(_survivalBookPrefab);
            _survivalBookPrefab = null;
        }

        protected override void OnBehaviourEnable(ICharacter character) => _survivalBookInput.RegisterStarted(OnSurvivalBookAction);

        protected override void OnBehaviourDisable(ICharacter character)
        {
            _survivalBookInput.UnregisterStarted(OnSurvivalBookAction);
            _controller.TryHolsterWieldable(_bookWieldable);
        }
        #endregion

        #region Input Handling
        private void OnSurvivalBookAction(InputAction.CallbackContext obj)
        {
            var activeWieldable = _controller.ActiveWieldable;
            if (activeWieldable != _bookWieldable)
                _controller.TryEquipWieldable(_bookWieldable, 1.35f);
            else
                _controller.TryHolsterWieldable(_bookWieldable);
        }
        #endregion
    }
}

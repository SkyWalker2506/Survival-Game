using PolymindGames.UserInterface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Wheel Input")]
    [RequireComponent(typeof(IWheelUI))]
    public class WheelUIInput : PlayerUIInputBehaviour
    {
        [SerializeField, BeginGroup("Actions"), EndGroup]
        private InputActionReference _itemWheelInput;

        private IWheelUI _wheel;


        #region Initialization
        protected override void Awake()
        {
            base.Awake();
            _wheel = GetComponent<IWheelUI>();
        }

        private void OnEnable()
        {
            _itemWheelInput.RegisterPerformed(OnWheelOpen);
            _itemWheelInput.RegisterCanceled(OnWheelCloseInput);
        }

        private void OnDisable()
        {
            _itemWheelInput.UnregisterPerformed(OnWheelOpen);
            _itemWheelInput.UnregisterCanceled(OnWheelCloseInput);
        }
        #endregion

        #region Input handling
        private void Update() => _wheel.UpdateSelection(RaycastManagerUI.Instance.GetCursorDelta());
        private void OnWheelOpen(InputAction.CallbackContext ctx) => _wheel.StartInspection();
        private void OnWheelCloseInput(InputAction.CallbackContext ctx) => _wheel.EndInspectionAndSelectHighlighted();
        #endregion
    }
}
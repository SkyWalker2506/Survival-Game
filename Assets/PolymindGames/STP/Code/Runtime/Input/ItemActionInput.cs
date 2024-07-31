using PolymindGames.InventorySystem;
using PolymindGames.UserInterface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Item Action Input")]
    public class ItemActionInput : PlayerUIInputBehaviour
    {
        [SerializeField, BeginGroup("Settings")]
        private InputActionReference _inputAction;

        [SerializeField, EndGroup]
        private ItemAction _itemAction;


        private void OnEnable() => _inputAction.RegisterPerformed(OnAction);
        private void OnDisable() => _inputAction.UnregisterPerformed(OnAction);

        private void OnAction(InputAction.CallbackContext context)
        {
            if (ItemDragger.Instance.IsDragging)
                return;
            
            var raycastObject = RaycastManagerUI.Instance.RaycastAtCursorPosition();
            if (raycastObject != null && raycastObject.TryGetComponent<ItemSlotUI>(out var slotUI))
            {
                var slot = slotUI.ItemSlot;
                _itemAction.Perform(slot, Character);
            }
        }
    }
}

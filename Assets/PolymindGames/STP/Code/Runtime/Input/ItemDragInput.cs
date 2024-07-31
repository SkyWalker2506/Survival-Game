using PolymindGames.UserInterface;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/Item Drag Input")]
    public class ItemDragInput : PlayerUIInputBehaviour
    {
        [SerializeField, NotNull, BeginGroup("Actions")]
        private ItemDragger _dragHandler;

        [SerializeField, NotNull]
        private InputActionReference _leftClickInput;

        [SerializeField, NotNull, EndGroup]
        private InputActionReference _splitItemStackInput;

        private Vector2 _pointerPositionLastFrame;
        private bool _pointerMovedLastFrame;
        private ItemSlotUI _dragStartSlot;
        private bool _isDragging;


        #region Initialization
        private void OnEnable()
        {
            _splitItemStackInput.Enable();
        }

        private void OnDisable()
        {
            _splitItemStackInput.TryDisable();

            _dragHandler.CancelDrag(_dragStartSlot);
            
            _dragStartSlot = null;
            _isDragging = false;
        }

        #endregion

        #region Input Handling
        private void Update()
        {
            Vector2 pointerPosition = RaycastManagerUI.Instance.GetCursorPosition();
            bool pointerMovedThisFrame = (pointerPosition - _pointerPositionLastFrame).sqrMagnitude > 0.0001f;
            _pointerPositionLastFrame = pointerPosition;
            UpdateDragging(pointerPosition, pointerMovedThisFrame, _pointerMovedLastFrame);
            _pointerMovedLastFrame = pointerMovedThisFrame;
        }

        private void UpdateDragging(Vector2 pointerPosition, bool pointerMovedThisFrame, bool pointerMovedLastFrame)
        {
            if (!_isDragging)
            {
                if (_leftClickInput.action.ReadValue<float>() > 0.1f && pointerMovedThisFrame && pointerMovedLastFrame)
                {
                    _dragStartSlot = GetSlotRaycast(out _);
                    _isDragging = true;

                    if (_dragStartSlot != null)
                    {
                        bool splitItemStack = _splitItemStackInput.action.ReadValue<float>() > 0.01f;
                        _dragHandler.OnDragStart(_dragStartSlot, pointerPosition, splitItemStack);
                    }
                }
            }
            else
            {
                _dragHandler.OnDrag(pointerPosition);

                if (_leftClickInput.action.WasReleasedThisFrame())
                {
                    if (_dragStartSlot != null)
                    {
                        var raycastedSlot = GetSlotRaycast(out var raycastedObject);
                        _dragHandler.OnDragEnd(_dragStartSlot, raycastedSlot, raycastedObject);
                    }

                    _isDragging = false;
                }
            }
        }

        private static ItemSlotUI GetSlotRaycast(out GameObject raycastedObject)
        {
            raycastedObject = RaycastManagerUI.Instance.RaycastAtCursorPosition();
            return raycastedObject != null ? raycastedObject.GetComponent<ItemSlotUI>() : null;
        }
        #endregion

#if UNITY_EDITOR
        private void Reset()
        {
            _dragHandler = GetComponent<ItemDragger>();
            if (_dragHandler == null)
                _dragHandler = gameObject.AddComponent<BasicItemDragger>();
        }
#endif
    }
}

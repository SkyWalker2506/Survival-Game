using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [DefaultExecutionOrder(ExecutionOrderConstants.SCENE_SINGLETON)]
    public sealed class RaycastManagerUI : MonoBehaviour
    {
        [SerializeField, BeginGroup]
        private EventSystem _eventSystem;

        [SerializeField, EndGroup]
        private GraphicRaycaster _raycaster;

        [SerializeField, SpaceArea, BeginGroup]
        private InputActionReference _cursorDeltaInput;

        [SerializeField, EndGroup]
        private InputActionReference _cursorPositionInput;
        
        private readonly List<RaycastResult> _raycastResults = new();
        private PointerEventData _pointerEventData;


        public static RaycastManagerUI Instance { get; private set; }
        
        public Vector2 GetCursorPosition()
        {
            var cursorPosition = _cursorPositionInput.action.ReadValue<Vector2>();
            return cursorPosition;
        }

        public Vector2 GetCursorDelta()
        {
            var cursorPosition = _cursorDeltaInput.action.ReadValue<Vector2>().normalized;
            return cursorPosition;
        }

        public GameObject RaycastAtCursorPosition()
        {
            var cursorPosition = GetCursorPosition();
            return Raycast(cursorPosition);
        }

        public GameObject Raycast(Vector2 position)
        {
            // Set the Pointer Event Position to that of the game object
            _pointerEventData.position = position;

            _raycastResults.Clear();

            // Raycast using the Graphics Raycaster and mouse click position
            _raycaster.Raycast(_pointerEventData, _raycastResults);

            return _raycastResults.Count > 0 ? _raycastResults[0].gameObject : null;
        }
        
        private void Awake()
        {
            _cursorDeltaInput.action.Enable();
            _cursorPositionInput.action.Enable();
            _pointerEventData = new PointerEventData(_eventSystem);
            
            // Ensure only one instance of RaycastManager exists
            if (Instance == null)
                Instance = this;
        }
        
        private void OnDestroy()
        {
            // Clear singleton instance when destroyed
            if (Instance == this)
                Instance = null;
        }
    }
}
using UnityEngine.EventSystems;
using UnityEngine;

namespace PolymindGames.InputSystem.Behaviours
{
    [AddComponentMenu("Input/UI Input")]
    public sealed class UIInput : MonoBehaviour, IInputBehaviour
    {
        [field: SerializeField, BeginGroup, EndGroup]
        public InputEnableMode EnableMode { get; private set; } = InputEnableMode.BasedOnContext;

        [SerializeField, NotNull, BeginGroup("Actions"), EndGroup]
        private BaseInputModule _inputModule;

        
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (this != (Object)null)
                    enabled = value;
            }
        }
        
        private void Awake() => InputManager.Instance.RegisterBehaviour(this);
        private void OnDestroy() => InputManager.Instance.UnregisterBehaviour(this);

        private void OnEnable()
        {
            _inputModule.enabled = true;
            UnityUtils.UnlockCursor();
        }

        private void OnDisable()
        {
            if (!UnityUtils.IsPlayMode)
                return;

            _inputModule.enabled = false;
            UnityUtils.LockCursor();
        }
    }
}

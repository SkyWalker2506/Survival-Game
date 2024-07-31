using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine;
using System;

namespace PolymindGames.InputSystem
{
    [DefaultExecutionOrder(ExecutionOrderConstants.SCRIPTABLE_SINGLETON)]
    [CreateAssetMenu(menuName = MANAGERS_MENU_PATH + "Input Manager", fileName = nameof(InputManager))]
    public sealed class InputManager : Manager<InputManager>
    {
        #region Initialization
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init() => LoadOrCreateInstance();
        
        protected override void OnInitialized()
        {
#if UNITY_EDITOR
            _lastEscapeCallbackRemoveFrame = -1;
            ActiveContext = null;
            _behaviours.Clear();
            _escapeCallbacks.Clear();
            _contextStack.Clear();
#endif
            _escapeInput.action.Enable();
            _escapeInput.action.performed += RaiseEscapeCallback;
            
            PushContext(_defaultContext);
        }

        #endregion

        #region Input Behaviours
        private readonly List<IInputBehaviour> _behaviours = new(16);


        public void RegisterBehaviour(IInputBehaviour behaviour)
        {
#if DEBUG
            if (behaviour == null)
                throw new ArgumentNullException(nameof(behaviour));

            if (_behaviours.Contains(behaviour))
            {
                Debug.LogWarning("This behaviour has already been registered.");
                return;
            }
#endif

            _behaviours.Add(behaviour);
            UpdateBehaviourStatus(behaviour, ActiveContext.BehaviourTypes);
        }

        public void UnregisterBehaviour(IInputBehaviour behaviour)
        {
#if DEBUG
            if (!UnityUtils.IsPlayMode)
                return;

            if (behaviour == null)
                throw new ArgumentNullException(nameof(behaviour));

            if (!_behaviours.Remove(behaviour))
                Debug.LogError("Trying to unregister a behaviour that has not been registered.");
#else
            _behaviours.Remove(behaviour);
#endif
        }

        private static void UpdateBehaviourStatus(IInputBehaviour behaviour, SerializedType[] types)
        {
            behaviour.Enabled = behaviour.EnableMode switch
            {
                InputEnableMode.BasedOnContext => Array.Exists(types, serializedType => serializedType.Type == behaviour.GetType()),
                InputEnableMode.AlwaysEnabled => true,
                InputEnableMode.AlwaysDisabled => false,
                InputEnableMode.Manual => behaviour.Enabled,
                _ => false
            };
        }
        #endregion
        
        #region Input Contexts
        public InputContext ActiveContext { get; private set; }
        public int ContextStackCount => _contextStack.Count;
        public IReadOnlyList<InputContext> ContextStack => _contextStack;
        
        public event UnityAction ContextChanged;

        [SerializeField, NotNull, BeginGroup]
        private InputContext _defaultContext;

        private readonly List<InputContext> _contextStack = new();

        
        public void PushContext(InputContext context)
        {
            if (context == null)
                return;
            
            _contextStack.Add(context);

            // Disable previous context and enable new.
            if (ActiveContext != context)
            {
                ActiveContext = context;
                
                foreach (var behaviour in _behaviours)
                    UpdateBehaviourStatus(behaviour, context.BehaviourTypes);
                
                ContextChanged?.Invoke();
            }
        }

        public bool IsDefaultContext() => ActiveContext == _defaultContext;
        
        public void PopContext(InputContext context)
        {
            if (context == null)
                return;
            
            int index = _contextStack.IndexOf(context);

            if (index != -1)
            {
                _contextStack.RemoveAt(index);

                if (ActiveContext == context)
                {
                    if (_contextStack.Count == 0)
                        PushContext(_defaultContext);
                    else
                    {
                        var contextToEnable = _contextStack[^1];
                        ActiveContext = contextToEnable;
                        
                        foreach (var behaviour in _behaviours)
                            UpdateBehaviourStatus(behaviour, contextToEnable.BehaviourTypes);
                        
                        ContextChanged?.Invoke();
                    }
                }
            }
        }
        #endregion

        #region Escape Callbacks
        public bool HasEscapeCallbacks => _escapeCallbacks.Count > 0 || _lastEscapeCallbackRemoveFrame == Time.frameCount;
        public IReadOnlyList<UnityAction> EscapeCallbacks => _escapeCallbacks;
        public int EscapeCallbacksCount => _escapeCallbacks.Count;

        [SerializeField, EndGroup]
        private InputActionReference _escapeInput;

        private int _lastEscapeCallbackRemoveFrame;
        private readonly List<UnityAction> _escapeCallbacks = new();


        public void PushEscapeCallback(UnityAction action)
        {
            if (action == null)
                return;
            
            int index = _escapeCallbacks.IndexOf(action);

            if (index != -1)
                _escapeCallbacks.RemoveAt(index);
            
            _escapeCallbacks.Add(action);
        }

        public void PopEscapeCallback(UnityAction action)
        {
            int index = _escapeCallbacks.IndexOf(action);
            if (index != -1)
                _escapeCallbacks.RemoveAt(index);
        }

        private void RaiseEscapeCallback(InputAction.CallbackContext context)
        {
            if (_escapeCallbacks.Count == 0)
                return;

            int lastCallbackIndex = _escapeCallbacks.Count - 1;
            var callback = _escapeCallbacks[lastCallbackIndex];
            _escapeCallbacks.RemoveAt(lastCallbackIndex);
            _lastEscapeCallbackRemoveFrame = Time.frameCount;
            callback.Invoke();
        }
        #endregion
    }
}

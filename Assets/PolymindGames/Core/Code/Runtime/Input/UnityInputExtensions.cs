using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PolymindGames.InputSystem
{
    public static class UnityInputExtensions
    {
        private static readonly Dictionary<InputActionReference, int> s_EnabledActions = new();


        #region Initialization
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reload()
        {
            foreach (var action in s_EnabledActions.Keys)
                action.action.Disable();
            
            s_EnabledActions.Clear();
        }
#endif
        #endregion

        public static void RegisterStarted(this InputActionReference actionRef, Action<InputAction.CallbackContext> callback)
        {
            CheckForNull(actionRef);

            Enable(actionRef);
            actionRef.action.started += callback;
        }

        public static void RegisterPerformed(this InputActionReference actionRef, Action<InputAction.CallbackContext> callback)
        {
            CheckForNull(actionRef);

            Enable(actionRef);
            actionRef.action.performed += callback;
        }

        public static void RegisterCanceled(this InputActionReference actionRef, Action<InputAction.CallbackContext> callback)
        {
            CheckForNull(actionRef);

            Enable(actionRef);
            actionRef.action.canceled += callback;
        }

        public static void UnregisterStarted(this InputActionReference actionRef, Action<InputAction.CallbackContext> callback)
        {
            CheckForNull(actionRef);

            actionRef.action.started -= callback;
            TryDisable(actionRef);
        }

        public static void UnregisterPerformed(this InputActionReference actionRef, Action<InputAction.CallbackContext> callback)
        {
            CheckForNull(actionRef);

            actionRef.action.performed -= callback;
            TryDisable(actionRef);
        }

        public static void UnregisterCanceled(this InputActionReference actionRef, Action<InputAction.CallbackContext> callback)
        {
            CheckForNull(actionRef);

            actionRef.action.canceled -= callback;
            TryDisable(actionRef);
        }

        public static void Enable(this InputActionReference actionRef)
        {
            CheckForNull(actionRef);

            if (s_EnabledActions.TryGetValue(actionRef, out var listenerCount))
                s_EnabledActions[actionRef] = listenerCount + 1;
            else
            {
                s_EnabledActions.Add(actionRef, 1);
                actionRef.action.Enable();
            }
        }

        public static void TryDisable(this InputActionReference actionRef)
        {
            CheckForNull(actionRef);

            if (s_EnabledActions.TryGetValue(actionRef, out var listenerCount))
            {
                listenerCount--;
                if (listenerCount == 0)
                {
                    s_EnabledActions.Remove(actionRef);
                    actionRef.action.Disable();
                }
                else
                    s_EnabledActions[actionRef] = listenerCount;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void CheckForNull(InputActionReference actionRef)
        {
#if DEBUG
            if (actionRef == null)
                Debug.LogError("The passed input action is null, you need to set it in the inspector.");
#endif
        }
    }
}

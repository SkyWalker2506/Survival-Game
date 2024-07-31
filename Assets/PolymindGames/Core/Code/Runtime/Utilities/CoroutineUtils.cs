using UnityEngine.Events;
using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace PolymindGames
{
    /// <summary>
    /// Utility class for managing coroutines in Unity.
    /// </summary>
    public static class CoroutineUtils
    {
        private static GlobalMonoBehaviour s_GlobalMonoBehaviour;
        private static bool s_HasGlobalMonoBehaviour;
        private const float MIN_INVOKE_DELAY = 0.001f;


        /// <summary>
        /// Gets the global MonoBehaviour instance used for starting global coroutines.
        /// </summary>
        private static MonoBehaviour GlobalBehaviour
        {
            get
            {
                if (!s_HasGlobalMonoBehaviour)
                {
                    var gameObj = new GameObject("GlobalMonoBehaviour")
                    {
                        hideFlags = HideFlags.HideInHierarchy
                    };

                    s_GlobalMonoBehaviour = gameObj.AddComponent<GlobalMonoBehaviour>();
                    s_HasGlobalMonoBehaviour = true;
                }

                return s_GlobalMonoBehaviour;
            }
        }

        /// <summary>
        /// Starts a coroutine on the global MonoBehaviour instance.
        /// </summary>
        /// <param name="enumerator">The coroutine to start.</param>
        /// <returns>The Coroutine object representing the started coroutine.</returns>
        public static Coroutine StartGlobalCoroutine(IEnumerator enumerator) =>
            GlobalBehaviour.StartCoroutine(enumerator);

        /// <summary>
        /// Invokes an action after a specified delay using a coroutine on the global MonoBehaviour instance.
        /// </summary>
        /// <param name="action">The action to invoke.</param>
        /// <param name="delay">The delay before invoking the action.</param>
        /// <returns>The Coroutine object representing the delayed invocation.</returns>
        public static Coroutine InvokeDelayedGlobal(UnityAction action, float delay)
        {
            CheckNullAction(action);

            if (delay < MIN_INVOKE_DELAY)
            {
                action.Invoke();
                return null;
            }

            return StartGlobalCoroutine(C_InvokeDelayed(action, delay));
        }

        /// <summary>
        /// Stops a coroutine running on the global MonoBehaviour instance.
        /// </summary>
        /// <param name="coroutine">Reference to the coroutine to stop.</param>
        public static void StopGlobalCoroutine(ref Coroutine coroutine)
        {
            if (coroutine != null)
            {
                GlobalBehaviour.StopCoroutine(coroutine);
                coroutine = null;
            }
        }

        /// <summary>
        /// Stops a coroutine running on a specific MonoBehaviour instance.
        /// </summary>
        /// <param name="parent">The MonoBehaviour instance running the coroutine.</param>
        /// <param name="routine">Reference to the coroutine to stop.</param>
        public static void StopCoroutine(MonoBehaviour parent, ref Coroutine routine)
        {
            if (routine != null)
            {
                parent.StopCoroutine(routine);
                routine = null;
            }
        }

        /// <summary>
        /// Starts a coroutine on a specific MonoBehaviour instance, replacing any existing coroutine.
        /// </summary>
        /// <param name="parent">The MonoBehaviour instance to start the coroutine on.</param>
        /// <param name="enumerator">The coroutine enumerator to start.</param>
        /// <param name="coroutine">Reference to the existing coroutine, if any.</param>
        public static void StartAndReplaceCoroutine(MonoBehaviour parent, IEnumerator enumerator, ref Coroutine coroutine)
        {
            CheckNullParent(parent);
            StopCoroutine(parent, ref coroutine);
            coroutine = parent.StartCoroutine(enumerator);
        }

        /// <summary>
        /// Invokes an action repeatedly for a specified duration on a specific MonoBehaviour instance.
        /// </summary>
        /// <param name="parent">The MonoBehaviour instance to run the action on.</param>
        /// <param name="action">The action to invoke repeatedly.</param>
        /// <param name="duration">The duration for which to run the action (default is infinite).</param>
        /// <returns>The Coroutine object representing the invoked loop.</returns>
        public static Coroutine InvokeLooped(MonoBehaviour parent, UnityAction action, float duration = Mathf.Infinity)
        {
            CheckNullAction(action);
            CheckNullParent(parent);
            return parent.StartCoroutine(C_LoopAction(action, duration));
        }

        /// <summary>
        /// Invokes an action after a specified delay on a specific MonoBehaviour instance.
        /// </summary>
        /// <param name="parent">The MonoBehaviour instance to run the action on.</param>
        /// <param name="action">The action to invoke after the delay.</param>
        /// <param name="delay">The delay before invoking the action.</param>
        /// <returns>The Coroutine object representing the delayed invocation.</returns>
        public static Coroutine InvokeDelayed(MonoBehaviour parent, UnityAction action, float delay)
        {
            CheckNullAction(action);
            CheckNullParent(parent);

            if (delay < MIN_INVOKE_DELAY)
            {
                action.Invoke();
                return null;
            }

            return parent.StartCoroutine(C_InvokeDelayed(action, delay));
        }

        /// <summary>
        /// Invokes an action with a parameter after a specified delay on a specific MonoBehaviour instance.
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the action.</typeparam>
        /// <param name="parent">The MonoBehaviour instance to run the action on.</param>
        /// <param name="action">The action to invoke after the delay.</param>
        /// <param name="value">The parameter value for the action.</param>
        /// <param name="delay">The delay before invoking the action.</param>
        /// <returns>The Coroutine object representing the delayed invocation.</returns>
        public static Coroutine InvokeDelayed<T>(MonoBehaviour parent, UnityAction<T> action, T value, float delay)
        {
            CheckNullAction(action);

            if (delay < MIN_INVOKE_DELAY)
            {
                action.Invoke(value);
                return null;
            }

            return parent.StartCoroutine(C_InvokeDelayed(action, value, delay));
        }

        /// <summary>
        /// Invokes an action on the next frame on a specific MonoBehaviour instance.
        /// </summary>
        /// <param name="parent">The MonoBehaviour instance to run the action on.</param>
        /// <param name="action">The action to invoke on the next frame.</param>
        /// <returns>The Coroutine object representing the delayed invocation.</returns>
        public static Coroutine InvokeNextFrame(MonoBehaviour parent, UnityAction action)
        {
            CheckNullAction(action);
            CheckNullParent(parent);
            return parent.StartCoroutine(C_InvokeOneFrameLater(action));
        }

        /// <summary>
        /// Invokes an action with a parameter on the next frame on a specific MonoBehaviour instance.
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the action.</typeparam>
        /// <param name="parent">The MonoBehaviour instance to run the action on.</param>
        /// <param name="action">The action to invoke on the next frame.</param>
        /// <param name="value">The parameter value for the action.</param>
        /// <returns>The Coroutine object representing the delayed invocation.</returns>
        public static Coroutine InvokeNextFrame<T>(MonoBehaviour parent, UnityAction<T> action, T value)
        {
            CheckNullAction(action);
            CheckNullParent(parent);
            return parent.StartCoroutine(C_InvokeOneFrameLater(action, value));
        }

        /// <summary>
        /// Coroutine to repeatedly invoke an action over a specified duration of time.
        /// </summary>
        /// <param name="action">The action to be invoked.</param>
        /// <param name="duration">The duration of time over which the action will be invoked repeatedly.</param>
        /// <returns>An IEnumerator representing the coroutine.</returns>
        private static IEnumerator C_LoopAction(UnityAction action, float duration)
        {
            while (duration > 0f)
            {
                duration -= Time.deltaTime;
                action();
                yield return null;
            }
        }

        /// <summary>
        /// Coroutine to invoke an action after a specified delay.
        /// </summary>
        /// <param name="action">The action to be invoked.</param>
        /// <param name="delay">The delay before invoking the action.</param>
        /// <returns>An IEnumerator representing the coroutine.</returns>
        private static IEnumerator C_InvokeDelayed(UnityAction action, float delay)
        {
            while (delay > 0f)
            {
                delay -= Time.deltaTime;
                yield return null;
            }

            action.Invoke();
        }

        /// <summary>
        /// Coroutine to invoke an action with a parameter after a specified delay.
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the action.</typeparam>
        /// <param name="action">The action to be invoked.</param>
        /// <param name="value">The parameter value for the action.</param>
        /// <param name="delay">The delay before invoking the action.</param>
        /// <returns>An IEnumerator representing the coroutine.</returns>
        private static IEnumerator C_InvokeDelayed<T>(UnityAction<T> action, T value, float delay)
        {
            while (delay > 0f)
            {
                delay -= Time.deltaTime;
                yield return null;
            }

            action.Invoke(value);
        }

        /// <summary>
        /// Coroutine to invoke an action on the next frame.
        /// </summary>
        /// <param name="action">The action to be invoked.</param>
        /// <returns>An IEnumerator representing the coroutine.</returns>
        private static IEnumerator C_InvokeOneFrameLater(UnityAction action)
        {
            yield return null;
            action.Invoke();
        }

        /// <summary>
        /// Coroutine to invoke an action with a parameter on the next frame.
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the action.</typeparam>
        /// <param name="action">The action to be invoked.</param>
        /// <param name="value">The parameter value for the action.</param>
        /// <returns>An IEnumerator representing the coroutine.</returns>
        private static IEnumerator C_InvokeOneFrameLater<T>(UnityAction<T> action, T value)
        {
            yield return null;
            action.Invoke(value);
        }

        /// <summary>
        /// Checks if the provided UnityAction is null and logs an error in DEBUG mode if it is.
        /// </summary>
        /// <param name="action">The UnityAction to check for null.</param>
        [Conditional("DEBUG")]
        private static void CheckNullAction(UnityAction action)
        {
#if DEBUG
            if (action == null)
                Debug.LogError("Action is null.");
#endif
        }

        /// <summary>
        /// Checks if the provided UnityAction with a parameter is null and logs an error in DEBUG mode if it is.
        /// </summary>
        /// <typeparam name="T">The type of the parameter for the UnityAction.</typeparam>
        /// <param name="action">The UnityAction with a parameter to check for null.</param>
        [Conditional("DEBUG")]
        private static void CheckNullAction<T>(UnityAction<T> action)
        {
#if DEBUG
            if (action == null)
                Debug.LogError("Action is null.");
#endif
        }

        /// <summary>
        /// Checks if the provided MonoBehaviour parent is null and logs an error in DEBUG mode if it is.
        /// </summary>
        /// <param name="parent">The MonoBehaviour parent to check for null.</param>
        [Conditional("DEBUG")]
        private static void CheckNullParent(MonoBehaviour parent)
        {
#if DEBUG
            if (parent == null)
                Debug.LogError("Parent is null.");
#endif
        }

        /// <summary>
        /// Internal MonoBehaviour class used for global coroutine management.
        /// </summary>
        private sealed class GlobalMonoBehaviour : MonoBehaviour
        {
            /// <summary>
            /// Callback method called when the GameObject is destroyed.
            /// </summary>
            private void OnDestroy()
            {
                // Reset global references
                s_HasGlobalMonoBehaviour = false;
                s_GlobalMonoBehaviour = null;

                // Ensure the GameObject associated with this MonoBehaviour is destroyed
                if (gameObject != null)
                    Destroy(gameObject);
            }
        }
    }
}
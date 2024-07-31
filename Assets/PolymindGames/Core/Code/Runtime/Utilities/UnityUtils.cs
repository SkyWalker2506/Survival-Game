using System.Linq;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
#endif

namespace PolymindGames
{
    using UnityObject = Object;

    /// <summary>
    /// Provides utility methods for runtime operations in Unity.
    /// </summary>
    public static partial class UnityUtils
    {
#if !UNITY_EDITOR
        /// <summary>
        /// Throws a warning message when called at runtime, intended for use in editor-only methods like OnValidate.
        /// </summary>
        /// <param name="obj">The Unity object invoking the method.</param>
        /// <param name="onValidateAction">The action to perform on validation.</param>
        public static void SafeOnValidate(Object obj, UnityAction onValidateAction) 
        {
            Debug.LogWarning("SafeOnValidate should only be called from editor methods like OnValidate.");
        }
        
        /// <summary>
        /// Indicates whether the game is currently in play mode.
        /// </summary>
        public static bool IsPlayMode { get; private set; } = true;

        /// <summary>
        /// Indicates whether the game is in play mode for the first time since launch.
        /// </summary>
        public static bool IsFirstPlayMode { get; private set; } = true;
#endif

        private static Camera s_CachedMainCamera;

        /// <summary>
        /// Gets the main camera, caching it for subsequent calls.
        /// </summary>
        public static Camera CachedMainCamera
        {
            get
            {
                if (s_CachedMainCamera == null)
                    s_CachedMainCamera = Camera.main;

                return s_CachedMainCamera;
            }
        }
        
        /// <summary>
        /// Locks the cursor.
        /// </summary>
        public static void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        /// <summary>
        /// Unlocks the cursor.
        /// </summary>
        public static void UnlockCursor()
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Provides utility methods for editor-specific operations in Unity.
    /// </summary>
    public static partial class UnityUtils
    {
        /// <summary>
        /// Indicates whether the game is currently in play mode.
        /// </summary>
        public static bool IsPlayMode { get; private set; } = false;

        /// <summary>
        /// Indicates whether the game is in play mode for the first time since launch.
        /// </summary>
        public static bool IsFirstPlayMode { get; private set; }

        private const string FIRST_TIME_KEY = "HasEnteredPlayModeBefore";


        [DidReloadScripts]
        private static void OnEditorCompile()
        {
            if (EditorApplication.timeSinceStartup < 10f)
                EditorPrefs.SetBool(FIRST_TIME_KEY, false);
        }

        [InitializeOnEnterPlayMode]
        private static void LogPlayModeState(EnterPlayModeOptions options)
        {
            // When entering Play mode
            if (!EditorPrefs.GetBool(FIRST_TIME_KEY, false))
            {
                // Set the flag in EditorPrefs to true to indicate that play mode has been entered before
                EditorPrefs.SetBool(FIRST_TIME_KEY, true);
                IsFirstPlayMode = true;
            }
            else
                IsFirstPlayMode = false;
            
            IsPlayMode = true;
            Application.quitting += Quit;

            static void Quit()
            {
                IsPlayMode = false;
                Application.quitting -= Quit;
            }
        }

        public static void PingResourceAsset<T>() where T : Object
        {
            var resourceObject = Resources.LoadAll<T>(string.Empty).FirstOrDefault();

            if (resourceObject == null)
            {
                Debug.LogError($"No {nameof(T)} found in the resources folder!");
                return;
            }

            EditorGUIUtility.PingObject(resourceObject);
        }

        /// <summary>
        /// Sometimes, when you use Unity's built-in OnValidate, it will spam you with a very annoying warning message,
        /// even though nothing has gone wrong. To avoid this, you can run your OnValidate code through this utility.
        /// Runs <paramref name="onValidateAction"/> once, after all inspectors have been updated.
        /// </summary>
        public static void SafeOnValidate(UnityObject obj, UnityAction onValidateAction)
        {
            EditorApplication.delayCall += OnValidate;
            return;

            void OnValidate()
            {
                EditorApplication.delayCall -= OnValidate;

                // Important: this function could be called after the object has been destroyed
                // (ex: during reinitialization when entering Play Mode, when saving it in a prefab...),
                // so to prevent a potential ArgumentNullException, we check if "this" is null.
                // Note: the components we want to modify could also be in this "destroyed" state
                // and trigger an ArgumentNullException.

                // We also check if "this" is dirty, this is to prevent the scene to be marked
                // as dirty as soon as we load it (because we will dirty some components in this function).

                if (obj == null || !EditorUtility.IsDirty(obj))
                    return;

                onValidateAction();
            }
        }

        /// <summary>
        /// Determines whether a given component is part of a prefab asset or is being edited in prefab mode.
        /// </summary>
        /// <param name="component">The component to check.</param>
        /// <returns>True if the component is part of a prefab asset or is being edited in prefab mode; otherwise, false.</returns>
        public static bool IsAssetOnDisk(Component component)
        {
            return PrefabUtility.IsPartOfPrefabAsset(component) || IsEditingInPrefabMode(component);
        }

        private static bool IsEditingInPrefabMode(Component component)
        {
            if (component == null || component.gameObject == null)
                return false;

            if (EditorUtility.IsPersistent(component))
            {
                // if the game object is stored on disk, it is a prefab of some kind, despite not returning true for IsPartOfPrefabAsset =/
                return true;
            }

            // If the GameObject is not persistent let's determine which stage we are in first because getting Prefab info depends on it
            var mainStage = StageUtility.GetMainStageHandle();
            var currentStage = StageUtility.GetStageHandle(component.gameObject);
            if (currentStage != mainStage)
            {
                var prefabStage = PrefabStageUtility.GetPrefabStage(component.gameObject);
                if (prefabStage != null)
                    return true;
            }
            return false;
        }
    }
#endif
}
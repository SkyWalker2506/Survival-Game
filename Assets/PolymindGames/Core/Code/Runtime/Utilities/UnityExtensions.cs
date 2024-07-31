using System.Collections.Generic;
using UnityEngine;
using System;

namespace PolymindGames
{
    using Object = UnityEngine.Object;
    
    /// <summary>
    /// Extension methods for Unity types.
    /// </summary>
    public static class UnityExtensions
    {
    #if !UNITY_2021_3_OR_NEWER
        /// <summary>
        /// Sets the local position and rotation of a transform.
        /// </summary>
        /// <param name="trs">The transform to set the position and rotation of.</param>
        /// <param name="localPosition">The local position to set.</param>
        /// <param name="localRotation">The local rotation to set.</param>
        public static void SetLocalPositionAndRotation(this Transform trs, Vector3 localPosition, Quaternion localRotation)
        {
            trs.localPosition = localPosition;
            trs.localRotation = localRotation;
        }
    #endif
    
        /// <summary>
        /// Checks if a component belongs to a prefab.
        /// </summary>
        /// <param name="component">The component to check.</param>
        /// <returns>True if the component belongs to a prefab, otherwise false.</returns>
        public static bool IsPrefab(this Component component) => !component.gameObject.scene.IsValid();
    
        /// <summary>
        /// Checks if a game object is a prefab.
        /// </summary>
        /// <param name="gameObject">The game object to check.</param>
        /// <returns>True if the game object is a prefab, otherwise false.</returns>
        public static bool IsPrefab(this GameObject gameObject) => !gameObject.scene.IsValid();
    
        /// <summary>
        /// Checks if a transform is a child of another transform.
        /// </summary>
        /// <param name="transform">The transform to check.</param>
        /// <param name="root">The potential parent transform.</param>
        /// <returns>True if the transform is a child of the root transform, otherwise false.</returns>
        public static bool IsChildOfTransform(this Transform transform, Transform root)
        {
            Transform target = transform;
            while (target != null)
            {
                if (target == root)
                    return true;
                target = target.parent;
            }
    
            return false;
        }

        /// <summary>
        /// Sets the layer of the specified game object and all of its children recursively.
        /// </summary>
        /// <param name="gameObject">The game object to set the layers of.</param>
        /// <param name="layer">The layer to set.</param>
        public static void SetLayersInChildren(this GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
                child.gameObject.SetLayersInChildren(layer);
        }

        /// <summary>
        /// Gets a component of type T from the root of the specified game object's hierarchy.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <param name="gameObj">The game object whose root to search in.</param>
        /// <returns>A component of type T found in the root of the hierarchy, or null if none is found.</returns>
        public static T GetComponentInRoot<T>(this GameObject gameObj)
        {
            return gameObj.transform.root.GetComponentInChildren<T>();
        }

        /// <summary>
        /// Gets or adds a component of the specified type to the specified game object.
        /// </summary>
        /// <param name="gameObj">The game object to get or add the component to.</param>
        /// <param name="type">The type of component to get or add.</param>
        /// <returns>The component of the specified type attached to the game object, or a new component if none is found.</returns>
        public static Component GetOrAddComponent(this GameObject gameObj, Type type)
        {
            return gameObj.TryGetComponent(type, out var comp) ? comp : gameObj.AddComponent(type);
        }

        /// <summary>
        /// Gets, adds, or swaps a component of the specified type with a base type from the specified game object.
        /// </summary>
        /// <typeparam name="BaseType">The base type of component to retrieve.</typeparam>
        /// <param name="gameObj">The game object to get, add, or swap the component on.</param>
        /// <param name="targetType">The type of component to retrieve, add, or swap.</param>
        /// <returns>
        /// The component of the specified type attached to the game object if found, otherwise a new component is added or swapped.
        /// </returns>
        public static BaseType GetAddOrSwapComponent<BaseType>(this GameObject gameObj, Type targetType) where BaseType : Component
        {
            if (gameObj.TryGetComponent(out BaseType comp))
            {
                if (comp.GetType() != targetType)
                {
        #if UNITY_EDITOR
                    if (Application.isPlaying)
                        Object.Destroy(comp);
                    else
                        Object.DestroyImmediate(comp);
        #else
                    Object.Destroy(comp);
        #endif
                }
                else
                    return comp;
            }

            return gameObj.AddComponent(targetType) as BaseType;
        }

        /// <summary>
        /// Gets or adds a component of type T to the specified game object.
        /// </summary>
        /// <typeparam name="T">The type of component to get or add.</typeparam>
        /// <param name="gameObj">The game object to get or add the component to.</param>
        /// <returns>The component of type T attached to the game object, or a new component if none is found.</returns>
        public static T GetOrAddComponent<T>(this GameObject gameObj) where T : Component
        {
            return gameObj.TryGetComponent(out T comp) ? comp : gameObj.AddComponent<T>();
        }

        /// <summary>
        /// Checks if the specified game object has a component of type T attached.
        /// </summary>
        /// <typeparam name="T">The type of component to check for.</typeparam>
        /// <param name="gameObject">The game object to check.</param>
        /// <returns>True if the game object has a component of type T attached, otherwise false.</returns>
        public static bool HasComponent<T>(this GameObject gameObject)
        {
            return gameObject.TryGetComponent<T>(out _);
        }

        /// <summary>
        /// Tries to get a component of type T from the specified game object's hierarchy.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <param name="gameObj">The game object to search in.</param>
        /// <param name="comp">The found component of type T, if any.</param>
        /// <returns>True if a component of type T is found in the hierarchy, otherwise false.</returns>
        public static bool TryGetComponentInHierarchy<T>(this GameObject gameObj, out T comp)
        {
            comp = gameObj.GetComponentInChildren<T>();
            if (comp != null)
                return true;

            comp = gameObj.transform.root.GetComponentInChildren<T>();
            return comp != null;
        }

        /// <summary>
        /// Gets the first component of type T found in the immediate children of the specified game object.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <param name="gameObj">The game object to search in.</param>
        /// <returns>The first component of type T found in the immediate children, or null if none is found.</returns>
        public static T GetComponentInFirstChildren<T>(this GameObject gameObj) where T : class
        {
            var transform = gameObj.transform;
            int childCount = transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                if (transform.GetChild(i).TryGetComponent<T>(out var comp))
                    return comp;
            }

            return null;
        }
        
        /// <summary>
        /// Gets components of type T from the immediate children of the specified game object.
        /// </summary>
        /// <typeparam name="T">The type of components to retrieve.</typeparam>
        /// <param name="gameObj">The game object to search in.</param>
        /// <param name="multipleOnSameObject">Indicates whether to allow multiple components of type T on the same object.</param>
        /// <returns>A list of components of type T found in the immediate children.</returns>
        public static List<T> GetComponentsInFirstChildren<T>(this GameObject gameObj, bool multipleOnSameObject = false)
        {
            var list = new List<T>();
            GetComponentsInFirstChildren(gameObj.transform, list, multipleOnSameObject);
            return list;
        }

        /// <summary>
        /// Gets components of type T from the immediate children of the specified transform.
        /// </summary>
        /// <typeparam name="T">The type of components to retrieve.</typeparam>
        /// <param name="transform">The transform whose children to search in.</param>
        /// <param name="list">The list to which the found components will be added.</param>
        /// <param name="multipleOnSameObject">Indicates whether to allow multiple components of type T on the same object.</param>
        public static void GetComponentsInFirstChildren<T>(this Transform transform, List<T> list, bool multipleOnSameObject = false)
        {
            list.Clear();
            int childCount = transform.childCount;
            if (multipleOnSameObject)
            {
                for (int i = 0; i < childCount; i++)
                    list.AddRange(transform.GetChild(i).GetComponents<T>());
            }
            else
            {
                for (int i = 0; i < childCount; i++)
                {
                    if (transform.GetChild(i).TryGetComponent(out T component))
                        list.Add(component);
                }
            }
        }
    }
}
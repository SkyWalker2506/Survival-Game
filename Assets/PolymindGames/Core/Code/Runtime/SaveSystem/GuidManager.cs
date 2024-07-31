using System.Collections.Generic;
using UnityEngine;
using System;

namespace PolymindGames.SaveSystem
{
    // Class to handle registering and accessing objects by GUID
    public sealed class GuidManager
    {
        private static readonly GuidManager s_Instance = new();
        private readonly Dictionary<Guid, GuidComponent> _guidToObjectMap = new();


#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Init() => s_Instance._guidToObjectMap.Clear();
#endif

        public static bool Add(GuidComponent guidComponent) => s_Instance.InternalAdd(guidComponent);
        public static void Remove(Guid guid) => s_Instance.InternalRemove(guid);

        private bool InternalAdd(GuidComponent guidComponent)
        {
            Guid guid = guidComponent.InstanceGuid.Guid;

            if (_guidToObjectMap.TryAdd(guid, guidComponent))
                return true;

            var existingComponent = _guidToObjectMap[guid];

            if (existingComponent == guidComponent)
                return true;

            if (existingComponent == null)
            {
                _guidToObjectMap[guid] = guidComponent;
                return true;
            }

            // Normally, a duplicate GUID is a big problem, means you won't necessarily be referencing what you expect.
            if (Application.isPlaying)
            {
                Debug.AssertFormat(false, guidComponent, "Guid Collision Detected between {0} and {1}.\n Assigning new Guid. Consider tracking runtime instances using a direct reference or other method.",
                    _guidToObjectMap[guid] != null ? _guidToObjectMap[guid].name : "NULL", guidComponent != null ? guidComponent.name : "NULL");
            }
            else
            {
                // However, at editor time, copying an object with a GUID will duplicate the GUID resulting in a collision and repair.
                // We warn about this just for pedantry reasons, and so you can detect if you are unexpectedly copying these components.
                Debug.LogWarningFormat(guidComponent, "Guid Collision Detected while creating {0}.\nAssigning new Guid.",
                    guidComponent != null ? guidComponent.name : "NULL");
            }

            return false;
        }

        private void InternalRemove(Guid guid) => _guidToObjectMap.Remove(guid);
    }
}
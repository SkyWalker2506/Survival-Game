using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [RequireCharacterComponent(typeof(IMovementControllerCC))]
    public sealed class MotionDataHandler : MonoBehaviour, IMotionDataHandler
    {
        [SerializeField, BeginGroup, EndGroup]
#if UNITY_EDITOR
        [OnValueChanged(nameof(Editor_PresetChanged))]
#endif
        private MotionProfile _profile;
        
        private static readonly Dictionary<Type, Type> s_DataInterfaceTypes;
        
        private readonly Dictionary<Type, BehaviourEntry> _data = new();
        private MovementStateType _stateType;


        static MotionDataHandler()
        {
            Type baseType = typeof(IMotionData);
            var dataTypes =
                baseType.Assembly.GetTypes().Where(type => !type.IsInterface && baseType.IsAssignableFrom(type) && type != baseType).ToArray();

            for (var i = 0; i < dataTypes.Length; i++)
            {
                var dataType = dataTypes[i];
                var interfaces = dataType.GetInterfaces();
                for (var j = 0; j < interfaces.Length; j++)
                {
                    var dataInterface = interfaces[j];
                    if (dataInterface != baseType && baseType.IsAssignableFrom(dataInterface))
                    {
                        s_DataInterfaceTypes ??= new Dictionary<Type, Type>();
                        s_DataInterfaceTypes.Add(dataType, dataInterface);
                        break;
                    }
                }
            }
        }

        public void SetPreset(MotionProfile profile)
        {
#if UNITY_EDITOR
            if (_profile != null)
                _profile.OnPresetChangedEditor -= UpdateAllEntries;
#endif

            _profile = profile != null ? profile : _profile;

#if UNITY_EDITOR
            if (_profile != null)
                _profile.OnPresetChangedEditor += UpdateAllEntries;
#endif

            UpdateAllEntries();
        }

        public void RegisterBehaviour<T>(MotionDataChangedDelegate changedCallback)
        {
            if (_data.TryGetValue(typeof(T), out var entry))
                entry.Callbacks.Add(changedCallback);
            else
            {
                entry = new BehaviourEntry(changedCallback);
                _data.Add(typeof(T), entry);
            }
        }

        public void UnregisterBehaviour<T>(MotionDataChangedDelegate changedCallback)
        {
            if (_data.TryGetValue(typeof(T), out var entry))
            {
                var callbacks = entry.Callbacks;
                if (callbacks.Remove(changedCallback))
                {
                    if (callbacks.Count == 0)
                        _data.Remove(typeof(T));
                }
            }
        }

        public void SetDataOverride<T>(T data) where T : IMotionData
        {
            if (_data.TryGetValue(typeof(T), out var entry))
            {
                entry.Override = data;
                InvokeEntry(entry, false);
            }
        }

        public void SetDataOverride(IMotionData data, bool enable)
        {
            var dataType = data.GetType();

            if (s_DataInterfaceTypes.TryGetValue(dataType, out var interfaceType))
                dataType = interfaceType;

            if (_data.TryGetValue(dataType, out var entry))
            {
                entry.Override = enable ? data : null;
                InvokeEntry(entry, false);
            }
        }

        public T GetData<T>() where T : IMotionData
        {
            if (_data.TryGetValue(typeof(T), out var entry) && entry.Override != null)
                return (T)entry.Override;

            if (_profile == null)
                return default(T);

            return _profile.GetData<T>(_stateType) ?? _profile.GetData<T>(MovementStateType.None);
        }

        public bool TryGetData<T>(out T data) where T : IMotionData
        {
            data = GetData<T>();
            return data != null;
        }

        public void SetStateType(MovementStateType stateType)
        {
#if UNITY_EDITOR
            _stateType = _stateToVisualize ?? stateType;
#else
            _stateType = stateType;
#endif

            UpdateAllEntries();
        }

        private void Start() => SetPreset(_profile);

        private void InvokeEntry(BehaviourEntry entry, bool forceUpdate)
        {
            var callbacks = entry.Callbacks;
            for (int i = 0; i < callbacks.Count; i++)
                callbacks[i].Invoke(this, forceUpdate);
        }

        private void UpdateAllEntries()
        {
            foreach (var entry in _data.Values)
            {
                if (entry.Override == null)
                    InvokeEntry(entry, false);
            }
        }

        #region Internal
        private sealed class BehaviourEntry
        {
            public readonly List<MotionDataChangedDelegate> Callbacks;
            public IMotionData Override;


            public BehaviourEntry(MotionDataChangedDelegate callback)
            {
                Callbacks = new List<MotionDataChangedDelegate>
                {
                    callback
                };
                Override = null;
            }
        }
        #endregion

#if UNITY_EDITOR
        private MovementStateType? _stateToVisualize;


        public void Visualize(MovementStateType? stateToVisualize)
        {
            if (!Application.isPlaying)
                return;

            _stateToVisualize = stateToVisualize;
            SetStateType(stateToVisualize.GetValueOrDefault());
        }

        [Conditional("UNITY_EDITOR")]
        public void Editor_PresetChanged()
        {
            if (Application.isPlaying)
                SetPreset(_profile);
        }
#endif
    }
}
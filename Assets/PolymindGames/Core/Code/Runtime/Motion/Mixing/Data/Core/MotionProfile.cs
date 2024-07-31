using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolymindGames.ProceduralMotion
{
    [CreateAssetMenu(menuName = "Polymind Games/Motion/Motion Preset", fileName = "Motion_", order = 100)]
    public sealed class MotionProfile : ScriptableObject
    {
        [SerializeField, BeginGroup, EndGroup]
        [Help("Fallback preset. If a certain type of data is not found in this preset, the default preset will be used instead.")]
        private MotionProfile _baseProfile;

        [SerializeField, SpaceArea, LabelFromChild("State")]
#if UNITY_EDITOR
        [ReorderableListWithCallbacks(ListStyle.Boxed, OverrideNewElementMethodName = nameof(GetDefaultState), OverrideRemoveElementMethodName = nameof(OnMotionRemoved))]
#endif
        [Help("The data from the ''None'' state will be used if no data of the needed type is found in the current state.")]
        private StateData[] _motionData = Array.Empty<StateData>();

        private Dictionary<Type, IMotionData>[] _states;


        public T GetData<T>(MovementStateType stateType) where T : IMotionData
        {
            _states ??= CreateDataCache();

            int index = stateType.GetValue();
            if (_states.Length > index)
            {
                var dict = _states[index];

                if (dict != null && dict.TryGetValue(typeof(T), out var data))
                    return (T)data;
            }

            return _baseProfile != null ? _baseProfile.GetData<T>(stateType) : default(T);
        }

        private Dictionary<Type, IMotionData>[] CreateDataCache()
        {
            int maxState = 0;
            foreach (var data in _motionData)
            {
                int stateValue = data.State.GetValue();
                if (stateValue > maxState)
                    maxState = stateValue;
            }

            var states = new Dictionary<Type, IMotionData>[maxState + 1];
            foreach (var motionData in _motionData)
            {
                var dict = new Dictionary<Type, IMotionData>(motionData.Data.Length);
                foreach (var data in motionData.Data)
                {
                    if (data == null)
                    {
                        Debug.LogError(motionData.State, this);
                        return states;
                    }

                    var dataType = data.GetType();
                    dataType = GetMotionDataInterface(dataType) ?? dataType;

                    dict.Add(dataType, data);
                }

                int i = (int)motionData.State;
                states[i] = dict;
            }

#if !UNITY_EDITOR
            _motionData = null;
#endif

            return states;

            static Type GetMotionDataInterface(Type dataType)
            {
                return dataType.GetInterfaces().FirstOrDefault(type => type != typeof(IMotionData) && typeof(IMotionData).IsAssignableFrom(type));
            }
        }

        #region Internal
        [Serializable]
        public struct StateData
        {
            [BeginGroup]
            public MovementStateType State;

            [EndGroup, NestedScriptableListInLine(Foldable = false, HideSubAssets = false)]
            public MotionData[] Data;
        }
        #endregion

#if UNITY_EDITOR
        public event UnityAction OnPresetChangedEditor;

        private StateData GetDefaultState() => new()
        {
            Data = null,
            State = MovementStateType.None
        };

        private void OnMotionRemoved(int index)
        {
            if (_motionData.Length < index)
                return;

            if (_motionData[index].Data != null)
            {
                foreach (var data in _motionData[index].Data)
                {
                    AssetDatabase.RemoveObjectFromAsset(data);
                    DestroyImmediate(data);
                }

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private void Reset()
        {
            _motionData = new[]
            {
                GetDefaultState()
            };
        }

        private void OnValidate()
        {
            if (_baseProfile != null)
            {
                if (_baseProfile == this || _baseProfile._baseProfile == this)
                {
                    EditorUtility.SetDirty(this);
                    _baseProfile = null;
                }
            }

            if (Application.isPlaying)
            {
                CreateDataCache();
                OnPresetChangedEditor?.Invoke();
            }
        }
#endif
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(ParentConstraint))]
    public sealed class WieldableArmsHandler : MonoBehaviour
    {
        [SerializeField, NotNull, BeginGroup, EndGroup]
        private Animator _animator;

        [SerializeField, ReorderableList(ListStyle.Boxed)]
        private ArmSet[] _armSets;

        private ParentConstraint _parentConstraint;
        private int _selectedArmsIndex;
        private bool _isVisible = true;
        private int _instanceId;

        private static readonly Dictionary<int, WieldableArmsHandler> s_ArmInstances = new();


        public Animator Animator => _animator;

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
                _armSets[_selectedArmsIndex].Enable(value);
            }
        }

        public static WieldableArmsHandler GetArmInstanceFromPrefab(WieldableArmsHandler prefab)
        {
            if (prefab == null || prefab.GetInstanceID() < 0)
                return null;

            int prefabId = prefab.GetInstanceID();

            if (s_ArmInstances.TryGetValue(prefabId, out var instance))
                instance.gameObject.SetActive(true);
            else
            {
                instance = Instantiate(prefab, Player.LocalPlayer.GetCC<IWieldableControllerCC>().transform);
                instance._instanceId = prefabId;
            }

            return instance;
        }

        public void EnableArms() => gameObject.SetActive(true);
        public void DisableArms() => gameObject.SetActive(false);

        public void ToggleNextArmSet()
        {
            var prevArms = _armSets[_selectedArmsIndex];
            var arms = _armSets.Select(ref _selectedArmsIndex, SelectionType.Sequence);

            prevArms.Enable(false);
            arms.Enable(_isVisible);
        }

        private void OnEnable()
        {
            var character = GetComponentInParent<IFPSCharacter>();
            if (character == null)
            {
                Debug.LogError("This behaviour requires a parent character.", gameObject);
                return;
            }

            var source = new ConstraintSource
            {
                weight = 1f,
                sourceTransform = character.HandsMotionMixer.TargetTransform
            };
            
            if (_parentConstraint.sourceCount == 0)
                _parentConstraint.AddSource(source);
            else
                _parentConstraint.SetSource(0, source);
        }

        private void Awake()
        {
            if (_armSets.Length == 0)
            {
                Debug.LogError("No arm sets assigned.", gameObject);
                return;
            }

            _armSets[0].Enable(true);
            for (int i = 1; i < _armSets.Length; i++)
                _armSets[i].Enable(false);

            _parentConstraint = GetComponent<ParentConstraint>();
            _parentConstraint.constraintActive = true;
        }

        private void Start() => s_ArmInstances.Add(_instanceId, this);
        private void OnDestroy() => s_ArmInstances.Remove(_instanceId);
        
        #region Internal
        [Serializable]
        private struct ArmSet
        {
            public string Name;
            public SkinnedMeshRenderer LeftArm;
            public SkinnedMeshRenderer RightArm;

            public void Enable(bool enable)
            {
                LeftArm.gameObject.SetActive(enable);
                RightArm.gameObject.SetActive(enable);
            }
        }
        #endregion
    }
}
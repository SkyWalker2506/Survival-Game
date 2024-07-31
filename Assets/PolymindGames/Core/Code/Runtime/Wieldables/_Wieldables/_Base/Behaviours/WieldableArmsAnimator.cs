using System.Linq;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Polymind Games/Wieldables/Behaviours/Wieldable Arms Animator")]
    public sealed class WieldableArmsAnimator : MonoBehaviour, IAnimator
    {
        [BeginGroup, EndGroup]
        [SerializeField, PrefabObjectOnly, NotNull]
        [Tooltip("The prefab for the wieldable arms handler.")]
        private WieldableArmsHandler _armsPrefab;

        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("The animation override clips.")]
        private AnimationOverrideClips _clips;

        private WieldableArmsHandler _armsHandler;
        private Animator _animator;
        private float _holsterSpeed;


        public bool IsAnimating
        {
            get => _animator.speed != 0f;
            set => _animator.speed = value ? 1f : 0f;
        }

        public bool IsVisible
        {
            get => _armsHandler.IsVisible;
            set => _armsHandler.IsVisible = value;
        }

        public void SetFloat(int id, float value)
        {
            if (id == WieldableAnimationConstants.HOLSTER_SPEED)
                _animator.SetFloat(id, _holsterSpeed * value);
            else
                _animator.SetFloat(id, value);
        }

        public void SetBool(int id, bool value) => _animator.SetBool(id, value);
        public void SetInteger(int id, int value) => _animator.SetInteger(id, value);
        public void SetTrigger(int id) => _animator.SetTrigger(id);
        public void ResetTrigger(int id) => _animator.ResetTrigger(id);

        private void Awake()
        {
            FixHolsterSpeed();
            _armsHandler = WieldableArmsHandler.GetArmInstanceFromPrefab(_armsPrefab);
            _animator = _armsHandler.Animator;
        }
        
        private void OnEnable()
        {
            _armsHandler.EnableArms();
            _animator.runtimeAnimatorController = _clips.OverrideController;
            for (int i = 0; i < _clips.DefaultParameters.Length; i++)
                _clips.DefaultParameters[i].TriggerParameter(_animator);
        }

        private void OnDisable() => _armsHandler.DisableArms();

        private void FixHolsterSpeed()
        {
            var param = _clips.DefaultParameters.FirstOrDefault(trigger => trigger.Hash == WieldableAnimationConstants.HOLSTER_SPEED);
            _holsterSpeed = param?.Value ?? 1f;
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (_clips != null && TryGetComponent<WieldableAnimator>(out var animator))
                _clips.Controller = animator.Clips.Controller;
        }

        private void OnValidate()
        {
            if (Application.isPlaying)
                FixHolsterSpeed();
        }
#endif
    }
}
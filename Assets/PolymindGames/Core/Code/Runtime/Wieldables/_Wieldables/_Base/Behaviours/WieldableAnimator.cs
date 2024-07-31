using System.Linq;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Polymind Games/Wieldables/Behaviours/Wieldable Animator")]
    public sealed class WieldableAnimator : MonoBehaviour, IAnimator
    {
        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("The Animator component.")]
        private Animator _animator;

        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("The animation override clips.")]
        private AnimationOverrideClips _clips;

        private Renderer[] _renderers;
        private float _holsterSpeed;


        public AnimationOverrideClips Clips => _clips;
        public Animator Animator => _animator;

        public bool IsAnimating
        {
            get => _animator.speed != 0f;
            set => _animator.speed = value ? 1f : 0f;
        }

        public bool IsVisible
        {
            get => true;
            set { }
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
            if (_clips.Controller != null)
                _animator.runtimeAnimatorController = _clips.OverrideController;
            
            FixHolsterSpeed();
        }

        private void OnEnable()
        {
            for (int i = 0; i < _clips.DefaultParameters.Length; i++)
                _clips.DefaultParameters[i].TriggerParameter(_animator);
        }

        private void FixHolsterSpeed()
        {
            var param = _clips.DefaultParameters.FirstOrDefault(trigger => trigger.Hash == WieldableAnimationConstants.HOLSTER_SPEED);
            _holsterSpeed = param?.Value ?? 1f;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();

            if (_clips != null)
            {
                var controller = _clips.Controller;
                if (_animator != null)
                    _animator.runtimeAnimatorController = controller;
            }

            if (Application.isPlaying)
                FixHolsterSpeed();
        }
#endif
    }
}
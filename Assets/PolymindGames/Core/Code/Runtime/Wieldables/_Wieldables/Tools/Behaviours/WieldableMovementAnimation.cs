using System;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Wieldable Movement Animation")]
    public class WieldableMovementAnimation : MonoBehaviour
    {
        [SerializeField]
        [ReorderableList(ListStyle.Boxed), LabelFromChild(nameof(AnimationTrigger.State))]
        private AnimationTrigger[] _animationTriggers;

        private IMovementControllerCC _movement;
        private IWieldable _wieldable;


        private void Awake()
        {
            _wieldable = GetComponentInParent<IWieldable>();
            foreach (var trigger in _animationTriggers)
                trigger.Initialize(_wieldable.Animation);
        }

        private void OnEnable()
        {
            if (_wieldable.Character.TryGetCC(out _movement))
            {
                AnimationTrigger activeAnim = null;
                var activeState = _movement.ActiveState;
                foreach (var anim in _animationTriggers)
                {
                    _movement.AddStateTransitionListener(anim.State, anim.Trigger, anim.Transition);

                    if (anim.State == activeState && anim.Transition == MovementStateTransitionType.Enter)
                        activeAnim = anim;
                }

                if (activeAnim != null)
                    CoroutineUtils.InvokeDelayed(this, () => activeAnim.Trigger(activeAnim.State), 0.1f);
            }
        }

        private void OnDisable()
        {
            if (_wieldable.Character.TryGetCC(out _movement))
            {
                foreach (var anim in _animationTriggers)
                    _movement.RemoveStateTransitionListener(anim.State, anim.Trigger, anim.Transition);
            }
        }

        #region Internal
        [Serializable]
        private sealed class AnimationTrigger
        {
            public MovementStateType State;
            public MovementStateTransitionType Transition;

            [SerializeField, ReorderableList(ListStyle.Boxed)]
            private AnimatorParameterTrigger[] _parameters = Array.Empty<AnimatorParameterTrigger>();

            private IAnimator _animator;


            public void Initialize(IAnimator animator) => _animator = animator;

            public void Trigger(MovementStateType stateType)
            {
                for (int i = 0; i < _parameters.Length; i++)
                {
                    var parameter = _parameters[i];
                    _animator.SetParameter(parameter.Type, parameter.Hash, parameter.Value);
                }
            }
        }
        #endregion
    }
}
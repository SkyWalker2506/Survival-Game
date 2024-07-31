using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [Serializable]
    public abstract class ComponentTween
    {
        public abstract void Play(bool useUnscaledTime, int loopCount, bool usePingPong);
        public abstract void SetTime(float f);
        public abstract void Stop();

#if UNITY_EDITOR
        public abstract void Validate(GameObject gameObject);
#endif
    }

    [Serializable]
    public abstract class ComponentTween<TweenValueType, ComponentType> : ComponentTween
        where TweenValueType : struct
        where ComponentType : Component
    {
        [SerializeField, NotNull]
        [Tooltip("The component to modify.")]
        protected ComponentType _component;

        [SerializeField, Range(0f, 100f), SpaceArea]
        [Tooltip("Added to the base sequence delay.")]
        private float _delay;

        [SerializeField, Range(0f, 100f)]
        [Tooltip("The delay of the Tween, when defines will hold the Tween for its first play. " +
                 "Any play after that when looping or ping-ponging will not be delayed.")]
        private float _duration = 1f;

        [SerializeField]
        [Tooltip("The Ease type of the Tween defining the style of animation.")]
        private EaseType _easeType = 0;

        [SerializeField, SpaceArea]
        [Tooltip("Is the from value the same as the current value?")]
        private bool _fromIsCurrent = true;

        [SerializeField]
        [HideIf(nameof(_fromIsCurrent), true)]
        [Tooltip("The starting value.")]
        private TweenValueType _valueFrom;

        [SerializeField]
        [Tooltip("Is the target value relative to the starting value?")]
        private bool _toIsRelative;

        [SerializeField]
        [Tooltip("The target value.")]
        private TweenValueType _valueTo;
        
        private ValueTween<TweenValueType> _tween;


        public override void Play(bool useUnscaledTime, int loopCount, bool usePingPong)
        {
            Stop();

            var valueFrom = _fromIsCurrent
                ? GetValueFromComponent()
                : _valueFrom;
            
            _tween = Tweens.Get(valueFrom)
                .SetUpdateCallback(OnUpdate)
                .SetDuration(_duration)
                .SetEase(_easeType)
                .SetLoop(loopCount, usePingPong)
                .SetCompleteCallback(OnComplete);
            
            var valueTo = _toIsRelative
                ? _tween.CombineValues(_valueFrom, _valueTo)
                : _valueTo;

            _tween.SetTo(valueTo, false)
                .Play(_component, _delay, useUnscaledTime);
        }

        private void OnComplete()
        {
            Tweens.Release(_tween);
            _tween = null;
        }

        public override void SetTime(float f)
        {
            if (_tween != null)
                _tween.SetTime(f);
            else
            {
                var valueFrom = _fromIsCurrent
                    ? GetValueFromComponent()
                    : _valueFrom;

                _tween = Tweens.Get(valueFrom);
                    
                var valueTo = _toIsRelative
                    ? _tween.CombineValues(_valueFrom, _valueTo)
                    : _valueTo;

                _tween.SetTo(valueTo);
                _tween.SetTime(1f);
                
                OnUpdate(_tween.CurrentValue);
                Tweens.Release(_tween);
                
                _tween = null;
            }
        }

        public override void Stop() => _tween?.Stop();

        protected abstract void OnUpdate(TweenValueType value);
        protected abstract TweenValueType GetValueFromComponent();

#if UNITY_EDITOR
        public override void Validate(GameObject gameObject)
        {
            if (_component != null || gameObject.TryGetComponent(out _component))
            {
                if (_fromIsCurrent)
                    _valueFrom = GetValueFromComponent();
            }
        }
#endif
    }
}
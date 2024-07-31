using System;
using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public abstract class ValueTween
    {
        private Component _parent;
        private bool _isPlaying;


        public bool IsPlaying => _isPlaying;
        public Component Parent => _parent;

        /// <summary>
        /// Ticks the tween.
        /// </summary>
        /// <param name="deltaTime"></param>
        /// <returns>True if it reached the end.</returns>
        public abstract void Update(float deltaTime);

        public void Play(Component parent, float delay = 0f, bool useUnscaledTime = false)
        {
            if (_isPlaying)
                return;
            
            _parent = parent;
            Play(delay, useUnscaledTime);
        }

        /// <summary>
        /// Start playing this tween.
        /// </summary>
        public void Play(float delay = 0f, bool useUnscaledTime = false)
        {
            if (_isPlaying)
                return;

            _isPlaying = true;

            if (delay > 0.01f)
                Tweens.StartUpdate(this, delay, OnStart, useUnscaledTime);
            else
            {
                Tweens.StartUpdate(this, useUnscaledTime);
                OnStart();
            }
        }

        /// <summary>
        /// Stops playing the tween.
        /// </summary>
        public void Stop()
        {
            if (!_isPlaying)
                return;
            
            _parent = null;
            _isPlaying = false;
            
            if (Tweens.StopUpdate(this))
                OnStop();
        }

        protected abstract void OnStart();
        protected abstract void OnStop();
    }

    /// <summary>
    /// Base class for all Value Tweens.
    /// </summary>
    public abstract class ValueTween<TweenValueType> : ValueTween where TweenValueType : struct
    {
        private Action<TweenValueType> _onUpdate;
        private Action _onComplete;
        private EaseType _easeType;
        private bool _isPlayingForward;
        private bool _hasPingPong;
        private float _time;
        private float _duration;
        private int _loopCount;

        /// <summary>
        /// The value the driver is currently at.
        /// </summary>
        protected TweenValueType ValueAt;

        /// <summary>
        /// The value the driver should Tween from.
        /// </summary>
        protected TweenValueType ValueFrom;

        /// <summary>
        /// The value the driver should Tween to.
        /// </summary>
        protected TweenValueType ValueTo;


        public TweenValueType FromValue => ValueFrom;
        public TweenValueType CurrentValue => ValueAt;
        public TweenValueType ToValue => ValueTo;

        public override void Update(float deltaTime)
        {
            // Increase or decrease the time of the tween based on the direction.
            float delta = deltaTime / _duration;
            bool didTimeReachEnd = false;

            _time += _isPlayingForward ? delta : -delta;

            // The time will be capped to 1, when ping pong is enabled the tween will
            // play backwards, otherwise when the tween is not infinite, didReachEnd
            // will be set to true.
            if (_time > 1f)
            {
                _time = 1f;

                if (_hasPingPong)
                    _isPlayingForward = false;
                else
                    didTimeReachEnd = true;
            }
            else if (_hasPingPong && _time < 0f)
            {
                _time = 0f;
                _isPlayingForward = true;
                didTimeReachEnd = true;
            }
            
            UpdateValue(_time);

            // When the end is reached either the loop count will be decreased, or
            // the tween will be marked as completed and will be decommissioned, 
            // the onComplete may be invoked.
            if (didTimeReachEnd)
            {
                if (_loopCount > 0)
                {
                    _time = 0f;
                    _loopCount--;
                }
                else
                    Stop();
            }
        }

        /// <summary>
        /// Sets the target value of this tween.
        /// </summary>
        /// <param name="valTo">The target value.</param>
        /// <param name="overwriteFrom">Should the from value be set to the current value.</param>
        /// <returns>The current Tween.</returns>
        public ValueTween<TweenValueType> SetTo(TweenValueType valTo, bool overwriteFrom = true)
        {
            ValueTo = valTo;

            if (overwriteFrom && _time > 0.001f)
                ValueFrom = CurrentValue;

            _time = 0f;
            return this;
        }

        /// <summary>
        /// Sets the value tween should animate from instead of its current.
        /// </summary>
        /// <param name="valFrom">The from value.</param>
        /// <returns>The current Tween.</returns>
        public ValueTween<TweenValueType> SetFrom(TweenValueType valFrom)
        {
            ValueFrom = valFrom;
            ValueAt = valFrom;
            return this;
        }

        /// <summary>
        /// Sets the duration of this Tween.
        /// </summary>
        /// <param name="duration">The duration of the tween.</param>
        /// <returns>The current Tween.</returns>
        public ValueTween<TweenValueType> SetDuration(float duration)
        {
            _duration = Mathf.Max(duration, 0.001f);
            return this;
        }

        /// <summary>
        /// Sets the time of the tween (0-1).
        /// </summary>
        /// <param name="time"></param>
        /// <returns>The current Tween.</returns>
        public ValueTween<TweenValueType> SetTime(float time)
        {
            _time = Mathf.Clamp01(time);
            UpdateValue(_time);
            return this;
        }

        /// <summary>
        /// Sets the loop count of tween until it can be released.
        /// Sets the loop type to ping pong, will bounce the animation back
        /// and forth endlessly. When a loop count is set, the tween has play forward
        /// and backwards to count as one cycle.
        /// </summary>
        /// <returns>The current Tween.</returns>
        public ValueTween<TweenValueType> SetLoop(int loopCount, bool pingPong = false)
        {
            _loopCount = loopCount;
            _hasPingPong = pingPong;
            return this;
        }

        /// <summary>
        /// Sets the ease for tween.
        /// </summary>
        /// <param name="ease">The target ease type.</param>
        /// <returns>The current Tween.</returns>
        public ValueTween<TweenValueType> SetEase(EaseType ease)
        {
            _easeType = ease;
            return this;
        }

        /// <summary>
        /// Binds an onComplete event which will be invoked when the tween ends.
        /// </summary>
        /// <param name="onComplete">The completion callback.</param>
        /// <returns>The current Tween.</returns>
        public ValueTween<TweenValueType> SetCompleteCallback(Action onComplete)
        {
            _onComplete = onComplete;
            return this;
        }

        /// <summary>
        /// Binds an onUpdate event which will be invoked when the tween is updated.
        /// </summary>
        /// <param name="onUpdate">The update callback.</param>
        /// <returns>The current Tween.</returns>
        public ValueTween<TweenValueType> SetUpdateCallback(Action<TweenValueType> onUpdate)
        {
            _onUpdate = onUpdate;
            return this;
        }
        
        /// <summary>
        /// Start playing this tween.
        /// </summary>
        public void PlayAndRelease(Component parent, float delay = 0f, bool useUnscaledTime = false)
        {
            Play(parent, delay, useUnscaledTime);
            _onComplete += () => Tweens.Release(this);
        }
        
        /// <summary>
        /// Gets the total duration of the tween including the loop count and
        /// ping pong settings, and the delay optionally.
        /// </summary>
        /// <returns>The total duration of the Tween.</returns>
        public float GetTotalDuration()
        {
            float duration = _duration;

            if (_loopCount > 0)
                duration *= _loopCount;

            if (_hasPingPong)
                duration *= 2f;

            return duration;
        }

        /// <summary>
        /// Resets the tween so that it can be reused in the future. 
        /// </summary>
        public void Reset()
        {
            _onUpdate = null;
            _loopCount = 0;
            _hasPingPong = false;
            _onComplete = null;
        }
        
        public IEnumerator PlayAndYield(Component parent, float delay = 0f, bool useUnscaledTime = false)
        {
            PlayAndRelease(parent, delay, useUnscaledTime);

            while (IsPlaying)
                yield return null;
        }
        
        public abstract TweenValueType CombineValues(TweenValueType value1, TweenValueType value2);

        /// <summary>
        /// On Update will be invoked every frame and passes the eased time. During
        /// cycle all animation calculations can be performed.
        /// </summary>
        /// <param name="easedTime">The current time of the ease.</param>
        protected abstract void OnUpdate(float easedTime);
        
        protected override void OnStart()
        {
            _time = 0f;
            _isPlayingForward = true;

            // When the tween has no duration, the timing will not be done and the
            // animation will be set to its last frame, the Tween will be 
            // stopped right away.
            if (_duration < 0.001f)
            {
                UpdateValue(1f);
                Stop();
            }
            else
            {
                UpdateValue(0f);
            }
        }

        protected override void OnStop() => _onComplete?.Invoke();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateValue(float t)
        {
            // The time will be updated on the inherited object.
            OnUpdate(Easer.Apply(_easeType, t));
            _onUpdate?.Invoke(CurrentValue);
        }
    }
}
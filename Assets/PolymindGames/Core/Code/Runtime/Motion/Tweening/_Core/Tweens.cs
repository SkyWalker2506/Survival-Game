using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Events;
using UnityEngine.Pool;
using UnityEditor;
using UnityEngine;
using System;

namespace PolymindGames.ProceduralMotion
{
    public static class Tweens
    {
        private static TweenHandler s_SceneTweenHandler;
        private static bool s_HasSceneTweenHandler;
        private static TweenPool s_Pool;


#if UNITY_EDITOR
        [InitializeOnEnterPlayMode]
        private static void OnEnterPlayMode(EnterPlayModeOptions options)
        {
            if (options != EnterPlayModeOptions.None && s_Pool != null)
                s_Pool.ClearPools();
        }
#endif

        private static TweenHandler SceneTweenHandler
        {
            get
            {
                if (!s_HasSceneTweenHandler)
                {
                    var gameObject = new GameObject("SceneTweens")
                    {
                        hideFlags = HideFlags.HideInHierarchy
                    };
                    
                    s_SceneTweenHandler = gameObject.AddComponent<TweenHandler>();
                    s_HasSceneTweenHandler = true;
                }

                return s_SceneTweenHandler;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTween<TweenValueType> Get<TweenValueType>(TweenValueType from, TweenValueType to, float duration, Action<TweenValueType> onUpdate)
            where TweenValueType : struct
        {
#if UNITY_EDITOR
            if (!UnityUtils.IsPlayMode)
                return null;
#endif
            
            s_Pool ??= new TweenPool();
            var tween = s_Pool.GetFromPool<TweenValueType>();
            tween.SetFrom(from)
                .SetTo(to)
                .SetDuration(duration)
                .SetUpdateCallback(onUpdate);

            return tween;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTween<TweenValueType> Get<TweenValueType>(TweenValueType from, TweenValueType to, float duration = 1f)
            where TweenValueType : struct
        {
#if UNITY_EDITOR
            if (!UnityUtils.IsPlayMode)
                return null;
#endif
            
            s_Pool ??= new TweenPool();
            var tween = s_Pool.GetFromPool<TweenValueType>();
            tween.SetFrom(from)
                .SetTo(to)
                .SetDuration(duration);

            return tween;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTween<TweenValueType> Get<TweenValueType>(TweenValueType from)
            where TweenValueType : struct
        {
#if UNITY_EDITOR
            if (!UnityUtils.IsPlayMode)
                return null;
#endif
            
            s_Pool ??= new TweenPool();
            var tween = s_Pool.GetFromPool<TweenValueType>();
            tween.SetFrom(from);
            return tween;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Release<TweenValueType>(ValueTween<TweenValueType> tween) where TweenValueType : struct
        {
#if UNITY_EDITOR
            if (!UnityUtils.IsPlayMode)
                return;
#endif
            
            s_Pool?.ReleaseToPool(tween);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartUpdate(ValueTween tween, bool isUnscaled = false)
        {
#if UNITY_EDITOR
            if (!UnityUtils.IsPlayMode)
                return;
#endif
            
            SceneTweenHandler.AddTween(tween, isUnscaled);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void StartUpdate(ValueTween tween, float delay, UnityAction callback, bool isUnscaled = false)
        {
#if UNITY_EDITOR
            if (!UnityUtils.IsPlayMode)
                return;
#endif
            
            SceneTweenHandler.AddTween(tween, delay, callback, isUnscaled);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool StopUpdate(ValueTween tween)
        {
#if UNITY_EDITOR
            if (!UnityUtils.IsPlayMode)
                return false;
#endif
            
            if (s_HasSceneTweenHandler)
                return s_SceneTweenHandler.RemoveTween(tween);

            return false;
        }

        public static void CancelAllForObject(Component parent)
        {
            if (s_HasSceneTweenHandler) 
                s_SceneTweenHandler.StopAllForParent(parent);
        }

#if UNITY_EDITOR
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void DebugAllActive()
        {
            if (s_HasSceneTweenHandler)
                s_SceneTweenHandler.DebugTweens();
        }
#endif
        
        private sealed class TweenHandler : MonoBehaviour
        {
            private readonly List<ValueTween> _tweens = new(12);
            private readonly List<ValueTween> _unscaledTweens = new(4);
            private readonly List<DelayedTween> _delayedTweens = new(4);


            internal void AddTween(ValueTween tween, bool isUnscaled = false)
            {
                if (isUnscaled)
                    _unscaledTweens.Add(tween);
                else
                    _tweens.Add(tween);
            }
            
            internal void AddTween(ValueTween tween, float delay, UnityAction callback, bool isUnscaled = false)
            {
                var delayedTween = new DelayedTween(tween, callback, delay + (isUnscaled ? Time.unscaledTime : Time.time), isUnscaled);
                _delayedTweens.Add(delayedTween);
            }
            
            internal bool RemoveTween(ValueTween tween)
            {
                // Try to remove the given tween from the active or delayed tween lists.
                if (_tweens.Remove(tween) || _unscaledTweens.Remove(tween))
                    return true;

                for (int i = _delayedTweens.Count - 1; i >= 0; i--)
                {
                    if (_delayedTweens[i].Tween == tween)
                    {
                        _delayedTweens.RemoveAt(i);
                        return true;
                    }
                }

                return false;
            }
            
            internal void StopAllForParent(Component parent)
            {
                for (int i = _tweens.Count - 1; i >= 0; i--)
                {
                    var tween = _tweens[i];
                    if (tween.Parent == parent)
                        tween.Stop();
                }

                for (int i = _delayedTweens.Count - 1; i >= 0; i--)
                {
                    var tween = _delayedTweens[i].Tween;
                    if (tween.Parent == parent)
                        tween.Stop();
                }
            }

    #if UNITY_EDITOR
            internal void DebugTweens()
            {
                foreach (var tween in _tweens)
                {
                    if (tween == null)
                        Debug.Log("NULL Tween");
                    else
                        Debug.Log(tween.GetType(), tween.Parent);
                }
                
                foreach (var tween in _unscaledTweens)
                {
                    if (tween == null)
                        Debug.Log("NULL Tween");
                    else
                        Debug.Log(tween.GetType(), tween.Parent);
                }
            }
    #endif


            private void Update()
            {
                // Update the active value-tweens
                for (int i = _tweens.Count - 1; i >= 0; i--)
                    _tweens[i].Update(Time.deltaTime);

                // Update the active unscaled value-tweens
                for (int i = _unscaledTweens.Count - 1; i >= 0; i--)
                    _unscaledTweens[i].Update(Time.unscaledDeltaTime);

                // Update the delayed value-tweens
                for (int i = _delayedTweens.Count - 1; i >= 0; i--)
                {
                    var delayedTween = _delayedTweens[i];
                    if (delayedTween.Time < (delayedTween.IsUnscaled ? Time.unscaledTime : Time.time))
                    {
                        if (delayedTween.IsUnscaled)
                            _unscaledTweens.Add(delayedTween.Tween);
                        else
                            _tweens.Add(delayedTween.Tween);

                        _delayedTweens.RemoveAt(i);
                        delayedTween.Action?.Invoke();
                    }
                }
            }

            private void OnDestroy()
            {
                _tweens.Clear();
                _delayedTweens.Clear();
                _unscaledTweens.Clear();
                
                s_HasSceneTweenHandler = false;
                s_SceneTweenHandler = null;
            }

            private readonly struct DelayedTween
            {
                public readonly ValueTween Tween;
                public readonly UnityAction Action; 
                public readonly float Time;
                public readonly bool IsUnscaled;

                public DelayedTween(ValueTween tween, UnityAction action, float time, bool isUnscaled)
                {
                    Tween = tween;
                    Action = action;
                    Time = time;
                    IsUnscaled = isUnscaled;
                }
            }
        }

        private sealed class TweenPool
        {
            private readonly Dictionary<Type, object> _pooledTweens = new(5);


            internal TweenPool()
            {
                _pooledTweens.Add(typeof(float),
                    new ObjectPool<ValueTween<float>>(() => new FloatTween(), null, StopAndReset, null, false, 8, 16));

                _pooledTweens.Add(typeof(Vector2),
                    new ObjectPool<ValueTween<Vector2>>(() => new Vector2Tween(), null, StopAndReset, null, false, 8, 16));

                _pooledTweens.Add(typeof(Vector3),
                    new ObjectPool<ValueTween<Vector3>>(() => new Vector3Tween(), null, StopAndReset, null, false, 8, 16));

                _pooledTweens.Add(typeof(Quaternion),
                    new ObjectPool<ValueTween<Quaternion>>(() => new QuaternionTween(), null, StopAndReset, null, false, 8, 16));

                _pooledTweens.Add(typeof(Color),
                   new ObjectPool<ValueTween<Color>>(() => new ColorTween(), null, StopAndReset, null, false, 8, 16));

                static void StopAndReset<T>(ValueTween<T> tween) where T : struct
                {
                    tween.Stop();
                    tween.Reset();
                }
            }

            internal void ClearPools()
            {
                foreach (var pool in _pooledTweens.Values)
                {
                    var disposable = (IDisposable)pool;
                    disposable.Dispose();
                }
            }

            internal ValueTween<TweenValueType> GetFromPool<TweenValueType>() where TweenValueType : struct
            {
                if (_pooledTweens.TryGetValue(typeof(TweenValueType), out object pool))
                {
                    var foundPool = (ObjectPool<ValueTween<TweenValueType>>)pool;
                    return foundPool.Get();
                }

                return null;
            }

            internal void ReleaseToPool<TweenValueType>(ValueTween<TweenValueType> tween) where TweenValueType : struct
            {
                if (_pooledTweens.TryGetValue(typeof(TweenValueType), out object pool))
                {
                    var foundPool = (ObjectPool<ValueTween<TweenValueType>>)pool;
                    foundPool.Release(tween);
                }
                else
                    Debug.LogError("No value tween of type {typeof(T)} found.");
            }
        }
    }
}
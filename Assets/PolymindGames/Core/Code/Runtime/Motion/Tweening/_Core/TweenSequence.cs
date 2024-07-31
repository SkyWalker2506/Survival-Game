using System;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [Serializable]
    public sealed class TweenSequence
    {
        [SerializeField, Clamp(0, int.MaxValue), Title("Loop Settings")]
        [Tooltip("The value of the loop count. When the loop count is used the Tween will keep re-playing until the loop count hits zero. " +
                 "When ping pong is enabled, a play forth and back counts as a single loop.")]
        private int _loopCount;

        [SerializeField]
        [Tooltip("Defines whether the Tween uses ping pong during it's animation. When set to true, the twee will play back and forth. " +
                 "Going once forth and once back counts as one cycle.")]
        private bool _usePingPong;

        [ReferencePicker(TypeGrouping = TypeGrouping.ByFlatName), Label("Tweens")]
        [SerializeReference, ReorderableList(ListStyle.Lined, elementLabel: "Tween", HasHeader = false)]
        private ComponentTween[] _tweens = Array.Empty<ComponentTween>();


        public void Play(bool useUnscaledTime = false)
        {
            for (int i = 0; i < _tweens.Length; i++)
                _tweens[i].Play(useUnscaledTime, _loopCount, _usePingPong);
        }

        public void SetTime(float t)
        {
            for (int i = 0; i < _tweens.Length; i++)
                _tweens[i].SetTime(t);
        }

        public void Cancel()
        {
            for (int i = 0; i < _tweens.Length; i++)
                _tweens[i].Stop();
        }

        public void Validate(GameObject gameObject)
        {
#if UNITY_EDITOR
            if (_tweens != null)
            {
                foreach (var tween in _tweens)
                    tween?.Validate(gameObject);
            }
#endif
        }
    }
}
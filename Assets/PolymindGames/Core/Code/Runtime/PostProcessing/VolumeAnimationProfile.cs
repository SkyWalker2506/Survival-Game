using UnityEngine;
using System;

namespace PolymindGames.PostProcessing
{
    public enum AnimateMode : byte
    {
        PlayOnce,
        PlayOnceAndReverse,
        PlayUntilManualStop
    }

    [CreateAssetMenu(menuName = "Polymind Games/Graphics/Volume Animation Profile", fileName = "Effect_")]
    public sealed class VolumeAnimationProfile : ScriptableObject
    {
        [SerializeField, BeginGroup]
        private AnimateMode _mode = AnimateMode.PlayOnce;

        [SerializeField, Range(0f, 15f)]
        private float _playDuration = 0.5f;

        [SerializeField, Range(0f, 15f), EndGroup]
        private float _cancelDuration = 0.25f;

        [SerializeField, NestedScriptableListInLine(HideSubAssets = true, ListStyle = ListStyle.Boxed)]
        private VolumeAnimation[] _animations = Array.Empty<VolumeAnimation>();
        
        
        public VolumeAnimation[] Animations => _animations;
        public AnimateMode Mode => _mode;
        public float PlayDuration => _playDuration;
        public float CancelDuration => _cancelDuration;
    }
}

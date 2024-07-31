using System.Collections.Generic;
using System.Collections;
using UnityEngine;

#if POLYMIND_GAMES_FPS_HDRP || POLYMIND_GAMES_FPS_URP
using VolProfile = UnityEngine.Rendering.VolumeProfile;
#else
using VolProfile = UnityEngine.Rendering.PostProcessing.PostProcessProfile;
#endif

namespace PolymindGames.PostProcessing
{
    public sealed class PostProcessingManager : Manager<PostProcessingManager>
    {
        private readonly VolumeAnimPlayer[] _animPlayers = new VolumeAnimPlayer[4];
        private readonly List<VolProfile> _profileStack = new(4);


        #region Initialization
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init() => CreateInstance();

        protected override void OnInitialized()
        {
            for (int i = 0; i < _animPlayers.Length; i++)
                _animPlayers[i] = new VolumeAnimPlayer();

#if UNITY_EDITOR
            _profileStack.Clear();
#endif
        }
        #endregion

        public void RegisterGlobalProfile(VolProfile profile)
        {
            if (profile != null && !_profileStack.Contains(profile))
                _profileStack.Add(profile);
        }

        public void UnregisterGlobalProfile(VolProfile profile)
        {
            if (profile != null)
                _profileStack.Remove(profile);
        }

        public VolProfile GetActiveProfile()
        {
            while (_profileStack.Count > 0)
            {
                var profile = _profileStack[^1];
                if (profile != null)
                    return profile;

                _profileStack.RemoveAt(_profileStack.Count - 1);
            }

            return null;
        }

        public void TryPlayAnimation(MonoBehaviour behaviour, VolumeAnimationProfile animProfile, float durationMod = 1f, bool useUnscaledTime = false)
        {
            if (animProfile == null || durationMod < 0.01f)
                return;

            PlayAnimation_Internal(behaviour, animProfile, durationMod, useUnscaledTime);
        }

        public void PlayAnimation(MonoBehaviour behaviour, VolumeAnimationProfile animProfile, float durationMod = 1f, bool useUnscaledTime = false)
        {
#if DEBUG
            if (animProfile == null)
                Debug.LogError($"The ''{nameof(VolumeAnimationProfile)}'' is null.", this);
#endif

            PlayAnimation_Internal(behaviour, animProfile, durationMod, useUnscaledTime);
        }

        public bool CancelAnimation(MonoBehaviour behaviour, VolumeAnimationProfile profile)
        {
            if (TryGetAnimPlayerFor(behaviour, profile, out var animPlayer))
                return animPlayer.Cancel(false);

            return false;
        }

        public void CancelAllAnimations()
        {
            foreach (var animPlayer in _animPlayers)
                animPlayer.Cancel(true);
        }

        private void PlayAnimation_Internal(MonoBehaviour behaviour, VolumeAnimationProfile animProfile, float durationMod = 1f, bool useUnscaledTime = false)
        {
            if (TryGetAnimPlayer(out var animPlayer))
            {
                var profile = GetActiveProfile();

                if (profile == null)
                    Debug.LogWarning("No Global Volume active in the scene.");

                animPlayer.Play(behaviour, animProfile, profile, durationMod, useUnscaledTime);
            }
        }
        
        private bool TryGetAnimPlayerFor(MonoBehaviour behaviour, VolumeAnimationProfile profile, out VolumeAnimPlayer player)
        {
            foreach (var animPlayer in _animPlayers)
            {
                if (!animPlayer.IsActive)
                    continue;

                if (animPlayer.AnimProfile == profile && animPlayer.Behaviour == behaviour)
                {
                    player = animPlayer;
                    return true;
                }
            }

            player = null;
            return false;
        }

        private bool TryGetAnimPlayer(out VolumeAnimPlayer volumeAnimPlayer)
        {
            for (int i = 0; i < _animPlayers.Length; i++)
            {
                var animPlayer = _animPlayers[i];
                if (animPlayer.Behaviour == null)
                {
                    volumeAnimPlayer = animPlayer;
                    return true;
                }
            }

            volumeAnimPlayer = null;
            return false;
        }

        #region Internal
        private sealed class VolumeAnimPlayer
        {
            private enum VolumeAnimState
            {
                Idle,
                Playing,
                Cancelling
            }

            private VolumeAnimState _state = VolumeAnimState.Idle;
            private VolumeAnimationProfile _animProfile;
            private VolProfile _volumeProfile;
            private MonoBehaviour _behaviour;
            private Coroutine _coroutine;


            public MonoBehaviour Behaviour => _behaviour;
            public VolProfile VolumeProfile => _volumeProfile;
            public VolumeAnimationProfile AnimProfile => _animProfile;
            public bool IsActive => _state != VolumeAnimState.Idle;

            public void Play(MonoBehaviour behaviour, VolumeAnimationProfile animProfile, VolProfile volumeProfile, float durationMod, bool useUnscaledTime)
            {
                if (_state != VolumeAnimState.Idle)
                    return;

                _state = VolumeAnimState.Playing;

                _behaviour = behaviour;
                _animProfile = animProfile;
                _volumeProfile = volumeProfile;
                _coroutine = behaviour.StartCoroutine(C_PlayAnimation(animProfile, volumeProfile, durationMod, useUnscaledTime));
            }

            public bool Cancel(bool instant)
            {
                if (_state == VolumeAnimState.Idle || _behaviour == null)
                    return false;

                if (instant)
                {
                    _behaviour.StopCoroutine(_coroutine);
                    _behaviour = null;
                    _coroutine = null;
                    _state = VolumeAnimState.Idle;
                    
                    foreach (var animation in _animProfile.Animations)
                        animation.Dispose(_volumeProfile);
                }
                else
                {
                    _state = VolumeAnimState.Cancelling;
                }

                return true;
            }

            /// <summary>
            /// Coroutine to play animations from a VolumeAnimationProfile
            /// </summary>
            private IEnumerator C_PlayAnimation(VolumeAnimationProfile animProfile, VolProfile profile, float durationMod, bool useUnscaledTime)
            {
                // Get animations from the profile
                var animations = animProfile.Animations;
                int animCount = animations.Length;

                // Set the profile for each animation
                for (int i = 0; i < animCount; i++)
                    animations[i].SetProfile(profile);

                // Initialize time variables
                float t = 0f;
                float duration = animProfile.PlayDuration * durationMod;
                float inverse = 1 / duration;

                // Continue animating while time is less than 1 and state is Playing
                while (t < 1f && _state == VolumeAnimState.Playing)
                {
                    // Animate each animation at time 't'
                    for (int i = 0; i < animCount; i++)
                        animations[i].Animate(t);

                    // Increment time based on frame time and unscaled or scaled time
                    t += inverse * (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
                    yield return null;
                }

                // If time exceeds 1, set time to 1 and finalize animations
                if (t > 1f)
                {
                    t = 1f;
                    for (int i = 0; i < animCount; i++)
                        animations[i].Animate(1f);
                }

                // Continue playing until manual stop or if state is Playing and mode is PlayUntilManualStop
                while (_state == VolumeAnimState.Playing && animProfile.Mode == AnimateMode.PlayUntilManualStop)
                {
#if UNITY_EDITOR
                    // Force animations to their final state if in Unity Editor
                    for (int i = 0; i < animCount; i++)
                        animations[i].Animate(1f);
#endif

                    yield return null;
                }

                // If cancelling or mode is PlayOnceAndReverse, reverse animation
                if (_state == VolumeAnimState.Cancelling || animProfile.Mode == AnimateMode.PlayOnceAndReverse)
                {
                    duration = t * animProfile.CancelDuration * durationMod;
                    inverse = 1 / duration;
                    while (t > 0f)
                    {
                        // Animate each animation at time 't' in reverse
                        for (int i = 0; i < animCount; i++)
                            animations[i].Animate(t);

                        // Decrement time based on frame time and unscaled or scaled time
                        t -= inverse * (useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
                        yield return null;
                    }
                }

                // Dispose animations from the profile
                for (int i = 0; i < animCount; i++)
                    animations[i].Dispose(profile);

                // Reset state and coroutine
                _state = VolumeAnimState.Idle;
                _behaviour = null;
                _coroutine = null;
            }
        }
        #endregion
    }
}
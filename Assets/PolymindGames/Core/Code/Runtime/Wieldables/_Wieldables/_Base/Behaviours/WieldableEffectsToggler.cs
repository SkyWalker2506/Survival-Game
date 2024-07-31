using System;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Behaviours/Wieldable Effects Toggle")]
    public sealed class WieldableEffectsToggler : MonoBehaviour
    {
        [SerializeField, BeginGroup("Toggle Effects")]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        private LightEffect[] _lightEffects;

        [SerializeField, ReorderableList(ListStyle.Boxed, HasLabels = false)]
        private AudioEffect[] _audioEffects;

        [SerializeField, ReorderableList(ListStyle.Boxed, HasLabels = false), EndGroup]
        private ParticleSystem[] _particles;

        [SerializeField, IndentArea, BeginGroup("Enable Effects")]
        private DelayedAudioData _enableAudio = DelayedAudioData.Default;

        [SerializeField, ReorderableList(ListStyle.Lined), EndGroup]
        private AnimatorParameterTrigger[] _enableParameters = Array.Empty<AnimatorParameterTrigger>();

        [SerializeField, IndentArea, BeginGroup("Disable Effects")]
        private DelayedAudioData _disableAudio = DelayedAudioData.Default;

        [SerializeField, ReorderableList(ListStyle.Boxed), EndGroup]
        private AnimatorParameterTrigger[] _disableParameters = Array.Empty<AnimatorParameterTrigger>();

        private IWieldable _wieldable;
        private bool _canEnable = true;
        private bool _enabled;


        public void SetCanEnable(bool value)
        {
            _canEnable = value;
            if (_enabled && !value)
                DisableEffects();
        }

        public void ToggleEffects()
        {
            if (_enabled)
                DisableEffects();
            else
                EnableEffects();
        }

        public void EnableEffects()
        {
            if (_enabled || !_canEnable)
                return;

            for (int i = 0; i < _lightEffects.Length; i++)
                _lightEffects[i].Play();

            for (int i = 0; i < _audioEffects.Length; i++)
                _audioEffects[i].Play();

            for (int i = 0; i < _particles.Length; i++)
                _particles[i].Play(true);

            _wieldable.AudioPlayer.PlayDelayed(_enableAudio);

            var animator = _wieldable.Animation;
            for (int i = 0; i < _enableParameters.Length; i++)
            {
                var parameter = _enableParameters[i];
                animator.SetParameter(parameter.Type, parameter.Hash, parameter.Value);
            }

            _enabled = true;
        }

        public void DisableEffects()
        {
            if (!_enabled)
                return;

            for (int i = 0; i < _lightEffects.Length; i++)
                _lightEffects[i].Stop();

            for (int i = 0; i < _audioEffects.Length; i++)
                _audioEffects[i].Stop();

            for (int i = 0; i < _particles.Length; i++)
                _particles[i].Stop(true, ParticleSystemStopBehavior.StopEmitting);

            _wieldable.AudioPlayer.PlayDelayed(_disableAudio);

            var animator = _wieldable.Animation;
            for (int i = 0; i < _disableParameters.Length; i++)
            {
                var parameter = _disableParameters[i];
                animator.SetParameter(parameter.Type, parameter.Hash, parameter.Value);
            }

            _enabled = false;
        }

        private void Awake()
        {
            _wieldable = GetComponentInParent<IWieldable>();
        }
    }
}
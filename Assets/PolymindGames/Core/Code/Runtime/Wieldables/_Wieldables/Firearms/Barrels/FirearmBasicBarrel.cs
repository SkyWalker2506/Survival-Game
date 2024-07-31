using System;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Muzzle Effects/Basic Barrel")]
    public sealed class FirearmBasicBarrel : FirearmBarrelBehaviour
    {
        [SerializeField, BeginGroup("Audio")]
        private AdvancedAudioData _fireAudio = AdvancedAudioData.Default;

        [SerializeField, EndGroup]
        private AdvancedAudioData _fireTailAudio = AdvancedAudioData.Default;

        [SerializeField, NotNull, BeginGroup("Effects"), SpaceArea]
        private LightEffect _light;

        [SerializeField, SpaceArea]
        private Transform _particlesRoot;

        [SerializeField, DisableIf(nameof(_particlesRoot), false)]
        private Vector3 _particlesAimOffset;

        [EditorButton(nameof(FillReferences)), EndGroup]
        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false)]
        private ParticleSystem[] _particles = Array.Empty<ParticleSystem>();
        
        private float _fireEffectsTimer;
        private Vector3 _originalOffset;

        private const float PLAY_PARTICLES_COOLDOW = 0.1f;


        public override void DoFireEffect()
        {
            Wieldable.AudioPlayer.Play(_fireAudio);
            _light.Play(false);

            if (_particlesRoot != null)
            {
                _particlesRoot.transform.localPosition
                    = Firearm.Sight.IsAiming ? _originalOffset + _particlesAimOffset : _originalOffset;
            }

            if (_fireEffectsTimer < Time.time)
            {
                for (int i = 0; i < _particles.Length; i++)
                    _particles[i].Play(false);

                _fireEffectsTimer = Time.time + PLAY_PARTICLES_COOLDOW;
            }
        }

        public override void DoFireStopEffect()
        {
            if (_fireTailAudio.IsPlayable)
                Wieldable.AudioPlayer.Play(_fireTailAudio);
        }

        protected override void Awake()
        {
            base.Awake();

            if (_particlesRoot != null)
                _originalOffset = _particlesRoot.localPosition;
        }

        [Conditional("UNITY_EDITOR")]
        private void FillReferences()
        {
#if UNITY_EDITOR
            _light = GetComponentInChildren<LightEffect>();
            _particles = GetComponentsInChildren<ParticleSystem>();

            if (_particles != null && _particles.Length > 0 && _particlesRoot == null)
            {
                int maxChildCount = 0;
                foreach (var particle in _particles)
                {
                    var trs = particle.transform;

                    if (trs.childCount > maxChildCount)
                    {
                        _particlesRoot = trs;
                        maxChildCount = trs.childCount;
                    }
                }

                if (_particlesRoot == null)
                    _particlesRoot = _particles[0].transform.parent;
            }

            EditorUtility.SetDirty(this);
#endif
        }
    }
}
using System;
using UnityEngine;

namespace PolymindGames
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/player/modules-and-behaviours/audio#audio-player-module")]
    public sealed class CharacterDamageAudio : CharacterBehaviour
    {
        [SerializeField, Range(0f, 100f), BeginGroup("Settings")]
        private float _damageAmountThreshold = 15f;

        [SerializeField, BeginIndent]
        [ShowIf(nameof(_damageAmountThreshold), 0f, Comparison = UnityComparisonMethod.Greater)]
        [Tooltip("The sounds that will be played when this entity receives damage.")]
        private AdvancedAudioData _damageAudio = AdvancedAudioData.Default;

        [SerializeField, ShowIf(nameof(_damageAmountThreshold), 0f, Comparison = UnityComparisonMethod.Greater)]
        private AudioData _deathAudio = AudioData.Default;

        [SerializeField, EndIndent, EndGroup]
        [ReorderableList(ListStyle.Lined), LabelFromChild(nameof(DamageAudio.Type))]
        private DamageAudio[] _damageTypeAudio;

        [SerializeField, Range(0f, 100f), BeginGroup]
        private float _heartbeatHealthThreshold = 10f;

        [SerializeField, BeginIndent, EndIndent, EndGroup]
        [ShowIf(nameof(_heartbeatHealthThreshold), 0f, Comparison = UnityComparisonMethod.Greater)]
        private AdvancedAudioData _heartbeatAudio = AdvancedAudioData.Default;

        private int _heartbeatLoopId = AudioManager.NULL_LOOP_ID;


        protected override void OnBehaviourEnable(ICharacter character)
        {
            var healthManager = character.HealthManager;
            healthManager.Death += OnDeath;
            healthManager.HealthRestored += OnHealthRestored;
            healthManager.DamageReceived += DamageReceived;
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            var healthManger = character.HealthManager;
            healthManger.Death -= OnDeath;
            healthManger.HealthRestored -= OnHealthRestored;
            healthManger.DamageReceived -= DamageReceived;
        }

        private void OnDeath(in DamageArgs args) => Character.AudioPlayer.Play(_deathAudio, BodyPoint.Torso);

        private void OnHealthRestored(float value)
        {
            // Stop heartbeat loop sound...
            if (_heartbeatLoopId != AudioManager.NULL_LOOP_ID && Character.HealthManager.Health > _heartbeatHealthThreshold)
                Character.AudioPlayer.StopLoop(ref _heartbeatLoopId);
        }

        private void DamageReceived(float damage, in DamageArgs args)
        {
            if (damage > _damageAmountThreshold)
                PlayDamageAudio(args.DamageType);

            // Start heartbeat loop sound...
            if (_heartbeatAudio.IsPlayable && _heartbeatLoopId == AudioManager.NULL_LOOP_ID && Character.HealthManager.Health < _heartbeatHealthThreshold)
                _heartbeatLoopId = Character.AudioPlayer.StartLoop(_heartbeatAudio.Clip, BodyPoint.Torso, _heartbeatAudio.Volume, _heartbeatAudio.Pitch);
        }

        private void PlayDamageAudio(DamageType damageType)
        {
            for (int i = 0; i < _damageTypeAudio.Length; i++)
            {
                if (_damageTypeAudio[i].Type == damageType)
                {
                    Character.AudioPlayer.Play(_damageTypeAudio[i].Audio, BodyPoint.Torso);
                    return;
                }
            }

            Character.AudioPlayer.Play(_damageAudio, BodyPoint.Torso);
        }

        #region Internal
        [Serializable]
        private struct DamageAudio
        {
            public DamageType Type;
            public AdvancedAudioData Audio;
        }
		#endregion
    }
}
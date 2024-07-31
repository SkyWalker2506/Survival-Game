using PolymindGames.PostProcessing;
using PolymindGames.InputSystem;
using UnityEngine.Audio;
using UnityEngine;

namespace PolymindGames
{
    /// <summary>
    /// Deals with the Player death and respawn behaviour.
    /// </summary>
    public class PlayerDeathHandler : CharacterBehaviour
    {
        [SerializeField, NotNull, BeginGroup, EndGroup]
        private InputContext _context;
        
        [SerializeField, BeginGroup, NotNull]
        private VolumeAnimationProfile _deathEffect;

        [SerializeField, NotNull]
        private AudioMixerSnapshot _deathSnapshot;

        [NewLabel("Fade Duration")]
        [SerializeField, IndentArea, Range(0f, 10f), EndGroup]
        private float _audioFadeDuration = 3f;
        

        protected override void OnBehaviourEnable(ICharacter character)
        {
            var health = character.HealthManager;
            health.Respawn += OnPlayerRespawn;
            health.Death += OnPlayerDeath;
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            var health = character.HealthManager;
            health.Respawn -= OnPlayerRespawn;
            health.Death -= OnPlayerDeath;
            
            if (health.IsAlive())
                InputManager.Instance.PopContext(_context);
        }
        
        protected virtual void OnPlayerDeath(in DamageArgs args)
        {
            InputManager.Instance.PushContext(_context);
            _deathSnapshot.TransitionTo(_audioFadeDuration);
            PostProcessingManager.Instance.CancelAllAnimations();
            PostProcessingManager.Instance.PlayAnimation(this, _deathEffect);
        }

        protected virtual void OnPlayerRespawn()
        {
            InputManager.Instance.PopContext(_context);
            AudioManager.Instance.DefaultSnapshot.TransitionTo(1f);
            PostProcessingManager.Instance.CancelAnimation(this, _deathEffect);
        }
    }
}
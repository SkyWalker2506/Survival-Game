using System.Collections;
using UnityEngine;
using TMPro;

namespace PolymindGames.UserInterface
{
    public sealed class DeathUI : CharacterUIBehaviour
    {
        [SerializeField, NotNull, BeginGroup]
        private TextMeshProUGUI _respawnTimeText;

        [SerializeField, NotNull]
        private SelectableUI _respawnButton;

        [SerializeField, Range(0f, 50f), EndGroup]
        private float _enableRespawnDelay = 7f;

        private int _effectId;
        
        private const float MAX_RESPAWN_WAIT = 30f;
        
        
        protected override void OnCharacterAttached(ICharacter character)
        {
            _respawnButton.OnSelected += RespawnPlayer;
            character.HealthManager.Death += OnPlayerDeath;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            _respawnButton.OnSelected -= RespawnPlayer;
            character.HealthManager.Death -= OnPlayerDeath;
        }

        protected override void Awake()
        {
            base.Awake();
            _respawnButton.gameObject.SetActive(false);
            _respawnButton.IsSelectable = false;
            _respawnTimeText.enabled = false;
        }

        private void OnPlayerDeath(in DamageArgs args) => StartCoroutine(C_RespawnRoutine());

        private IEnumerator C_RespawnRoutine()
        {
            _respawnButton.gameObject.SetActive(true);
            _respawnButton.IsSelectable = false;
            _respawnTimeText.enabled = true;

            float endTime = Time.time + _enableRespawnDelay;
            while (endTime >= Time.time)
            {
                float timeLeft = endTime - Time.time;
                _respawnTimeText.text = timeLeft.ToString("0.0");
                yield return null;
            }

            _respawnTimeText.text = "Respawn";
            _respawnButton.IsSelectable = true;

            endTime = Time.time + MAX_RESPAWN_WAIT;
            while (endTime >= Time.time)
            {
                if (_respawnButton.IsSelectable)
                    yield return null;
                else
                    yield break;
            }

            RespawnPlayer();
        }
        
        private void RespawnPlayer()
        {
            var health = Character.HealthManager;
            health.RestoreHealth(health.MaxHealth);

            _respawnButton.gameObject.SetActive(false);
            _respawnButton.IsSelectable = false;
            _respawnTimeText.enabled = false;
        }
    }
}
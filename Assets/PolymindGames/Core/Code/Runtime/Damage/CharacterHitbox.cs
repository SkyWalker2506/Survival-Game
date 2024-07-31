using UnityEngine;

namespace PolymindGames
{
    /// <summary>
    /// Will register damage events from outside and pass them to the parent character.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Polymind Games/Damage/Character Hitbox")]
    public sealed class CharacterHitbox : CharacterBehaviour, IDamageReceiver
    {
        [SerializeField, BeginGroup]
        private bool _isCritical;

        [SerializeField, Range(0f, 100f), EndGroup]
        private float _damageMultiplier = 1f;


        DamageResult IDamageReceiver.ReceiveDamage(float damage)
        {
            Character.HealthManager.ReceiveDamage(damage * _damageMultiplier);
            var result = _isCritical ? DamageResult.Critical : DamageResult.Normal;
            return result;
        }

        DamageResult IDamageReceiver.ReceiveDamage(float damage, in DamageArgs args)
        {
            damage = Character.HealthManager.ReceiveDamage(damage * _damageMultiplier, in args);

            var result = _isCritical ? DamageResult.Critical : DamageResult.Normal;
            DamageTracker.RegisterDamage(this, result, damage, in args);
            return result;
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            col.enabled = true;
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            var col = GetComponent<Collider>();
            col.isTrigger = true;
            col.enabled = false;
        }

#if UNITY_EDITOR
        private void Reset() => gameObject.layer = LayerConstants.HITBOX;
#endif
    }
}
using UnityEngine;

namespace PolymindGames
{
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Polymind Games/Damage/Fake Hitbox")]
    public sealed class FakeHitbox : MonoBehaviour, IDamageReceiver
    {
        [SerializeField, BeginGroup, EndGroup]
        private bool _isCritical;
        
        
        public ICharacter Character => null;

        public DamageResult ReceiveDamage(float damage)
        {
            var result = _isCritical ? DamageResult.Critical : DamageResult.Normal;
            DamageTracker.RegisterDamage(this, result, damage, in DamageArgs.Default);
            return result;
        }

        public DamageResult ReceiveDamage(float damage, in DamageArgs args)
        {
            var result = _isCritical ? DamageResult.Critical : DamageResult.Normal;
            DamageTracker.RegisterDamage(this, result, damage, in args);
            return result;
        }

        private void OnEnable() => GetComponent<Collider>().enabled = true;
        private void OnDisable() => GetComponent<Collider>().enabled = false;

#if UNITY_EDITOR
        private void Reset() => gameObject.layer = LayerConstants.HITBOX;
#endif
    }
}
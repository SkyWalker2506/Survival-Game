using UnityEngine;

namespace PolymindGames
{
    /// <summary>
    /// Registers damage events from outside and passes them to the parent health manager.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Polymind Games/Damage/Simple Hitbox")]
    public sealed class SimpleGenericHitbox : MonoBehaviour, IDamageReceiver
    {
        private IHealthManager _health;
        
        
        public ICharacter Character => null;

        DamageResult IDamageReceiver.ReceiveDamage(float damage)
        {
            damage = _health.ReceiveDamage(damage);
            DamageTracker.RegisterDamage(this, DamageResult.Normal, damage, in DamageArgs.Default);
            return DamageResult.Normal;
        }

        DamageResult IDamageReceiver.ReceiveDamage(float damage, in DamageArgs args)
        {
            damage = _health.ReceiveDamage(damage, in args);
            DamageTracker.RegisterDamage(this, DamageResult.Normal, damage, in args);
            return DamageResult.Normal;
        }

        private void OnEnable() => GetComponent<Collider>().enabled = true;
        private void OnDisable() => GetComponent<Collider>().enabled = false;

        private void Start()
        {
            _health = transform.root.GetComponentInChildren<IHealthManager>();
            if (_health == null)
                Debug.LogError("No Health Manager found as a parent of this HitBox.", gameObject);
        }

#if UNITY_EDITOR
        private void Reset() => gameObject.layer = LayerConstants.HITBOX;
#endif
    }
}
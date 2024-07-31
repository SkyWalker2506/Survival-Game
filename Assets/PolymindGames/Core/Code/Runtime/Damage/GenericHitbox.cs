using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames
{
    /// <summary>
    /// Will register damage events from outside and pass them to a health manager.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("Polymind Games/Damage/Generic Hitbox")]
    public sealed class GenericHitbox : MonoBehaviour, IDamageReceiver
    {
        [SerializeField, Range(0f, 100f), BeginGroup]
        private float _damageMultiplier = 1f;

        [SerializeField]
        private bool _isCritical;

        [SerializeField, EndGroup, SpaceArea]
        [Help("This event will be raised upon taking damage. Any Health Managers attached to this object will also be notified.")]
        private UnityEvent<float> _onDamage;

        private IHealthManager _health;
        
        
        public ICharacter Character => null;

        DamageResult IDamageReceiver.ReceiveDamage(float damage)
        {
            damage *= _damageMultiplier;

            _health.ReceiveDamage(damage);
            _onDamage.Invoke(-(damage * _damageMultiplier));

            DamageResult result = _isCritical ? DamageResult.Critical : DamageResult.Normal;
            DamageTracker.RegisterDamage(this, result, damage, in DamageArgs.Default);
            return result;
        }

        DamageResult IDamageReceiver.ReceiveDamage(float damage, in DamageArgs args)
        {
            damage *= _damageMultiplier;
            damage = _health.ReceiveDamage(damage, in args);
            _onDamage.Invoke(-(damage * _damageMultiplier));

            DamageResult result = _isCritical ? DamageResult.Critical : DamageResult.Normal;
            DamageTracker.RegisterDamage(this, result, damage, in args);
            return result;
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
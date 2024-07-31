using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(HealthManager))]
    [AddComponentMenu("Polymind Games/Damage/Health Manager Events")]
    public sealed class HealthManagerEvents : MonoBehaviour
    {
        [SerializeField, BeginGroup]
        private UnityEvent<float> _onHealthRestored;

        [SerializeField, EndGroup]
        private UnityEvent<float> _onDamage;

        [SerializeField, BeginGroup]
        private UnityEvent _onDeath;

        [SerializeField, EndGroup]
        private UnityEvent _onRespawn;


        private void OnEnable()
        {
            var health = GetComponent<HealthManager>();
            health.DamageReceived += OnDamage;
            health.HealthRestored += _onHealthRestored.Invoke;
            health.Death += OnDeath;
            health.Respawn += _onRespawn.Invoke;
        }

        private void OnDisable()
        {
            var health = GetComponent<HealthManager>();
            health.DamageReceived -= OnDamage;
            health.HealthRestored -= _onHealthRestored.Invoke;
            health.Death -= OnDeath;
            health.Respawn -= _onRespawn.Invoke;
        }

        private void OnDeath(in DamageArgs args) => _onDeath?.Invoke();
        private void OnDamage(float damage, in DamageArgs args) => _onDamage?.Invoke(damage);
    }
}
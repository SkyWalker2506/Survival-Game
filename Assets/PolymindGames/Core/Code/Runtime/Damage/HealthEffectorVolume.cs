using System.Collections.Generic;
using UnityEngine;

namespace PolymindGames
{
    [RequireComponent(typeof(SphereCollider))]
    [AddComponentMenu("Polymind Games/Damage/Health Effector")]
    public sealed class HealthEffectorVolume : MonoBehaviour
    {
        private enum StatInfluenceMode
        {
            IncreaseStat,
            DecreaseStat
        }
        
        [SerializeField, BeginGroup]
        private StatInfluenceMode _influenceMode = StatInfluenceMode.IncreaseStat;

        [SerializeField, Range(0f, 100f), SpaceArea]
        private float _damage = 1f;

        [SerializeField, Range(0f, 100f), EndGroup]
        private float _radius = 1f;

        private List<ICharacter> _charactersInsideTrigger;
        private SphereCollider _influenceVolume;
        private float _totalRadius;

        
        public float RadiusMod { get; set; } = 1f;
        public float RadiationMod { get; set; } = 1f;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out ICharacter character))
            {
                _charactersInsideTrigger ??= new List<ICharacter>();

                if (!_charactersInsideTrigger.Contains(character))
                    _charactersInsideTrigger.Add(character);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            CalculateRadius();

            for (int i = 0; i < _charactersInsideTrigger.Count; i++)
            {
                float distanceToCharacter = Vector3.Distance(transform.position, other.transform.position);
                float radFactor = 1f - distanceToCharacter / _totalRadius;
                float healthChange = RadiationMod * _damage * radFactor * Time.deltaTime;

                if (_influenceMode == StatInfluenceMode.DecreaseStat)
                    _charactersInsideTrigger[i].HealthManager.ReceiveDamage(healthChange);
                else
                    _charactersInsideTrigger[i].HealthManager.RestoreHealth(healthChange);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out ICharacter character))
                _charactersInsideTrigger.Remove(character);
        }

        private void Awake() => _influenceVolume = GetComponent<SphereCollider>();

        private void CalculateRadius()
        {
            _totalRadius = _radius * RadiusMod;
            _influenceVolume.radius = _totalRadius;
        }

        #region Editor
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var prevColor = Gizmos.color;
            Gizmos.color = new Color(1f, 1f, 1f, 0.3f) * Color.green;

            CalculateRadius();
            Gizmos.DrawSphere(transform.position, _totalRadius);

            Gizmos.color = prevColor;
        }

        private void OnValidate()
        {
            if (_influenceVolume == null) _influenceVolume = GetComponent<SphereCollider>();
            _influenceVolume.isTrigger = true;
        }
#endif
        #endregion
    }
}
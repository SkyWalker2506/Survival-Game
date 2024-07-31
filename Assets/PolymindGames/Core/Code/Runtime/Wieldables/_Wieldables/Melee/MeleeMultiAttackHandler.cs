using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(IWieldable))]
    [AddComponentMenu("Polymind Games/Wieldables/Melee/Melee Multi Attack Handler")]
    public class MeleeMultiAttackHandler : MonoBehaviour, IMeleeAttackHandler
    {
        [SerializeField, BeginGroup("Attacks")]
        [Tooltip("The main melee attack behaviour.")]
        private MeleeAttackBehaviour _mainAttack;

        [SerializeField, EndGroup]
        [Tooltip("The alternate melee attack behaviour.")]
        private MeleeAttackBehaviour _altAttack;

        private MeleeAttackBehaviour _lastAttack;
        private float _attackTimer;


        public bool QuickAttack(float accuracy = 1, bool altAttack = false)
        {
            if (!CanAttack())
                return false;

            var attack = altAttack ? _altAttack : _mainAttack;

            attack.Attack(accuracy);
            _attackTimer = Time.time + attack.AttackCooldown;
            _lastAttack = attack;

            return true;
        }

        public bool CanAttack() => Time.time > _attackTimer;

        private void OnDisable()
        {
            if (_lastAttack != null)
                _lastAttack.CancelAttack();
            _attackTimer = 0f;
        }
    }
}
using PolymindGames.InventorySystem;
using UnityEngine;
using System;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(IWieldable))]
    [AddComponentMenu("Polymind Games/Wieldables/Melee/Melee Attack Handler")]
    public sealed class DefaultMeleeAttackHandler : MonoBehaviour, IMeleeAttackHandler
    {
        [SerializeField, Range(0f, 100f), BeginGroup("Settings")]
        [Tooltip("The amount of durability used per hit.")]
        private float _hitDurabilityUsage = 1f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("The amount of stamina used per attack.")]
        private float _attackStaminaUsage = 0.075f;

        [SerializeField, Range(0f, 10f), EndGroup]
        [Tooltip("The delay before the combo reset.")]
        private float _resetComboDelay = 2f;

        [SerializeField, ReorderableList(ListStyle.Boxed, "Combo")]
        [Tooltip("List of attack combos.")]
        private AttackCombo[] _combos = Array.Empty<AttackCombo>();

        private IStaminaManagerCC _staminaManager;
        private MeleeAttackBehaviour _lastAttack;
        private ItemProperty _durability;
        private float _resetComboTimer = float.MaxValue;
        private float _attackTimer;
        private int _lastAttackIndex = -1;
        
        private const float MIN_STAMINA = 0.05f;


        /// <summary>
        /// Tries to do a melee attack using its assigned swings.
        /// </summary>
        /// <returns> "True" if successful </returns>
        public bool QuickAttack(float accuracy, bool altAttack)
        {
            if (!CanAttack())
                return false;

            if (_resetComboTimer < Time.time && _resetComboDelay > 0.01f)
                _lastAttackIndex = -1;

            _resetComboTimer = Time.time + _resetComboDelay;

            // Find and get the best available swing
            int comboIndex = GetAttackWithHighestPriority();
            ref AttackCombo attackCombo = ref _combos[comboIndex];
            var attack = attackCombo.Attacks.Select(ref _lastAttackIndex, attackCombo.SelectionType);

            // Do the attack
            attack.Attack(accuracy, OnHit);
            _attackTimer = Time.time + attack.AttackCooldown;
            _lastAttack = attack;

            // Use stamina
            if (_staminaManager != null)
                _staminaManager.Stamina -= _attackStaminaUsage;

            return true;
        }

        public bool CanAttack()
        {
            if (Time.time < _attackTimer)
                return false;

            return _staminaManager == null || _staminaManager.Stamina > MIN_STAMINA;
        }

        private void OnHit()
        {
            if (_durability != null)
                _durability.Float = Mathf.Max(_durability.Float - _hitDurabilityUsage, 0f);
        }

        private void OnDisable()
        {
            if (_lastAttack != null)
                _lastAttack.CancelAttack();
            _attackTimer = 0f;
        }

        private void Awake()
        {
            var wieldable = GetComponent<IWieldable>();
            if (wieldable.Character != null)
                _staminaManager = wieldable.Character.GetCC<IStaminaManagerCC>();

            if (_combos.Length == 0)
            {
                Debug.LogError("No combos assigned, this component doesn't have any purpose.", gameObject);
                return;
            }

            if (TryGetComponent(out IWieldableItem wieldableItem))
                wieldableItem.AttachedItemChanged += item => _durability = item?.GetPropertyWithId(WieldableItemConstants.DURABILITY);
        }

        private int GetAttackWithHighestPriority()
        {
            int swingComboIndex = 0;

            for (int i = 0; i < _combos.Length; i++)
            {
                var firstSwing = _combos[i].Attacks[0];

                if (firstSwing != null && firstSwing.CanAttack())
                {
                    swingComboIndex = i;
                    break;
                }
            }

            return Mathf.Clamp(swingComboIndex, 0, _combos.Length - 1);
        }

        #region Internal
        [Serializable]
        private struct AttackCombo
        {
            public SelectionType SelectionType;

            [ReorderableList(ListStyle.Lined, HasLabels = false)]
            public MeleeAttackBehaviour[] Attacks;
        }
        #endregion
    }
}
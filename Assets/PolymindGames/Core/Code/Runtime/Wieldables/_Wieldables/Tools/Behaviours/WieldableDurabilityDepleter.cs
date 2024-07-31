using PolymindGames.InventorySystem;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(IWieldable))]
    [AddComponentMenu("Polymind Games/Wieldables/Behaviours/Wieldable Durability Depleter")]
    public sealed class WieldableDurabilityDepleter : WieldableItemBehaviour
    {
        [BeginGroup, EndGroup]
        [SerializeField, Suffix(" / sec")]
        private float _depletion = 1f;

        [SerializeField, BeginGroup("Events")]
        private UnityEvent _onDurabilityDepleted;

        [SerializeField, EndGroup]
        private UnityEvent _onDurabilityRestored;

        private ItemProperty _durability;
        private bool _durabilityDepleted;
        private float _durabilityToDeplete;
        private float _lastDurability;


        protected override void OnItemChanged(IItem item)
        {
            if (item != null)
            {
                _durability = item.GetPropertyWithId(WieldableItemConstants.DURABILITY);
                _durabilityDepleted = _durability != null && _durability.Float < 0.01f;

                if (_durabilityDepleted)
                    _onDurabilityDepleted.Invoke();
                else
                    _onDurabilityRestored.Invoke();

                enabled = true;
            }
            else
                enabled = false;
        }

        protected override void Awake()
        {
            base.Awake();
            enabled = _durability != null;
        }

        private void FixedUpdate()
        {
            _durabilityToDeplete += _depletion * Time.fixedDeltaTime;

            if (_durabilityToDeplete >= _depletion)
            {
                // On Durability increased && previously fully depleted
                if (_durabilityDepleted && _durability.Float > _lastDurability)
                {
                    _durabilityDepleted = false;
                    _onDurabilityRestored.Invoke();
                }

                _durability.Float = Mathf.Max(_durability.Float - _durabilityToDeplete, 0f);

                _lastDurability = _durability.Float;
                _durabilityToDeplete = 0f;

                // On Depleted Durability
                if (!_durabilityDepleted && _lastDurability <= 0.001f)
                {
                    _durabilityDepleted = true;
                    _onDurabilityDepleted.Invoke();
                }
            }
        }
    }
}
using PolymindGames.InventorySystem;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(IWieldable))]
    [AddComponentMenu("Polymind Games/Wieldables/Behaviours/Wieldable Item")]
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_3)]
    public class WieldableItem : MonoBehaviour, IWieldableItem
    {
        [SerializeField, BeginGroup, EndGroup]
        [DataReferenceDetails(HasLabel = true, HasIcon = true, HasAssetReference = true)]
        private DataIdReference<ItemDefinition> _referencedItem;

        private ItemProperty _durabilityProperty;
        private ActionBlockHandler _useBlocker;
        private float _speedMod = 1f;


        public IItem AttachedItem { get; private set; }
        public IWieldable Wieldable { get; private set; }
        public DataIdReference<ItemDefinition> ReferencedItem => _referencedItem;

        public event UnityAction<IItem> AttachedItemChanged;

        public void SetItem(IItem item)
        {
            if (AttachedItem == item)
                return;

            AttachedItem = item;
            AttachedItemChanged?.Invoke(item);

            OnItemChanged(item);
        }

        protected virtual void OnItemChanged(IItem item)
        {
            _speedMod = GetWeightModForItem(item);

            if (_durabilityProperty != null)
            {
                _durabilityProperty.Changed -= OnDurabilityChanged; 
                _durabilityProperty = null; 

                _useBlocker?.RemoveBlocker(this);
                _useBlocker = null;
            }

            if (item != null && Wieldable is IUseInputHandler useInput)
            {
                _useBlocker = useInput.UseBlocker;
                if (item.TryGetPropertyWithId(WieldableItemConstants.DURABILITY, out _durabilityProperty))
                {
                    _durabilityProperty = item.GetPropertyWithId(WieldableItemConstants.DURABILITY);
                    _durabilityProperty.Changed += OnDurabilityChanged;
                    OnDurabilityChanged(_durabilityProperty);
                }
            }
            
            return;

            void OnDurabilityChanged(ItemProperty property)
            {
                if (property.Float < 0.01f)
                    _useBlocker.AddBlocker(this);
                else
                    _useBlocker.RemoveBlocker(this);
            }
        }

        private static float GetWeightModForItem(IItem itm)
        {
            if (itm == null)
                return 1f;

            const float MIN_WEIGHT = 1f;
            const float MAX_WEIGHT = 7.5f;
            const float RANGE_DIFFERENCE = MAX_WEIGHT - MIN_WEIGHT;
            const float MAX_SPEED_PENALTY = 0.175f;

            float percentage = Mathf.Clamp01((itm.Definition.Weight - MIN_WEIGHT) / RANGE_DIFFERENCE);
            return 1f - MAX_SPEED_PENALTY * percentage;
        }

        private void OnDisable() => SetItem(null);

        private void Awake()
        {
            Wieldable = GetComponent<IWieldable>();
            if (Wieldable is IMovementSpeedHandler speedHandler)
                speedHandler.SpeedModifier.AddModifier(() => _speedMod);
        }
    }
}
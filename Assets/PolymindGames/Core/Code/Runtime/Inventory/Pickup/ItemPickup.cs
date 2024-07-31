using PolymindGames.WorldManagement;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    /// <summary>
    /// Basic item pickup.
    /// References one item from the Database.
    /// </summary>
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/interaction/interactable/demo-interactables")]
    public class ItemPickup : MonoBehaviour, ISaveableComponent
    {
        [SerializeField, InLineEditor, BeginGroup, EndGroup]
        [Tooltip("Sound that will be played upon picking the item up.")]
        private AudioDataSO _addAudio;

        [SerializeField, BeginGroup]
        [DataReferenceDetails(HasAssetReference = true, HasIcon = true)]
        private DataIdReference<ItemDefinition> _item = new(0);

        [SerializeField, Range(1, 100)]
#if UNITY_EDITOR
        [ShowIf(nameof(IsItemStackable), true)]
#endif
        private int _minCount = 1;

        [SerializeField, Range(1, 100), EndGroup]
#if UNITY_EDITOR
        [ShowIf(nameof(IsItemStackable), true)]
#endif
        private int _maxCount;
        
        
        public IItem AttachedItem { get; private set; }
        protected IHoverableInteractable Interactable;

        public virtual void LinkWithItem(IItem item)
        {
            AttachedItem = item ?? GetDefaultItem();
            Interactable.Title = AttachedItem.Name;
            Interactable.Description = AttachedItem.Definition.Description;
        }

        private void Awake()
        {
            Interactable = GetComponent<IHoverableInteractable>();
            Interactable.Interacted += OnInteracted;
        }

        protected virtual void Start()
        {
            if (AttachedItem == null && !_item.IsNull)
                LinkWithItem(GetDefaultItem());
        }
        
        protected virtual Item GetDefaultItem()
        {
            int count = _minCount != _maxCount ? Random.Range(_minCount, _maxCount + 1) : _minCount;
            return new Item(_item.Def, count);
        }
        
        protected virtual void OnInteracted(IInteractable interactable, ICharacter character)
        {
            TryPickUpItem(character, AttachedItem);
        }

        protected virtual int TryAddItem(IInventory inventory, IItem item, out string rejectReason)
        {
            return inventory.TryAddItem(item, out rejectReason);
        }

        protected bool TryPickUpItem(ICharacter character, IItem item)
        {
            if (!IsItemValid(item))
                return false;

            int stackCount = item.StackCount;
            int addedCount = TryAddItem(character.Inventory, item, out string rejectReason);

            if (addedCount > 0)
            {
                World.Instance.Message.Dispatch(character, MessageType.Info, FormatPickupMessage(item, addedCount), item.Definition.Icon);
                AudioManager.Instance.PlayClipAtPoint(_addAudio.Clip, transform.position, _addAudio.Volume, _addAudio.Pitch);

                if (addedCount == stackCount)
                    Destroy(gameObject);

                return true;
            }
            
            World.Instance.Message.Dispatch(character, MessageType.Error, FormatRejectReason(rejectReason));
            return false;
        }

        public static string FormatRejectReason(string rejectReason)
        {
            return string.IsNullOrEmpty(rejectReason) ? "Inventory Full" : rejectReason;
        }

        public static string FormatPickupMessage(IItem item, int addedCount)
        {
            return item.Definition.StackSize > 1 ?
                $"Picked Up {item.Name} x {addedCount}" : $"Picked Up {item.Name}";
        }
        
        private static bool IsItemValid(IItem item)
        {
#if DEBUG
            if (item == null)
            {
                Debug.LogError("Item Instance is null, can't pick up anything.");
                return false;
            }
#endif
            return true;
        }

        #region Save & Load
        void ISaveableComponent.LoadMembers(object data) => LinkWithItem((IItem)data);
        object ISaveableComponent.SaveMembers() => AttachedItem;
		#endregion

		#region Editor
#if UNITY_EDITOR
        protected bool IsItemStackable()
        {
            return !_item.IsNull && _item.Def.StackSize > 1;
        }

        protected virtual void OnValidate()
        {
            _maxCount = Mathf.Max(_maxCount, _minCount);
        }
#endif
		#endregion
    }
}
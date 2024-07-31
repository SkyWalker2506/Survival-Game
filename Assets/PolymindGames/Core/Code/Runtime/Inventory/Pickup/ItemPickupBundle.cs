using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    public sealed class ItemPickupBundle : ItemPickup, ISaveableComponent
    {
        [SerializeField, ReorderableList(ListStyle.Boxed), IgnoreParent]
        private ItemGenerator[] _items = new ItemGenerator[1];

        private List<IItem> _attachedItems;


        public override void LinkWithItem(IItem item)
        {
            _attachedItems ??= new List<IItem>();
            _attachedItems.Add(item);
        }

        protected override void OnInteracted(IInteractable interactable, ICharacter character)
        {
            for (int i = 0; i < _attachedItems.Count; i++)
                TryPickUpItem(character, _attachedItems[i]);
        }

        protected override void Start()
        {
            if (_attachedItems == null)
            {
                _attachedItems = new List<IItem>();
                for (int i = 0; i < _items.Length; i++)
                    LinkWithItem(_items[i].GenerateItem());
            }

            var stringBuilder = new StringBuilder(_attachedItems.Count * 10);
            for (int i = 0; i < _attachedItems.Count; i++)
            {
                IItem item = _attachedItems[i];
                stringBuilder.Append($"''{item.Name}'' x {item.StackCount}\n");
            }

            Interactable.Description = stringBuilder.ToString();
        }

		#region Save & Load
        void ISaveableComponent.LoadMembers(object data)
        {
            if (_attachedItems == null)
                _attachedItems = new List<IItem>();
            else
                _attachedItems.Clear();

            _attachedItems.AddRange((IItem[])data);
        }

        object ISaveableComponent.SaveMembers() => _attachedItems.ToArray();
        #endregion
    }
}
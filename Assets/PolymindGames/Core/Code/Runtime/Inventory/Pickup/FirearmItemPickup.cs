using System;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    public sealed class FirearmItemPickup : WieldableItemPickup
    {
        [SerializeField, ReorderableList(ListStyle.Boxed, "Config")]
        private AttachmentItemConfiguration[] _configurations = Array.Empty<AttachmentItemConfiguration>();


        public override void LinkWithItem(IItem item)
        {
            base.LinkWithItem(item);

            foreach (var config in _configurations)
                config.AttachToItem(AttachedItem);
        }

        protected override Item GetDefaultItem()
        {
            var baseItem = base.GetDefaultItem();

            foreach (var config in _configurations)
            {
                if (baseItem.TryGetPropertyWithId(config.Property, out var property))
                    property.ItemId = config.CurrentItem;
            }

            return baseItem;
        }

        #region Editor
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!Application.isPlaying)
            {
                UnityUtils.SafeOnValidate(this, () =>
                {
                    foreach (var config in _configurations)
                        config.AttachToItem(GetDefaultItem());
                });
            }
        }
#endif
        #endregion
        
        #region Internal
        [Serializable]
        private sealed class AttachmentItemConfiguration
        {
            [Tooltip("Attachment Type Property (e.g. Aimer Attachment)")]
            public DataIdReference<ItemPropertyDefinition> Property;

            public DataIdReference<ItemDefinition> CurrentItem;

            [SpaceArea]
            [ReorderableList, LabelByChild("Object")]
            public ItemVisualsPair[] ItemVisuals;


            public void AttachToItem(IItem item)
            {
                if (item.TryGetPropertyWithId(Property, out var property))
                    EnableConfigurationWithID(property.ItemId);
            }

            public void EnableConfigurationWithID(int id)
            {
                for (var i = 0; i < ItemVisuals.Length; i++)
                {
                    bool enable = ItemVisuals[i].Item == id;
                    ItemVisuals[i].Object.SetActive(enable);
                }
            }
        }

        [Serializable]
        private struct ItemVisualsPair
        {
            public DataIdReference<ItemDefinition> Item;
            public GameObject Object;
        }
        #endregion
    }
}
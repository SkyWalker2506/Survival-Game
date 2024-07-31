using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [AddComponentMenu("Polymind Games/User Interface/Slots/Crafting Slot")]
    public class CraftingSlotUI : SlotUI
    {
        [SerializeField, IgnoreParent, BeginGroup("Info")]
        private ItemNameInfo _nameInfo;

        [SerializeField, IgnoreParent]
        private ItemDescriptionInfo _descriptionInfo;

        [SerializeField, IgnoreParent]
        private ItemIconInfo _iconInfo;

        [SerializeField, IgnoreParent, EndGroup]
        private ItemRequirementInfo _requirementInfo;

        private static readonly DummyItem s_DummyItem = new();
        
        
        public ItemDefinition ItemDef { get; private set; }

        public void SetItem(ItemDefinition itemDef)
        {
            s_DummyItem.Definition = itemDef;
            IItem item = itemDef != null ? s_DummyItem : null;

            _nameInfo.UpdateInfo(item);
            _descriptionInfo.UpdateInfo(item);
            _iconInfo.UpdateInfo(item);
            _requirementInfo.UpdateInfo(item);

            ItemDef = itemDef;
        }

        private void Awake() => SetItem(null);
    }
}
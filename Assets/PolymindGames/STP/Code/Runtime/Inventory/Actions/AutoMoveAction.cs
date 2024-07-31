using System.Collections;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Items/Actions/Auto Move Action", fileName = "ItemAction_AutoMove")]
    public sealed class AutoMoveAction : ItemAction
    {
        [SerializeField, BeginGroup("Auto Moving")]
        [Tooltip("All of the item tag(s) that correspond to the wieldables.")]
        private DataIdReference<ItemTagDefinition> _wieldableTag;

        [SerializeField, ReorderableList(HasLabels = false), EndGroup]
        [Tooltip("All of the clothing tag(s) that correspond to clothing items.")]
        private DataIdReference<ItemTagDefinition>[] _clothingTags;

        private IInventoryInspectManagerCC _cachedInspector;
        private IInventory _cachedInventory;


        public override float GetDuration(ItemSlot itemSlot, ICharacter character) => 0f;
        public override float GetDuration(IItem item, ICharacter character) => 0f;

        public override bool IsPerformable(ItemSlot itemSlot, ICharacter character)
        {
            return itemSlot.HasItem && itemSlot.HasContainer;
        }

        public override bool IsPerformable(IItem item, ICharacter character) => false;

        private void Perform(ICharacter character, ItemSlot itemSlot)
        {
            _cachedInventory = character.Inventory;
            _cachedInspector = character.GetCC<IInventoryInspectManagerCC>();

            // Get the necessary references.
            IItemContainer container = itemSlot.Container;
            var tagRestriction = container.GetRestriction<ItemTagRestriction>();
            var validTags = tagRestriction?.Tags;

            // Move item from EXTERNAL container to somewhere else.
            if (IsContainerExternal(container))
            {
                if (TryMoveToStorage(itemSlot)) return;
                if (TryMoveToHolster(itemSlot)) return;
                if (TryMoveToEquipment(itemSlot)) return;
            }

            // Move item from HOLSTER to somewhere else.
            else if (validTags != null && ((IList)validTags).Contains(_wieldableTag))
            {
                if (TryMoveToExternal(itemSlot)) return;
                if (TryMoveToStorage(itemSlot)) return;
            }

            // Move item from EQUIPMENT to somewhere else.
            else if (validTags != null && validTags.ContainsAny(_clothingTags))
            {
                if (TryMoveToExternal(itemSlot)) return;
                if (TryMoveToStorage(itemSlot)) return;
            }

            // Move item from STORAGE to somewhere else.
            else
            {
                if (TryMoveToExternal(itemSlot)) return;
                if (TryMoveToEquipment(itemSlot)) return;
                if (TryMoveToHolster(itemSlot)) return;
            }

            _cachedInventory = null;
            _cachedInspector = null;
        }

        protected override IEnumerator C_PerformAction(ICharacter character, ItemSlot itemSlot, float duration)
        {
            Perform(character, itemSlot);
            yield break;
        }

        protected override IEnumerator C_PerformAction(ICharacter character, IItem item, float duration)
        {
            yield break;
        }

        // Try to move the item from its parent container to a generic storage container.
        private bool TryMoveToStorage(ItemSlot itemSlot)
        {
            foreach (var container in _cachedInventory.GetContainersWithoutTags())
            {
                if (itemSlot.AddOrSwapItemWithContainer(container))
                    return true;
            }

            return false;
        }

        // Try to move the item from its parent container to a container that holds wieldables.
        private bool TryMoveToHolster(ItemSlot itemSlot)
        {
            if (IsWieldable(itemSlot))
            {
                foreach (var container in _cachedInventory.GetContainersWithTag(_wieldableTag))
                {
                    if (itemSlot.AddOrSwapItemWithContainer(container))
                        return true;
                }
            }

            return false;

            bool IsWieldable(ItemSlot slot)
            {
                var itemTag = slot.Item.Definition.Tag;
                return itemTag == _wieldableTag;
            }
        }

        // Try to move the item from its parent container to a container that holds clothes/equipment.
        private bool TryMoveToEquipment(ItemSlot itemSlot)
        {
            if (IsClothing(itemSlot))
            {
                var tag = itemSlot.Item.Definition.Tag;

                foreach (var container in _cachedInventory.GetContainersWithTag(tag))
                {
                    if (itemSlot.AddOrSwapItemWithContainer(container))
                        return true;
                }
            }

            return false;

            bool IsClothing(ItemSlot slot)
            {
                var itemTag = slot.Item.Definition.Tag;
                return ((IList)_clothingTags).Contains(itemTag);
            }
        }

        // Try to move the item from its parent container to an external container (if active).
        private bool TryMoveToExternal(ItemSlot itemSlot)
        {
            return GetExternalContainer() != null && itemSlot.AddOrSwapItemWithContainer(GetExternalContainer());
        }

        private bool IsContainerExternal(IItemContainer container)
        {
            var externalContainer = GetExternalContainer();

            if (externalContainer == null)
                return false;

            return container == externalContainer;
        }

        private IItemContainer GetExternalContainer()
        {
            var workstation = _cachedInspector.Workstation;

            if (workstation != null)
            {
                var containers = _cachedInspector.Workstation.GetContainers();
                return containers.Length > 0 ? containers[0] : null;
            }

            return null;
        }
    }
}
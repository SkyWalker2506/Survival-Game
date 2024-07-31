using PolymindGames.WorldManagement;
using System.Collections;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Items/Actions/Dismantle Action", fileName = "ItemAction_Dismantle")]
    public sealed class DismantleAction : ItemAction
    {
        [BeginGroup("Dismantling")]
        [SerializeField, Suffix("sec"), Clamp(0f, 100f)]
        private float _durationPerGivenItem = 2f;

        [SerializeField]
        private DataIdReference<ItemPropertyDefinition> _durabilityProperty;

        [SerializeField, EndGroup]
        private AudioData _dismantleAudio;

        
        public override float GetDuration(ItemSlot itemSlot, ICharacter character) =>
            GetDuration(itemSlot.Item, character);

        public override float GetDuration(IItem item, ICharacter character)
        {
            if (item.Definition.TryGetDataOfType<CraftingData>(out var craftData))
                return craftData.Blueprint.Length * _durationPerGivenItem;

            return 0f;
        }

        public override bool IsPerformable(ItemSlot itemSlot, ICharacter character)
            => IsPerformable(itemSlot.Item, character);

        public override bool IsPerformable(IItem item, ICharacter character)
        {
            return item != null && item.Definition.TryGetDataOfType<CraftingData>(out var craftData) && craftData.AllowDismantle;
        }

        protected override IEnumerator C_PerformAction(ICharacter character, ItemSlot itemSlot, float duration)
        {
            AudioManager.Instance.PlayClip2D(_dismantleAudio.Clip, _dismantleAudio.Volume);

            for (float timer = Time.time + duration; timer > Time.time;)
                yield return null;

            DismantleItem(character, itemSlot.Item);
        }

        protected override IEnumerator C_PerformAction(ICharacter character, IItem item, float duration)
        {
            AudioManager.Instance.PlayClip2D(_dismantleAudio.Clip, _dismantleAudio.Volume);

            for (float timer = Time.time + duration; timer > Time.time;)
                yield return null;

            DismantleItem(character, item);
        }

        private void DismantleItem(ICharacter character, IItem item)
        {
            // Retrieve crafting data for the item.
            var craftData = item.Definition.GetDataOfType<CraftingData>();

            // Calculate dismantle efficiency based on item durability.
            float durabilityFactor = item.GetPropertyWithId(_durabilityProperty)?.Float / 100f ?? 1f;
            float dismantleEfficiency = craftData.DismantleEfficiency * durabilityFactor;

            // Decrease the stack count of the item.
            item.StackCount--;

            // Dispatch a message about dismantling the item.
            World.Instance.Message.Dispatch(character, MessageType.Error, $"Dismantled {item.Name}", item.Definition.Icon);
            
            // Add blueprint items to the character's inventory.
            foreach (var blueprintItem in craftData.Blueprint)
            {
                // Calculate the amount of each blueprint item to add based on dismantle efficiency.
                int amountToAdd = Mathf.CeilToInt(blueprintItem.Amount * dismantleEfficiency);
        
                // Attempt to add the item to the inventory.
                int addedCount = character.Inventory.AddItems(blueprintItem.Item, amountToAdd);

                // If not all items could be added to the inventory, perform drop action.
                if (addedCount < amountToAdd)
                    character.Inventory.DropItem(new Item(blueprintItem.Item.Def, amountToAdd - addedCount));
                else
                {
                    // Dispatch a message about adding the item to the inventory.
                    string msg = addedCount > 1 ? $"Added {blueprintItem.Item.Name} x {addedCount}" : $"Added {blueprintItem.Item.Name}";
                    World.Instance.Message.Dispatch(character, MessageType.Info, msg, blueprintItem.Item.Def.Icon);
                }
            }
        }
    }
}
using PolymindGames.WorldManagement;
using PolymindGames.UserInterface;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/player/modules-and-behaviours/crafting#crafting-manager-module")]
    public sealed class CraftingManager : CharacterBehaviour, ICraftingManagerCC
    {
        [SerializeField, InLineEditor, BeginGroup, EndGroup]
        [Tooltip("Craft Sound: Sound that will be played after crafting an item.")]
        private AudioDataSO _craftAudio;

        private ItemDefinition _currentItemToCraft;
        
        
        public bool IsCrafting => _currentItemToCraft != null;

        public void Craft(ItemDefinition itemDef)
        {
            if (IsCrafting || itemDef == null)
                return;

            if (itemDef.TryGetDataOfType<CraftingData>(out var craftingData))
            {
                var blueprint = craftingData.Blueprint;
                var inventory = Character.Inventory;

                // Verify if all blueprint crafting materials exist in the inventory
                foreach (var item in blueprint)
                {
                    if (inventory.GetItemsCount(item.Item) < item.Amount)
                        return;
                }

                // Start crafting
                _currentItemToCraft = itemDef;
                var craftingParams = new CustomActionArgs($"Crafting <b>{itemDef.Name}</b>...", craftingData.CraftDuration, true, OnCraftItemEnd, OnCraftCancel);
                CustomActionManagerUI.Instance.StartAction(craftingParams);
                Character.AudioPlayer.PlaySafe(_craftAudio, BodyPoint.Torso);
            }
        }

        public void CancelCrafting()
        {
            if (IsCrafting)
                CustomActionManagerUI.Instance.CancelCurrentAction();
        }

        private void OnCraftItemEnd()
        {
            var craftData = _currentItemToCraft.GetDataOfType<CraftingData>();
            var blueprint = craftData.Blueprint;
            var inventory = Character.Inventory;

            // Verify if all blueprint crafting materials exist in the inventory
            foreach (var item in blueprint)
            {
                if (inventory.GetItemsCount(item.Item) < item.Amount)
                    return;
            }
                
            // Remove the blueprint items from the inventory
            foreach (var item in blueprint)
                inventory.RemoveItems(item.Item, item.Amount);

            // Add the crafted item to the inventory
            int addedCount = inventory.AddItems(_currentItemToCraft.Id, craftData.CraftAmount);

            // If the crafted item couldn't be added to the inventory, spawn the world prefab
            if (addedCount < craftData.CraftAmount)
                Character.Inventory.DropItem(new Item(_currentItemToCraft, craftData.CraftAmount - addedCount));
            else
                World.Instance.Message.Dispatch(Character, MessageType.Info, $"Crafted {_currentItemToCraft.Name}", _currentItemToCraft.Icon);

            _currentItemToCraft = null;
        }

        private void OnCraftCancel() => _currentItemToCraft = null;
    }
}
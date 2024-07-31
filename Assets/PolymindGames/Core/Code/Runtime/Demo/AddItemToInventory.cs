using PolymindGames.InventorySystem;
using PolymindGames.WorldManagement;
using UnityEngine;

namespace PolymindGames.Demo
{
    public sealed class AddItemToInventory : MonoBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        private AudioDataSO _addAudio;
        
        [SerializeField, DataReferenceDetails(HasNullElement = false), ReorderableList(ListStyle.Boxed, HasLabels = false)]
        private DataIdReference<ItemCategoryDefinition>[] _categories;


        public void AddItemToCharacter(ICharacter character)
        {
            var item = CreateItem();

            if (ReferenceEquals(character, Player.LocalPlayer))
            {
                int addedCount = character.Inventory.TryAddItem(item, out string rejectReason);

                if (addedCount > 0)
                    World.Instance.Message.Dispatch(character, MessageType.Info, ItemPickup.FormatPickupMessage(item, addedCount), item.Definition.Icon);
                else
                    World.Instance.Message.Dispatch(character, MessageType.Error, ItemPickup.FormatRejectReason(rejectReason));
            }
            else
            {
                character.Inventory.AddItem(item);
            }
            
            character.AudioPlayer.PlaySafe(_addAudio);
        }

        public void AddItemToCollider(Collider col)
        {
            if (col.TryGetComponent(out ICharacter character))
                AddItemToCharacter(character);
        }

        private IItem CreateItem()
        {
            var category = _categories.SelectRandom().Def;
            return new Item(category.Members.SelectRandom());
        }
    }
}
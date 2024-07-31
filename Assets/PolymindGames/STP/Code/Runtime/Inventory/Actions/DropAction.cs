using System.Collections;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Items/Actions/Drop Action", fileName = "ItemAction_Drop")]
    public sealed class DropAction : ItemAction
    {
        [SerializeField, Range(0f, 5f), BeginGroup("Dropping")]
        private float _dropDelay = 0.35f;


        public override float GetDuration(ItemSlot itemSlot, ICharacter character) => _dropDelay;
        public override float GetDuration(IItem item, ICharacter character) => _dropDelay;

        public override bool IsPerformable(ItemSlot itemSlot, ICharacter character) => itemSlot != null && itemSlot.HasItem;
        public override bool IsPerformable(IItem item, ICharacter character) => item != null;

        protected override IEnumerator C_PerformAction(ICharacter character, ItemSlot itemSlot, float duration)
        {
            for (float timer = Time.time + duration + _dropDelay; timer > Time.time;)
                yield return null;

            character.Inventory.DropItem(itemSlot);
        }

        protected override IEnumerator C_PerformAction(ICharacter character, IItem item, float duration)
        {
            for (float timer = Time.time + duration + _dropDelay; timer > Time.time;)
                yield return null;

            character.Inventory.DropItem(item);
        }
    }
}
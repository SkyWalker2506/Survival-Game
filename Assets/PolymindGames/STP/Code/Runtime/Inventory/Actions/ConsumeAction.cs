using System.Collections;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Items/Actions/Change Vitals Action", fileName = "ItemAction_Consume")]
    public sealed class ConsumeAction : ItemAction
    {
        [SerializeField, Range(0f, 10f), BeginGroup("Consuming")]
        private float _duration;

        [SerializeField, EndGroup]
        private AudioData _consumeAudio;


        public override float GetDuration(ItemSlot itemSlot, ICharacter character) => _duration;
        public override float GetDuration(IItem item, ICharacter character) => _duration;

        /// <summary>
        /// Checks if the given item can be consumed.
        /// </summary>
        public override bool IsPerformable(ItemSlot itemSlot, ICharacter character) => itemSlot.HasItem && itemSlot.Item.Definition.HasDataOfType(typeof(VitalsData));

        public override bool IsPerformable(IItem item, ICharacter character) => item != null && item.Definition.HasDataOfType(typeof(VitalsData));

        protected override IEnumerator C_PerformAction(ICharacter character, ItemSlot itemSlot, float duration)
        {
            AudioManager.Instance.PlayClip2D(_consumeAudio.Clip, _consumeAudio.Volume);

            for (float timer = Time.time + duration; timer > Time.time;)
                yield return null;

            Consume(character, itemSlot.Item);
        }

        protected override IEnumerator C_PerformAction(ICharacter character, IItem item, float duration)
        {
            AudioManager.Instance.PlayClip2D(_consumeAudio.Clip, _consumeAudio.Volume);

            for (float timer = Time.time + duration; timer > Time.time;)
                yield return null;

            Consume(character, item);
        }

        private static void Consume(ICharacter character, IItem item)
        {
            // Try to get the food data from the item definition.
            if (item.Definition.TryGetDataOfType<VitalsData>(out var foodData))
            {
                // Restore or reduce health based on the health change value.
                float healthChange = foodData.HealthChange;
                if (!Mathf.Approximately(healthChange, 0f))
                {
                    if (healthChange > 0f)
                        character.HealthManager.RestoreHealth(healthChange);
                    else
                        character.HealthManager.ReceiveDamage(-healthChange);
                }

                // Change hunger if hunger data is available.
                if (!Mathf.Approximately(foodData.HungerChange, 0f) && character.TryGetCC<IHungerManagerCC>(out var hunger))
                    hunger.Hunger += foodData.HungerChange;

                // Change thirst if thirst data is available.
                if (!Mathf.Approximately(foodData.ThirstChange, 0f) && character.TryGetCC<IThirstManagerCC>(out var thirst))
                    thirst.Thirst += foodData.ThirstChange;

                // Decrease the stack count of the consumed item.
                item.StackCount--;
            }
        }
    }
}
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Items/Item Tag", fileName = "Tag_")]
    public sealed class ItemTagDefinition : DataDefinition<ItemTagDefinition>
    {
        [BeginGroup, EndGroup]
        [SerializeField, NewLabel("Name")]
        [Tooltip("The name of the item tag.")]
        private string _tagName;
        
        public const string UNTAGGED = "Untagged";


        public override string Name
        {
            get => _tagName;
            protected set => _tagName = value;
        }

        public override string FullName => _tagName;
    }
}
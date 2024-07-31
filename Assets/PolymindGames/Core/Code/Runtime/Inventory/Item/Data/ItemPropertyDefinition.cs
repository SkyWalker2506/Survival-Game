using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Items/Item Property", fileName = "Property_")]
    public sealed class ItemPropertyDefinition : DataDefinition<ItemPropertyDefinition>
    {
        [SerializeField, NewLabel("Name"), BeginGroup]
        private string _propertyName;

        [SerializeField, NewLabel("Type")]
        private ItemPropertyType _propertyType;

#if UNITY_EDITOR
        [SerializeField, Multiline(6), EndGroup]
        [Tooltip("Property description, only shown in the editor")]
        private string _description;
#endif
            
        
        public override string Name
        {
            get => _propertyName;
            protected set => _propertyName = value;
        }
        
        public ItemPropertyType Type => _propertyType;
        
#if UNITY_EDITOR
        public override string Description => _description;
#endif
    }
}
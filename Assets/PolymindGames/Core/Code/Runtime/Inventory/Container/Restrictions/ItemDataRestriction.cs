using System;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [Serializable]
    public sealed class ItemDataRestriction : ItemRestriction
    {
        [SerializeField, ClassImplements(typeof(ItemData), AllowAbstract = false)]
        private SerializedType _type;


        public ItemDataRestriction(Type type)
        {
            _type = new SerializedType(type);
        }

        public override int GetAllowedAddAmount(IItem item, int count) => item.Definition.HasDataOfType(_type.Type) ? count : 0;
    }
}
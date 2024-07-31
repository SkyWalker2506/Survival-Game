using System;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [Serializable]
    public sealed class ItemActionRestriction : ItemRestriction
    {
        [SerializeField, ClassImplements(typeof(ItemAction), AllowAbstract = false)]
        private SerializedType _type;


        public ItemActionRestriction(Type type)
        {
            _type = new SerializedType(type);
        }

        public override int GetAllowedAddAmount(IItem item, int count) => item.Definition.HasActionOfType(_type.Type) ? count : 0;
    }
}
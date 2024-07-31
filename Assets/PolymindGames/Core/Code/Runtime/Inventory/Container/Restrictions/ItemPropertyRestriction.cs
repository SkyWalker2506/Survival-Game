using System;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [Serializable]
    public sealed class ItemPropertyRestriction : ItemRestriction
    {
        [SerializeField]
        private DataIdReference<ItemPropertyDefinition>[] _requiredProperties;
        
        
        public DataIdReference<ItemPropertyDefinition>[] RequiredProperties => _requiredProperties;

        public ItemPropertyRestriction(params DataIdReference<ItemPropertyDefinition>[] requiredProperties)
        {
            _requiredProperties = requiredProperties;
        }

        public override int GetAllowedAddAmount(IItem item, int count)
        {
            bool isValid = true;
            var def = item.Definition;

            foreach (var property in _requiredProperties)
                isValid &= def.HasProperty(property);

            if (!isValid)
                return 0;

            return count;
        }
    }
}
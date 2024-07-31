using System;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [Serializable]
    public sealed class ItemCategoryRestriction : ItemRestriction
    {
        [SerializeField]
        private DataIdReference<ItemCategoryDefinition>[] _validCategories;
        
        
        public DataIdReference<ItemCategoryDefinition>[] ValidTags => _validCategories;

        public ItemCategoryRestriction(DataIdReference<ItemCategoryDefinition>[] validCategories)
        {
            _validCategories = validCategories;
        }

        public override int GetAllowedAddAmount(IItem item, int count)
        {
            if (_validCategories == null)
                return count;

            var def = item.Definition;
            bool isValid = false;

            foreach (var category in _validCategories)
                isValid |= def.ParentGroup == category.Def;

            return isValid ? count : 0;
        }
    }
}
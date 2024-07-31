using System;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [Serializable]
    public sealed class ItemTagRestriction : ItemRestriction
    {
        public enum AllowType : byte
        {
            OnlyWithTags,
            OnlyWithoutTags
        }

        [SerializeField]
        private DataIdReference<ItemTagDefinition>[] _tags;

        [SerializeField]
        private AllowType _allowType;

        
        public DataIdReference<ItemTagDefinition>[] Tags => _tags;

        public ItemTagRestriction(AllowType allowType, params DataIdReference<ItemTagDefinition>[] tags)
        {
            _allowType = allowType;
            _tags = tags;
        }

        public override int GetAllowedAddAmount(IItem item, int count)
        {
            if (_tags.Length == 0)
                return count;

            var defTag = item.Definition.Tag;
            switch (_allowType)
            {
                case AllowType.OnlyWithTags:
                    for (var i = 0; i < _tags.Length; i++)
                    {
                        if (defTag == _tags[i])
                            return count;
                    }
                    return 0;
                case AllowType.OnlyWithoutTags:
                    for (var i = 0; i < _tags.Length; i++)
                    {
                        if (defTag == _tags[i])
                            return 0;
                    }
                    return count;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
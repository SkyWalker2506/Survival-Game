using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PolymindGames.InventorySystem
{
    /// <summary>
    /// Generates an item instance based on a few parameters.
    /// </summary>
    [Serializable]
    public struct ItemGenerator : ISerializationCallbackReceiver
    {
        public enum ItemGenerationMethod
        {
            Specific,
            Random,
            RandomFromCategory
        }

        [SerializeField, BeginGroup]
        private ItemGenerationMethod _method;

        [SerializeField, DataReferenceDetails(HasNullElement = false)]
        [ShowIf(nameof(_method), ItemGenerationMethod.Specific)]
        private DataIdReference<ItemDefinition> _item;

        [SerializeField, DataReferenceDetails(HasNullElement = false)]
        [ShowIf(nameof(_method), ItemGenerationMethod.RandomFromCategory)]
        private DataIdReference<ItemCategoryDefinition> _category;

        [SerializeField, Range(1, 100)]
        private int _minCount;

        [SerializeField, Range(1, 100), EndGroup]
        private int _maxCount;


        public readonly IItem GenerateItem()
        {
            switch (_method)
            {
                case ItemGenerationMethod.Specific:
                    return CreateItem(_item.Def);
                case ItemGenerationMethod.RandomFromCategory:
                    {
                        var category = _category.Def;
                        if (category != null)
                        {
                            var itemDef = category.Members.SelectRandom();
                            if (itemDef != null)
                                return CreateItem(itemDef);
                        }
                        break;
                    }
                case ItemGenerationMethod.Random:
                    {
                        var category = ItemCategoryDefinition.Definitions.SelectRandom();

                        if (category != null)
                        {
                            ItemDefinition itemDef = category.Members.SelectRandom();

                            if (itemDef != null)
                                return CreateItem(itemDef);
                        }

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        public ItemDefinition GetItemDefinition()
        {
            switch (_method)
            {
                case ItemGenerationMethod.Specific:
                    return _item.Def;
                case ItemGenerationMethod.RandomFromCategory:
                    {
                        ItemDefinition itemDef = _category.Def.Members.SelectRandom();
                        return itemDef;
                    }
                case ItemGenerationMethod.Random:
                    {
                        var category = ItemCategoryDefinition.Definitions.SelectRandom();

                        if (category != null)
                        {
                            ItemDefinition itemDef = category.Members.SelectRandom();
                            return itemDef;
                        }

                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return null;
        }

        private readonly IItem CreateItem(ItemDefinition itemDef)
        {
            int itemCount = Random.Range(_minCount, _maxCount + 1);

            if (itemCount == 0 || itemDef == null)
                return null;

            return new Item(itemDef, itemCount);
        }

        readonly void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            _minCount = Mathf.Max(_minCount, 1);
            _maxCount = Mathf.Max(_maxCount, 1);
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolymindGames.InventorySystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Items/Item Definition", fileName = "Item_")]
    public sealed class ItemDefinition : GroupMemberDefinition<ItemDefinition, ItemCategoryDefinition>
    {
        [SerializeField, NewLabel("Name"), BeginGroup]
        [Tooltip("Item name.")]
        private string _itemName;

        [SerializeField, SpritePreview, SpaceArea]
        [Tooltip("Item Icon.")]
        private Sprite _icon;

        [SerializeField, NewLabel("Short Description")]
        [Tooltip("Quick, short description to display in the UI.")]
        private string _description;

        [SerializeField, Multiline]
        [Tooltip("Long, detailed description to display in the UI. If left empty the short description will be used instead.")]
        private string _longDescription;

        [SerializeField, Space]
        [Tooltip("Corresponding world pickup prefab for this item. This helps the scripts to know which pickup to spawn when dropping/spawning this item.")]
        private ItemPickup _pickup;

        [SerializeField]
        [Tooltip("Corresponding world stack pickup prefab for this item. Used instead of the normal pickup when the stack count is higher than 1.")]
        private ItemPickup _stackPickup;

        [SerializeField, Range(0.01f, 10f), SpaceArea]
        [Tooltip("The inventory weight of this item in kilograms.")]
        private float _weight = 1f;

        [SerializeField, Range(1, 1000), EndGroup]
        [Tooltip("How many items of this type can be stacked in a single slot.")]
        private int _stackSize = 1;

        [SerializeField, BeginGroup]
        [DataReferenceDetails(NullElementName = ItemTagDefinition.UNTAGGED)]
        private DataIdReference<ItemTagDefinition> _tag;

        [SerializeField, EndGroup]
        private ItemRarityLevel _rarity;

        [SerializeField, BeginGroup, EndGroup]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        [Help("Available actions for this item (the base actions from the parent category are also included)", UnityMessageType.None)]
        private ItemAction[] _actions = Array.Empty<ItemAction>();

        [SerializeField, BeginGroup, EndGroup]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        [Help("Small data that can be changed at runtime (not shared between item instances)", UnityMessageType.None)]
        private ItemPropertyGenerator[] _properties = Array.Empty<ItemPropertyGenerator>();

        [SerializeField, BeginGroup, EndGroup]
        [NestedScriptableListInLine(ListStyle = ListStyle.Lined, HideSubAssets = true)]
        [Help("Data that is shared between all item instances of this type.", UnityMessageType.None)]
        private ItemData[] _data = Array.Empty<ItemData>();
        
        public const string WEIGHT_UNIT = "KG";

        
        /// <summary>
        /// The name of the item definition.
        /// </summary>
        public override string Name
        {
            get => _itemName;
            protected set => _itemName = value;
        }

        /// <summary>
        /// The icon representing the item definition.
        /// </summary>
        public override Sprite Icon => _icon;

        /// <summary>
        /// The description of the item definition.
        /// </summary>
        public override string Description => _description;

        /// <summary>
        /// The color associated with the item definition.
        /// </summary>
        public override Color Color => Rarity.Color;

        /// <summary>
        /// The long description of the item, which may be different from the short description.
        /// </summary>
        public string LongDescription => string.IsNullOrEmpty(_longDescription)
            ? _description
            : _longDescription;

        /// <summary>
        /// The pickup prefab associated with the item.
        /// </summary>
        public ItemPickup Pickup => _pickup;

        /// <summary>
        /// The pickup prefab associated with stacking multiple instances of the item.
        /// </summary>
        public ItemPickup StackPickup => _stackPickup;

        /// <summary>
        /// The maximum stack size for the item definition.
        /// </summary>
        public int StackSize => _stackSize;

        /// <summary>
        /// The weight of the item.
        /// </summary>
        public float Weight => _weight;

        /// <summary>
        /// The tag associated with the item definition.
        /// </summary>
        public DataIdReference<ItemTagDefinition> Tag => _tag;

        /// <summary>
        /// The rarity level of the item definition.
        /// </summary>
        public ItemRarityLevel Rarity
        {
            get
            {
                if (_rarity == null)
                    _rarity = ItemRarityLevel.CommonRarity;

                return _rarity;
            }
        }

        /// <summary>
        /// The actions that can be performed with the item definition.
        /// </summary>
        public ItemAction[] Actions => _actions;

        /// <summary>
        /// Retrieves all additional data associated with the item definition.
        /// </summary>
        public ItemData[] GetAllData() => _data;

        #region Item Tag (Methods)
        public static List<ItemDefinition> GetAllItemsWithTag(ItemTagDefinition tag)
        {
            if (tag == null) return null;
            int tagId = tag.Id;
            if (tagId == 0) return null;

            var items = new List<ItemDefinition>();

            foreach (var item in Definitions)
            {
                if (item._tag == tagId)
                    items.Add(item);
            }

            return items;
        }
        #endregion

        #region Item Data (Methods)
        /// <summary>
        /// Tries to return an item data of type T.
        /// </summary>
        public bool TryGetDataOfType<T>(out T data) where T : ItemData
        {
            var itemData = _data;
            for (int i = 0; i < itemData.Length; i++)
            {
                if (itemData[i] is T)
                {
                    data = (T)itemData[i];
                    return true;
                }
            }

            data = null;
            return false;
        }

        /// <summary>
        /// Returns an item data of the given type (if available).
        /// </summary>
        public T GetDataOfType<T>() where T : ItemData
        {
            var itemData = _data;
            for (int i = 0; i < itemData.Length; i++)
            {
                if (itemData[i] is T)
                    return (T)itemData[i];
            }

            return null;
        }

        /// <summary>
        /// Checks if this item has an item data of type T attached.
        /// </summary>
        public bool HasDataOfType(Type type)
        {
            var itemData = _data;
            for (int i = 0; i < itemData.Length; i++)
            {
                if (itemData[i].GetType() == type)
                    return true;
            }

            return false;
        }
        #endregion

        #region Item Actions (Methods)
        /// <summary>
        /// Tries to return all of the items and item actions of type T.
        /// </summary>
        public static bool GetAllItemsWithAction<T>(out List<ItemDefinition> itemList, out List<T> actionList) where T : ItemAction
        {
            var items = Definitions;
            itemList = new List<ItemDefinition>();
            actionList = new List<T>();

            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].TryGetItemAction<T>(out var action))
                {
                    itemList.Add(items[i]);
                    actionList.Add(action);
                }
            }

            return itemList.Count > 0;
        }

        /// <summary>
        /// Returns all of the item actions present on this item.
        /// </summary>
        public ItemAction[] GetAllActions()
        {
            if (ParentGroup == null || ParentGroup.BaseActions.Length == 0)
                return _actions;

            if (_actions.Length == 0)
                return ParentGroup.BaseActions;

            var parentActions = ParentGroup.BaseActions;
            var actions = new ItemAction[_actions.Length + parentActions.Length];

            for (int i = 0; i < _actions.Length; i++)
                actions[i] = _actions[i];

            int offset = _actions.Length;
            for (int i = 0; i < parentActions.Length; i++)
                actions[i + offset] = parentActions[i];

            return actions;
        }

        /// <summary>
        /// Tries to return an item action of type T.
        /// </summary>
        public bool TryGetItemAction<T>(out T action) where T : ItemAction
        {
            var targetType = typeof(T);

            for (int i = 0; i < _actions.Length; i++)
            {
                if (_actions[i].GetType() == targetType)
                {
                    action = (T)_actions[i];
                    return true;
                }
            }

            action = null;
            return false;
        }

        /// <summary>
        /// Returns an item action of the given type (if available).
        /// </summary>
        public T GetActionOfType<T>() where T : ItemAction
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                if (_actions[i] is T action)
                    return action;
            }

            var parentActions = ParentGroup.BaseActions;
            for (int i = 0; i < parentActions.Length; i++)
            {
                if (parentActions[i] is T action)
                    return action;
            }

            return null;
        }

        /// <summary>
        /// Checks if this item has an item action of type T attached.
        /// </summary>
        public bool HasActionOfType(Type type)
        {
            for (int i = 0; i < _actions.Length; i++)
            {
                if (_actions[i].GetType() == type)
                    return true;
            }

            var parentActions = ParentGroup.BaseActions;
            for (int i = 0; i < parentActions.Length; i++)
            {
                if (parentActions[i].GetType() == type)
                    return true;
            }

            return false;
        }
        #endregion

        #region Item Properties (Methods)
        public static List<ItemDefinition> GetAllItemsWithProperty(ItemPropertyDefinition property)
        {
            if (property == null) return null;
            int propId = property.Id;
            if (propId == 0) return null;

            var items = new List<ItemDefinition>();

            foreach (var item in Definitions)
            {
                if (item.HasProperty(propId))
                    items.Add(item);
            }

            return items;
        }

        public ItemPropertyGenerator[] GetPropertyGenerators() => _properties;

        public bool HasProperty(DataIdReference<ItemPropertyDefinition> property)
        {
            _properties ??= Array.Empty<ItemPropertyGenerator>();
            foreach (var prop in _properties)
            {
                if (prop.Property == property)
                    return true;
            }

            return false;
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        /// <summary>
        /// <para> Warning: This is an editor method, don't call it at runtime.</para> 
        /// Sets the tag of this item (Internal).
        /// </summary>
        public void SetTag(DataIdReference<ItemTagDefinition> tag)
        {
            _tag = tag;
            EditorUtility.SetDirty(this);
        }

        public override void Reset()
        {
            base.Reset();
            _pickup = null;

            var nu = ItemRarityLevel.CommonRarity;
        }

        protected override void OnValidate()
        {
            CollectionExtensions.DistinctPreserveNull(ref _actions);

            for (int i = 0; i < _properties.Length; i++)
            {
                for (int j = 0; j < _properties.Length; j++)
                {
                    if (i == j)
                        continue;

                    if (_properties[i].Property == _properties[j].Property)
                    {
                        _properties[j] = new ItemPropertyGenerator(null);
                        break;
                    }
                }
            }

            if (_rarity == null)
                UnityUtils.SafeOnValidate(this, () => _rarity = ItemRarityLevel.CommonRarity);

            base.OnValidate();
        }
#endif
        #endregion
    }
}
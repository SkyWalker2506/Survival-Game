using System;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Items/Item Category", fileName = "Category_")]
    public sealed class ItemCategoryDefinition : GroupDefinition<ItemCategoryDefinition, ItemDefinition>
    {
#if UNITY_EDITOR
	    [SerializeField, BeginGroup]
		[DataReferenceDetails(NullElementName = ItemTagDefinition.UNTAGGED)]
		private DataIdReference<ItemTagDefinition> _defaultTag;
#endif

		[SerializeField, EndGroup]
		[ReorderableList(ListStyle.Lined, HasLabels = false)]
		private ItemAction[] _baseActions = Array.Empty<ItemAction>();


		public ItemAction[] BaseActions => _baseActions;
		
#if UNITY_EDITOR
		/// <summary>
		/// Warning: This is an editor method, don't call it at runtime.
		/// </summary>
		public override void AddDefaultDataToDefinition(ItemDefinition itemDef)
		{
			if (itemDef == null)
				return;

			if (!_defaultTag.IsNull)
				itemDef.SetTag(_defaultTag);
		}
#endif
	}
}
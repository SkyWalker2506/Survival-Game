using PolymindGames.InventorySystem;
using JetBrains.Annotations;
using UnityEngine;
using System;

namespace PolymindGamesEditor.ToolPages
{
    using UnityObject = UnityEngine.Object;
    
    [UsedImplicitly]
    public sealed class ItemsAndCategoriesToolPage : DataDefinitionPage
    {
        private readonly Lazy<GroupDefinitionPageContent<ItemCategoryDefinition, ItemDefinition>> _content = new(() =>
            new GroupDefinitionPageContent<ItemCategoryDefinition, ItemDefinition>("Categories", "Items"));
        
        public override string DisplayName => "Items & Categories";
        public override bool DisableInPlaymode => true;
        public override int Order => 1;
        
        public override void DrawPage(Rect rect)
        {
            _content.Value.Draw(rect, EditorDrawableLayoutType.Horizontal);
        }

        public override bool IsCompatibleWithObject(UnityObject unityObject)
        {
            if (unityObject is ItemDefinition or ItemCategoryDefinition)
            {
                switch (unityObject)
                {
                    case ItemCategoryDefinition group:
                        _content.Value.GroupsToolbar.SelectDefinition(group);
                        break;
                    case ItemDefinition member:
                        _content.Value.GroupsToolbar.SelectDefinition(member.ParentGroup);
                        _content.Value.MembersToolbar.SelectDefinition(member);
                        break;
                }

                return true;
            }

            return false;
        }
    }
}
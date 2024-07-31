using PolymindGames.InventorySystem;
using JetBrains.Annotations;
using System.Linq;
using UnityEngine;
using System;

namespace PolymindGamesEditor.ToolPages
{
    using UnityObject = UnityEngine.Object;
    
    [UsedImplicitly]
    public sealed class ItemPropertiesAndTagsPage : DataDefinitionPage
    {
        private readonly Lazy<DataDefinitionsContent> _content = new(() => new DataDefinitionsContent(
            new DataDefinitionsContent.Element<ItemPropertyDefinition>("Properties"),
            new DataDefinitionsContent.Element<ItemTagDefinition>("Tags")));
        
        public override string DisplayName => "Properties & Tags";
        public override bool DisableInPlaymode => true;
        public override int Order => 2;

        public override void DrawPage(Rect rect)
        {
            _content.Value.Draw(rect, EditorDrawableLayoutType.Horizontal);
        }

        public override bool IsCompatibleWithObject(UnityObject unityObject)
        {
            if (unityObject is ItemPropertyDefinition or ItemTagDefinition)
            {
                switch (unityObject)
                {
                    case ItemPropertyDefinition property:
                        {
                            var element = _content.Value.Elements.Where(item => item is DataDefinitionsContent.Element<ItemPropertyDefinition>)
                                .Cast<DataDefinitionsContent.Element<ItemPropertyDefinition>>().FirstOrDefault();

                            element?.Toolbar.SelectDefinition(property);
                            break;
                        }
                    case ItemTagDefinition tagDefinition:
                        {
                            var element = _content.Value.Elements.Where(item => item is DataDefinitionsContent.Element<ItemTagDefinition>)
                                .Cast<DataDefinitionsContent.Element<ItemTagDefinition>>().FirstOrDefault();

                            element?.Toolbar.SelectDefinition(tagDefinition);
                            break;
                        }
                }

                return true;
            }

            return false;
        }
    }
}
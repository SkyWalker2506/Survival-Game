using PolymindGames.BuildingSystem;
using JetBrains.Annotations;
using UnityEngine;
using System;

namespace PolymindGamesEditor.ToolPages
{
    using UnityObject = UnityEngine.Object;
    
    [UsedImplicitly]
    public sealed class BuildingPiecesAndCategoriesToolPage : DataDefinitionPage
    {
        private readonly Lazy<GroupDefinitionPageContent<BuildingPieceCategoryDefinition, BuildingPieceDefinition>> _content
            = new(() => new GroupDefinitionPageContent<BuildingPieceCategoryDefinition, BuildingPieceDefinition>("Categories", "Building Pieces"));
        
        public override string DisplayName => "Building Pieces & Categories";
        public override bool DisableInPlaymode => true;
        public override int Order => 5;

        public override void DrawPage(Rect rect)
        {
            _content.Value.Draw(rect, EditorDrawableLayoutType.Horizontal);
        }

        public override bool IsCompatibleWithObject(UnityObject unityObject)
        {
            if (unityObject is BuildingPieceDefinition or BuildingPieceCategoryDefinition)
            {
                switch (unityObject)
                {
                    case BuildingPieceCategoryDefinition group:
                        _content.Value.GroupsToolbar.SelectDefinition(group);
                        break;
                    case BuildingPieceDefinition member:
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
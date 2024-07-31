using PolymindGames.BuildingSystem;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;
using System;

namespace PolymindGamesEditor.BuildingSystem
{
    [CustomEditor(typeof(BuildingPieceDefinition))]
    public sealed class BuildingPieceDefinitionEditor : DataDefinitionEditor<BuildingPieceDefinition>
    {
        private Rect _buildableBtnRect;


        protected override Action<SerializedProperty> GetDrawingAction() => DrawProperty;

        private void DrawProperty(SerializedProperty property)
        {
            switch (property.name)
            {
                case "_icon":
                    DrawIconProperty(property);
                    break;
                case "_prefab":
                    DrawBuildingPiece(property);
                    break;
                default:
                    ToolboxEditorGui.DrawToolboxProperty(property);
                    break;
            }
        }

        private void DrawIconProperty(SerializedProperty property)
        {
            using (new GUILayout.HorizontalScope())
                AssetWizardUtility.FindMatchingAssetButton(property, Definition.Name, "BuildingPiece_");
        }

        private void DrawBuildingPiece(SerializedProperty property)
        {
            using (new GUILayout.HorizontalScope())
            {
                AssetWizardUtility.FindMatchingAssetButton(property, Definition.Name, "BuildingPiece_");
                AssetWizardUtility.ShowWizardButton(ref _buildableBtnRect, property, () => new BuildingPieceCreationWizard(Definition));
            }
        }
    }
}
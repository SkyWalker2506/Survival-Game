using System;
using PolymindGames.InventorySystem;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.InventorySystem
{
    [CustomEditor(typeof(ItemDefinition))]
    public sealed class ItemDefinitionEditor : DataDefinitionEditor<ItemDefinition>
    {
        private Rect _pickupBtnRect;


        protected override Action<SerializedProperty> GetDrawingAction() => DrawProperty;

        private void DrawProperty(SerializedProperty property)
        {
            switch (property.name)
            {
                case "_icon":
                    DrawIconProperty(property);
                    break;
                case "_pickup":
                    DrawPickupProperty(property);
                    break;
                case "_actions":
                    DrawActionsProperty(property);
                    break;
                default:
                    ToolboxEditorGui.DrawToolboxProperty(property);
                    break;
            }
        }

        private void DrawIconProperty(SerializedProperty property)
        {
            using (new GUILayout.HorizontalScope())
                AssetWizardUtility.FindMatchingAssetButton(property, Definition.Name, "Item_");
        }

        private void DrawPickupProperty(SerializedProperty property)
        {
            using (new GUILayout.HorizontalScope())
            {
                AssetWizardUtility.FindMatchingAssetButton(property, Definition.Name, "Pickup_");
                AssetWizardUtility.ShowWizardButton(ref _pickupBtnRect, property, () => new ItemPickupCreationWizard(Definition));
            }
        }

        private void DrawActionsProperty(SerializedProperty property)
        {
            ToolboxEditorGui.DrawToolboxProperty(property);

            if (!Definition.HasParentGroup)
                return;

            using (new GuiLayout.VerticalScope(GuiStyles.Box, separator: false))
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    GUILayout.Label("Actions (Inherited)");

                    using (new GuiLayout.HorizontalScope(GuiStyles.Box))
                    {
                        var actions = Definition.ParentGroup.BaseActions;
                        for (int i = 0; i < actions.Length; i++)
                            EditorGUILayout.ObjectField(actions[i], typeof(ItemAction), false);
                    }
                }
            }

            EditorGUILayout.Space();
        }
    }
}
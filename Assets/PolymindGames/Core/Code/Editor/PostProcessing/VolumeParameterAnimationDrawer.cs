using System;
using JetBrains.Annotations;
using PolymindGames.PostProcessing;
using Toolbox.Editor;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.PostProcessing
{
    [UsedImplicitly]
    public sealed class VolumeParameterAnimationDrawer : ToolboxTargetTypeDrawer
    {
        public override void OnGui(SerializedProperty property, GUIContent label)
        {
            EditorGUILayout.LabelField(label, GuiStyles.BoldMiniGreyLabel);
            ToolboxEditorGui.DrawPropertyChildren(property);
        }

        public override Type GetTargetType() => typeof(VolumeParameterAnimation);
        public override bool UseForChildren() => true;
    }
}
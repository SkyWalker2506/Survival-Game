using System.Collections.Generic;
using JetBrains.Annotations;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System;

namespace PolymindGamesEditor.ToolPages
{
    using UnityObject = UnityEngine.Object;

    public abstract class DataDefinitionPage : ToolPage
    { }

    [UsedImplicitly]
    public sealed class DefinitionsPage : RootPage
    {
        public override string DisplayName => "Definitions";
        public override bool DisableInPlaymode => false;
        public override int Order => 1;


        public override void DrawPage(Rect rect)
        {
            GUILayout.BeginVertical();

            EditorGUILayout.Space();

            DrawDataDefinitionShortcuts();

            GUILayout.FlexibleSpace();

            if (GuiLayout.ColoredButton("Fix All Asset Definitions", GuiStyles.BlueColor, GuiStyles.StandardButton, GUILayout.Height(25f)))
                DataDefinitionEditorUtility.ResetAllAssetDefinitionNamesAndFix();

            GUILayout.EndVertical();
        }

        public override bool IsCompatibleWithObject(UnityObject unityObject) => false;
        
        public override IEnumerable<IEditorToolPage> GetSubPages()
        {
            var definitionTypes = typeof(DataDefinitionPage).Assembly.GetTypes()
                .Where(type => !type.IsAbstract && type.IsSubclassOf(typeof(DataDefinitionPage))).ToArray();

            var subPages = new IEditorToolPage[definitionTypes.Length];
            for (int i = 0; i < subPages.Length; i++)
                subPages[i] = (IEditorToolPage)Activator.CreateInstance(definitionTypes[i]);
            
            return subPages;
        }
        
        private static void DrawDataDefinitionShortcuts()
        {
            // Display the shortcuts
            var shortcutStyle = new GUIStyle(GuiStyles.BoldMiniGreyLabel)
            {
                fontSize = 13
            };

            GUILayout.Label("Shortcuts");
            GuiLayout.Separator(Color.white * 0.4f);

            DrawShortcut("'F5'", "Refresh database.", shortcutStyle);
            DrawShortcut("'Up / Down' Arrows", "Navigate Selection.", shortcutStyle);
            DrawShortcut("'Ctrl + Space'", "Create a new element.", shortcutStyle);
            DrawShortcut("'Ctrl + D'", "Duplicate an element.", shortcutStyle);
            DrawShortcut("'Ctrl + C'", "Copy element.", shortcutStyle);
            DrawShortcut("'Ctrl + V'", "Paste element.", shortcutStyle);
            DrawShortcut("'Del'", "Delete a single element.", shortcutStyle);
            DrawShortcut("'Ctrl + Del'", "Delete all elements.", shortcutStyle);
            DrawShortcut("'Ctrl + U'", "Unlink element.", shortcutStyle);
        }

        private static void DrawShortcut(string shortcut, string label, GUIStyle labelStyle)
        { 
            GUILayout.BeginVertical(GuiStyles.Box);
            GUILayout.Label(shortcut, labelStyle);
            GUILayout.Label(label);
            GUILayout.EndVertical();
        }
    }
}
using PolymindGamesEditor.ToolPages;
using PolymindGames.InventorySystem;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

namespace PolymindGamesEditor.InventorySystem
{
    [CustomEditor(typeof(ItemPropertyDefinition))]
    public sealed class ItemPropertyDefinitionEditor : DataDefinitionEditor<ItemPropertyDefinition>
    {
        private static bool s_ShowFoldout;
        
        private List<ItemDefinition> _itemReferences;
        private Vector2 _scrollView;


        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            EditorGUILayout.Space();

            s_ShowFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(s_ShowFoldout, "References");

            if (s_ShowFoldout)
                DrawReferences();

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        private void DrawReferences()
        {
            var property = (ItemPropertyDefinition)target;
            _itemReferences ??= ItemDefinition.GetAllItemsWithProperty(property);

            var layoutParams = EditorWindow.HasOpenInstances<ToolsWindow>()
                ? new[]
                {
                    GUILayout.Height(100f)
                }
                : Array.Empty<GUILayoutOption>();
            
            using var scroll = new GUILayout.ScrollViewScope(_scrollView, layoutParams);
            _scrollView = scroll.scrollPosition;

            using (new EditorGUI.DisabledScope(true))
            {
                if (_itemReferences.Count == 0)
                    GUILayout.Label("No references found..");

                foreach (var item in _itemReferences)
                    EditorGUILayout.ObjectField(item, typeof(ItemDefinition), false);
            }
        }
    }
}
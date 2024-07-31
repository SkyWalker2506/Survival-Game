using System.Collections.Generic;
using System.Reflection;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public abstract class FoldoutBaseTypeEditor<T> : ObjectEditor where T : Object
    {
        private static bool s_ChildFoldout = true;
        private static bool _baseFoldout;
        
        private readonly Color _boxColor = new(0.95f, 0.95f, 0.95f, 1f);
        private readonly List<SerializedProperty> _ignoredProperties = new();

        private string _baseTypeName;
        
        
        protected override bool IgnoreScriptProperty => true;

        public override void DrawCustomInspector()
        {
            //try to draw the first property (m_Script)
            using (new EditorGUI.DisabledScope(true))
                DrawCustomProperty("m_Script");

            if (IsChildOfBaseType())
            {
                _baseFoldout = EditorGUILayout.Foldout(_baseFoldout, $"Settings ({_baseTypeName})", true, EditorStyles.foldoutHeader);
                EditorGUILayout.BeginVertical(GuiStyles.Box);

                GUI.color = _boxColor;
                if (_baseFoldout)
                {
                    serializedObject.Update();

                    foreach (var prop in _ignoredProperties)
                        ToolboxEditorGui.DrawToolboxProperty(prop);

                    serializedObject.ApplyModifiedProperties();
                }
                GUI.color = Color.white;

                EditorGUILayout.EndVertical();

                if (!_baseFoldout)
                    GUILayout.Space(-8f);

                var targetTypeName = ObjectNames.NicifyVariableName(target.GetType().Name);
                s_ChildFoldout = EditorGUILayout.Foldout(s_ChildFoldout, $"Settings ({targetTypeName})", true, EditorStyles.foldoutHeader);
                EditorGUILayout.BeginVertical(GuiStyles.Box);

                GUI.color = _boxColor;
                if (s_ChildFoldout)
                    DrawChildInspector();

                GUI.color = Color.white;

                EditorGUILayout.EndVertical();

                GUILayout.Space(-8f);
                DrawEndInspector();
            }
            else
                base.DrawCustomInspector();

        }

        protected virtual void OnEnable()
        {
            _baseTypeName = ObjectNames.NicifyVariableName(typeof(T).Name);

            if (IsChildOfBaseType())
            {
                var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (var field in fields)
                {
                    SerializedProperty property = serializedObject.FindProperty(field.Name);

                    if (property != null)
                    {
                        _ignoredProperties.Add(property);
                        IgnoreProperty(property.propertyPath);
                    }
                }
            }
        }

        protected virtual void DrawChildInspector() => base.DrawCustomInspector();
        protected virtual void DrawEndInspector() { }

        private bool IsChildOfBaseType() => target.GetType() != typeof(T);
    }
}
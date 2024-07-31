using JetBrains.Annotations;
using Toolbox.Editor.Internal;
using UnityEditor;
using UnityEngine;

namespace Toolbox.Editor.Drawers
{

    [UsedImplicitly]
    public sealed class DisabledInLineEditorPropertyDrawer : ToolboxSelfPropertyDrawer<DisabledInLineEditorAttribute>
    {
        private static readonly PropertyDataStorage<UnityEditor.Editor, DisabledInLineEditorAttribute> s_Storage;
        

        static DisabledInLineEditorPropertyDrawer()
        {
            s_Storage = new PropertyDataStorage<UnityEditor.Editor, DisabledInLineEditorAttribute>(false,
                (p, a) =>
                {
                    var value = p.objectReferenceValue;
                    if (value != null)
                    {
                        var editor = UnityEditor.Editor.CreateEditor(value);
                        if (editor.HasPreviewGUI())
                        {
                            editor.ReloadPreviewInstances();
                        }

                        if (a.HideScript)
                        {
                            if (editor is ToolboxEditor toolboxEditor)
                            {
                                toolboxEditor.IgnoreProperty("m_Script");
                            }
                        }

                        return editor;
                    }
                    
                    return null;
                },
                Object.DestroyImmediate);
        }
        
        private static UnityEditor.Editor GetTargetsEditor(SerializedProperty property, DisabledInLineEditorAttribute editorAttribute)
        {
            var editor = s_Storage.ReturnItem(property, editorAttribute);
            if (editor.target != property.objectReferenceValue)
                editor = s_Storage.CreateItem(property, editorAttribute);

            return editor;
        }

        private static bool GetInspectorToggle(SerializedProperty property)
        {
            using (new DisabledScope(true))
                return GUILayout.Toggle(property.isExpanded, Style.FoldoutContent, Style.FoldoutStyle, Style.FoldoutOptions);
        }

        private static void DrawEditor(UnityEditor.Editor editor, DisabledInLineEditorAttribute editorAttribute)
        {
            using (new EditorGUILayout.VerticalScope(Style.BackgroundStyle))
            {
                //draw and prewarm the inlined Editor version
                DrawEditor(editor, editorAttribute.DisableEditor, editorAttribute.DrawPreview, editorAttribute.DrawSettings, editorAttribute.PreviewHeight);
            }
        }

        private static void DrawEditor(UnityEditor.Editor editor, bool disableEditor, bool drawPreview, bool drawSettings, float previewHeight)
        {
            using (new EditorGUILayout.VerticalScope())
            {
                //draw the whole inspector and apply changes 
                using (new EditorGUI.DisabledScope(disableEditor))
                {
                    editor.serializedObject.Update();
                    editor.OnInspectorGUI();
                    editor.serializedObject.ApplyModifiedProperties();
                }

                if (!editor.HasPreviewGUI() || !drawPreview)
                {
                    return;
                }

                //draw the preview if possible and needed
                editor.OnPreviewGUI(EditorGUILayout.GetControlRect(false, previewHeight), Style.PreviewStyle);
                if (drawSettings)
                {
                    //draw additional settings associated to the Editor
                    //for example:
                    // - audio management for the AudioClip
                    // - material preview for the Material
                    using (new EditorGUILayout.HorizontalScope(Style.SettingsStyle))
                    {
                        editor.OnPreviewSettings();
                    }
                }
            }
        }


        /// <summary>
        /// Handles the property drawing process and tries to create a inlined version of the <see cref="Editor"/>.
        /// </summary>
        protected override void OnGuiSafe(SerializedProperty property, GUIContent label, DisabledInLineEditorAttribute editorAttribute)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                //create a standard property field for given property
                EditorGUI.BeginChangeCheck();

                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(property, label, property.isExpanded);
                }

                if (EditorGUI.EndChangeCheck())
                {
                    //make sure previously cached Editor is disposed
                    s_Storage.ClearItem(property);
                }

                //NOTE: multiple different Editors are not supported
                if (property.hasMultipleDifferentValues)
                {
                    return;
                }

                var propertyValue = property.objectReferenceValue;
                if (propertyValue == null)
                {
                    return;
                }

                property.isExpanded = GetInspectorToggle(property);
            }

            //create additional Editor for the associated reference 
            if (property.isExpanded)
            {
                var editor = GetTargetsEditor(property, editorAttribute);
                InspectorUtility.SetIsEditorExpanded(editor, true);

                //make usage of the created (cached) Editor instance
                using (new FixedFieldsScope())
                {
                    DrawEditor(editor, editorAttribute);
                }
            }
        }


        public override bool IsPropertyValid(SerializedProperty property)
        {
            return property.propertyType == SerializedPropertyType.ObjectReference;
        }

        private static class Style
        {
            internal static readonly GUIStyle BackgroundStyle;
            internal static readonly GUIStyle FoldoutStyle;
            internal static readonly GUIStyle PreviewStyle;
            internal static readonly GUIStyle SettingsStyle;

            internal static readonly GUIContent FoldoutContent = new("Edit", "Show/Hide Editor");

            internal static readonly GUILayoutOption[] FoldoutOptions = new GUILayoutOption[]
            {
                GUILayout.Width(40.0f)
            };

            static Style()
            {
                BackgroundStyle = new GUIStyle(EditorStyles.helpBox)
                {
                    padding = new RectOffset(13, 13, 8, 8)
                };
                
                FoldoutStyle = new GUIStyle(EditorStyles.miniButton)
                {
#if UNITY_2019_3_OR_NEWER
                    fontSize = 10,
#else
                    fontSize = 9,
#endif
                    alignment = TextAnchor.MiddleCenter
                };

                PreviewStyle = new GUIStyle
                {
                    normal =
                    {
                        background = EditorGuiUtility.CreateColorTexture()
                    }
                };

                SettingsStyle = new GUIStyle()
                {
#if UNITY_2019_3_OR_NEWER
                    padding = new RectOffset(4, 0, 0, 0)
#else
                    padding = new RectOffset(5, 0, 0, 0)
#endif
                };
            }
        }
    }
}
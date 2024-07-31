using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public sealed class CachedObjectEditor
    {
        private readonly GUILayoutOption[] _rectLayoutOptions;
        private Editor _editor;
        private Vector2 _scrollPos;


        public CachedObjectEditor(Object defaultObject, params GUILayoutOption[] options)
        {
            SetObject(defaultObject);
            _rectLayoutOptions = options;
        }

        public Editor Editor => _editor;

        public void SetObject(Object obj)
        {
            if (obj == null)
                _editor = null;
            else
                Editor.CreateCachedEditor(obj, null, ref _editor);
        }

        public void DrawGUI()
        {
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, _rectLayoutOptions);

            if (_editor != null)
                _editor.OnInspectorGUI();

            GUILayout.EndScrollView();
        }

        public void DrawGUI(string labelName, Color bgColor, params GUILayoutOption[] options)
        {
            using (new GuiLayout.VerticalScope(GuiStyles.Box, bgColor, false, false, options))
            {
                GUILayout.Label($"Inspector ({labelName})", GuiStyles.Title, options);
                DrawGUI();
            }
        }

        public void DrawGUIWithToggle(ref bool value, string labelName, Color bgColor, params GUILayoutOption[] options)
        {
            using (new GuiLayout.VerticalScope(GuiStyles.Box, bgColor, false, false, options))
            {
                GUILayout.Space(6f);
                GuiLayout.Separator();

                if (value)
                {
                    GUILayout.Label($"Inspector ({labelName})", GuiStyles.Title, options);

                    using (new GUILayout.VerticalScope(EditorStyles.helpBox))
                        DrawGUI();
                }

                string btnName = value ? "Hide Inspector" : "Show Inspector";
                if (GuiLayout.ColoredButton(btnName, GuiStyles.YellowColor))
                    value = !value;
            }
        }
    }
}
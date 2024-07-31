using System;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public sealed class DataDefinitionActionWindow : EditorWindow
    {
        private static DataDefinitionActionWindow s_Window;
        private static EditorWindow s_LastFocusedWindow;
        private string _definitionTypeName;
        private string _definitionName;
        
        private Action<string> _createDefinition;
        private string _actionName;


        public static void OpenWindow(Action<string> action, string typeName, string actionName)
        {
            if (s_Window != null)
                s_Window.Close();

            s_LastFocusedWindow = focusedWindow;

            typeName = ObjectNames.NicifyVariableName(typeName.Replace("Definition", ""));
            s_Window = GetWindow<DataDefinitionActionWindow>(utility: true, title: $"{actionName} {typeName}",
                focus: true);

            s_Window.maxSize = new Vector2(512, 84);
            s_Window.minSize = new Vector2(512, 84);

            s_Window._createDefinition = action;
            s_Window._definitionTypeName = typeName;
            s_Window._actionName = actionName;
        }

        private void OnGUI()
        {
            if (Event.current.keyCode == KeyCode.Escape)
            {
                CloseWindow();
                return;
            }

            GUILayout.FlexibleSpace();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                GUILayout.Label(_definitionTypeName);

                GUI.SetNextControlName("DefText");
                _definitionName = EditorGUILayout.TextField(_definitionName);

                GUILayout.FlexibleSpace();
            }

            GUILayout.FlexibleSpace();

            EditorGUI.FocusTextInControl("DefText");

            if (Event.current.keyCode == KeyCode.Return || GUILayout.Button($"{_actionName} {_definitionTypeName}",
                    GuiStyles.LargeButton))
            {
                _createDefinition?.Invoke(_definitionName);
                CloseWindow();
            }
        }

        private static void CloseWindow()
        {
            s_Window.Close();

            if (s_LastFocusedWindow != null)
                s_LastFocusedWindow.Focus();
        }
    }

    public sealed class DataDefinitionAction
    {
        private readonly Action _click;
        private readonly Color _color;
        private readonly Func<bool> _enabled;
        private readonly GUIContent _guiContent;
        private readonly KeyCode _shortcutKey;


        public DataDefinitionAction(Action click, Func<bool> enabled, GUIContent content,
            KeyCode shortcutKey, Color color = default(Color))
        {
            _click = click;
            _enabled = enabled;
            _guiContent = content;
            _shortcutKey = shortcutKey;
            _color = color;
        }

        public void DrawGUI(GUILayoutOption[] buttonLayout = null)
        {
            using (new EditorGUI.DisabledScope(!_enabled()))
            {
                if (GuiLayout.ColoredButton(_guiContent, _color, buttonLayout))
                    _click?.Invoke();
            }
        }

        public void HandleEvent(Event current)
        {
            if (current.control && current.keyCode == _shortcutKey)
                _click?.Invoke();
        }
    }
}
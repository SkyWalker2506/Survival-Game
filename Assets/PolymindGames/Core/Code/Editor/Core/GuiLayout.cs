using System;
using UnityEngine;

namespace PolymindGamesEditor
{
    public static class GuiLayout
    {
        #region GUI Scopes
        public readonly struct FgColorScope : IDisposable
        {
            private readonly Color _prevColor;

            public FgColorScope(Color color)
            {
                _prevColor = GUI.color;
                GUI.color = color;
            }

            public void Dispose() => GUI.color = _prevColor;
        }

        public readonly struct BgColorScope : IDisposable
        {
            private readonly Color _prevColor;

            public BgColorScope(Color color)
            {
                _prevColor = GUI.backgroundColor;
                GUI.backgroundColor = color;
            }

            public void Dispose() => GUI.backgroundColor = _prevColor;
        }

        public readonly struct VerticalScope : IDisposable
        {
            private readonly bool _separator;
            private readonly bool _space;
            
            private const float SPACE = 3f;
            
            public VerticalScope(GUIStyle style, Color color = default(Color), bool space = true, bool separator = true, params GUILayoutOption[] guiLayoutOptions)
            {
                if (color != default(Color))
                    GUI.color = color;

                GUILayout.BeginVertical(style, guiLayoutOptions);
                GUI.color = Color.white;

                _separator = separator;
                _space = space;

                if (space)
                    GUILayout.Space(SPACE);
            }

            public void Dispose()
            {
                if (_space)
                    GUILayout.Space(3f);

                GUILayout.EndVertical();

                if (_separator)
                    Separator(SPACE);
            }
        }

        public readonly struct HorizontalScope : IDisposable
        {
            public HorizontalScope(GUIStyle style, Color color = default(Color), float space = 3f, params GUILayoutOption[] guiLayoutOptions)
            {
                if (color != default(Color))
                    GUI.color = color;

                GUILayout.BeginHorizontal(style, guiLayoutOptions);
                GUI.color = Color.white;

                GUILayout.Space(space);
            }

            public void Dispose()
            {
                GUILayout.Space(3f);
                GUILayout.EndHorizontal();
            }
        }
        #endregion

        public static bool ColoredButton(string label, Color color, params GUILayoutOption[] options)
        {
            GUI.backgroundColor = color;
            bool pressedButton = GUILayout.Button(label, options);
            GUI.backgroundColor = Color.white;

            return pressedButton;
        }

        public static bool ColoredButton(string label, Color color, GUIStyle style, params GUILayoutOption[] options)
        {
            GUI.backgroundColor = color;
            bool pressedButton = GUILayout.Button(label, style, options);
            GUI.backgroundColor = Color.white;

            return pressedButton;
        }

        public static bool ColoredButton(GUIContent content, Color color, params GUILayoutOption[] options)
        {
            GUI.backgroundColor = color;
            bool pressedButton = GUILayout.Button(content, options);
            GUI.backgroundColor = Color.white;

            return pressedButton;
        }

        /// <summary>
        /// Make an on/off toggle button.
        /// </summary>
        public static bool ColoredToggle(ref bool value, string label, Color color, params GUILayoutOption[] options)
        {
            using (new BgColorScope(color))
            {
                bool prevVal = value;
                value = GUILayout.Toggle(value, label, GuiStyles.StandardButton, options);
                return prevVal != value;
            }
        }

        public static void Separator(Color color, float thickness = 1)
        {
            Rect position = GUILayoutUtility.GetRect(GUIContent.none, GuiStyles.Splitter, GUILayout.Height(thickness));

            if (Event.current.type == EventType.Repaint)
            {
                Color restoreColor = GUI.color;
                GUI.color = color;
                GuiStyles.Splitter.Draw(position, false, false, false, false);
                GUI.color = restoreColor;
            }
        }

        public static void Separator(float thickness = 1f)
        {
            Rect position = GUILayoutUtility.GetRect(GUIContent.none, GuiStyles.Splitter, GUILayout.Height(thickness));

            if (Event.current.type == EventType.Repaint)
            {
                using (new FgColorScope(GuiStyles.SplitterColor))
                    GuiStyles.Splitter.Draw(position, false, false, false, false);
            }
        }
    }
}
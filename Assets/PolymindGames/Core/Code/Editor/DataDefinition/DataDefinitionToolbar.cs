using PolymindGames;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public sealed class DataDefinitionToolbar<T> : SearchableDataDefinitionList<T> where T : DataDefinition<T>
    {
        private GUIContent[] _guiContents;
        private Vector2 _scrollPos;
        
        
        public float ButtonHeight { get; set; } = 40f;
        public float IconSize { get; set; } = 1f;

        #region Initialization
        public DataDefinitionToolbar(string listName, params DataDefinitionAction[] customActions) : base(listName, customActions) { }

        protected override void SetDefinitions(T[] scriptables)
        {
            base.SetDefinitions(scriptables);

            _guiContents = new GUIContent[Definitions.Count];
            for (int i = 0; i < Definitions.Count; i++)
            {
                _guiContents[i] = new GUIContent
                {
                    text = Definitions[i].Name,
                    tooltip = Definitions[i].Description,
                    image = Definitions[i].Icon != null ? AssetPreview.GetAssetPreview(Definitions[i].Icon) : null
                };
            }
        }
        #endregion

        #region Layout Drawing
        protected override void DoLayout()
        {
            base.DoLayout();

            using (var scrollView = new GUILayout.ScrollViewScope(_scrollPos, false, false))
            {
                _scrollPos = scrollView.scrollPosition;
                bool isFocused = false;

                using (new GUILayout.VerticalScope(GuiStyles.Box))
                {
                    DrawSearchBar();
                    isFocused |= HasMouseFocus();

                    DrawScriptableToolbar();
                    isFocused |= HasMouseFocus();
                }

                if (isFocused)
                    FocusList(this);
                else if (Event.current.type == EventType.Repaint)
                    RemoveFocus();

                GUILayout.FlexibleSpace();
            }

            DrawListEditingGUI();
        }

        private static bool HasMouseFocus()
        {
            return Event.current.type == EventType.Repaint && GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
        }

        private void DrawScriptableToolbar()
        {
            if (Count == 0)
                return;

            float iconSize = ButtonHeight * IconSize;
            using (new EditorGUIUtility.IconSizeScope(new Vector2(iconSize, iconSize)))
            {
                using (new GuiLayout.FgColorScope(DataDefinitionToolbarType.ButtonColor))
                {
                    int prevIndex = SelectedIndex;
                    int newIndex = GUILayout.SelectionGrid(prevIndex, _guiContents, 1, DataDefinitionToolbarType.ButtonStyle,
                        GUILayout.Height(ButtonHeight * Definitions.Count));

                    if (prevIndex != newIndex)
                    {
                        GUI.FocusControl(null);
                        SelectIndex(newIndex);
                    }
                }
            }
        }
        #endregion
    }

    internal static class DataDefinitionToolbarType
    {
        public static readonly GUIStyle ButtonStyle;
        public static readonly Color ButtonColor = Color.white;

        static DataDefinitionToolbarType()
        {
            ButtonStyle = new GUIStyle();
            ButtonStyle = GuiStyles.ColoredButton;
            ButtonStyle.alignment = TextAnchor.MiddleCenter;
        }
    }
}
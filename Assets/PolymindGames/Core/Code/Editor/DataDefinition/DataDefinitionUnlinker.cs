using System;
using System.Collections.Generic;
using PolymindGames;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public sealed class DataDefinitionUnlinker<Group, Member> where Group : GroupDefinition<Group, Member> where Member : GroupMemberDefinition<Member, Group>
    {
        public bool IsExpanded;
        
        private readonly GUILayoutOption[] _rectLayoutOptions;
        private readonly List<Member> _unlinkedDefinitions = new();
        
        private GUIContent[] _guiContents;
        private Vector2 _scrollPos;
        private int _selectedIndex;


        public DataDefinitionUnlinker(params GUILayoutOption[] options)
        {
            _rectLayoutOptions = options;
        }

        public bool HasUnlinkedDefinitions => _unlinkedDefinitions.Count > 0;

        public event Action<Member> DefinitionAdded;

        public void DoLayoutWithToggle(string label, params GUILayoutOption[] options)
        {
            GuiLayout.Separator();

            if (IsExpanded)
            {
                GUILayout.Label($"({label})", GuiStyles.Title, options);

                using (new GUILayout.VerticalScope())
                    DoLayout();
            }

            string btnName = IsExpanded ? $"Hide {label}" : $"Show {label}";
            if (GuiLayout.ColoredButton(btnName, GuiStyles.YellowColor))
                IsExpanded = !IsExpanded;
        }

        public void DoLayout()
        {
            using (new GUILayout.VerticalScope(_rectLayoutOptions))
            {
                using (new EditorGUIUtility.IconSizeScope(new Vector2(32f, 32f)))
                {
                    using (var scroll = new GUILayout.ScrollViewScope(_scrollPos))
                    {
                        _scrollPos = scroll.scrollPosition;

                        _selectedIndex = GUILayout.SelectionGrid(_selectedIndex, _guiContents, 1, GuiStyles.ColoredButton, GUILayout.Height(30 * _unlinkedDefinitions.Count));
                        _selectedIndex = Mathf.Clamp(_selectedIndex, 0, _unlinkedDefinitions.Count - 1);
                    }
                }

                GUILayout.FlexibleSpace();

                if (_unlinkedDefinitions.Count > 0)
                {
                    GUILayout.Space(10f);

                    GUILayout.FlexibleSpace();

                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GuiLayout.ColoredButton(DataDefinitionEditorStyles.LinkContent, GuiStyles.BlueColor, GUILayout.Height(30f)))
                            DefinitionAdded?.Invoke(_unlinkedDefinitions[_selectedIndex]);
                        GUILayout.FlexibleSpace();
                    }

                    GUILayout.Space(3f);
                }
            }
        }

        public void Refresh()
        {
            _unlinkedDefinitions.Clear();
            foreach (var def in GroupMemberDefinition<Member, Group>.Definitions)
            {
                if (!def.HasParentGroup)
                    _unlinkedDefinitions.Add(def);
            }

            _guiContents = new GUIContent[_unlinkedDefinitions.Count];
            for (int i = 0; i < _unlinkedDefinitions.Count; i++)
            {
                _guiContents[i] = new GUIContent
                {
                    text = _unlinkedDefinitions[i].Name,
                    tooltip = _unlinkedDefinitions[i].Name,
                    image = _unlinkedDefinitions[i].Icon != null ? AssetPreview.GetAssetPreview(_unlinkedDefinitions[i].Icon) : null
                };
            }
        }
    }
}
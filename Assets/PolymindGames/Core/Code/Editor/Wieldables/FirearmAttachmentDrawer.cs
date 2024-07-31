using System;
using PolymindGames.WieldableSystem;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.WieldableSystem
{
    public abstract class FirearmAttachmentDrawer
    {
        public abstract void Draw();
    }

    public sealed class FirearmAttachmentDrawer<T> : FirearmAttachmentDrawer where T : FirearmAttachmentBehaviour
    {
        private readonly string[] _partContents;
        private readonly string _partName;
        private readonly T[] _parts;
        private int _selectedIndex;
        private bool _foldout;

        private const float BUTTON_HEIGHT = 22f;
        

        public FirearmAttachmentDrawer(Firearm firearm, string partName)
        {
            _parts = firearm.gameObject.GetComponentsInChildren<T>(true) ?? Array.Empty<T>();
            _partName = ObjectNames.NicifyVariableName(partName);

            _partContents = new string[_parts.Length];
            for (int i = 0; i < _parts.Length; i++)
                _partContents[i] = ObjectNames.NicifyVariableName(_parts[i].GetType().Name);

            _foldout = _parts.Length > 1;
            _selectedIndex = GetSelectedIndex();
        }

        public override void Draw()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(5f);

                using (new GUILayout.VerticalScope())
                {
                    bool newFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_foldout,
                        $"{_partName} ({_parts.Length})");

                    if (_foldout != newFoldout)
                        Event.current.Use();

                    _foldout = newFoldout || !HasParts();

                    if (_foldout)
                    {
                        GUILayout.Space(6f);

                        if (HasParts())
                            DrawToolbar();
                        else
                        {
                            EditorGUILayout.HelpBox(
                                $"No {_partName} found, a null one will be used instead which can cause unexpected issues.",
                                MessageType.Warning);
                        }
                    }

                    EditorGUILayout.EndFoldoutHeaderGroup();
                }
            }
        }

        private void DrawToolbar()
        {
            for (int i = 0; i < _partContents.Length; i++)
            {
                bool selected = DrawToolbarButton(_selectedIndex == i, _partContents[i], _parts[i]);

                if (selected && i != _selectedIndex)
                {
                    if (!Application.isPlaying)
                    {
                        Detach(_parts[_selectedIndex]);
                        _selectedIndex = i;
                        Attach(_parts[i]);
                    }
                    else
                    {
                        _selectedIndex = i;
                        Attach(_parts[i]);
                    }

                    break;
                }
            }
        }

        private int GetSelectedIndex()
        {
            if (_parts.Length == 0)
                return -1;

            int selectedIndex = -1;

            for (var i = 0; i < _parts.Length; i++)
            {
                var attachment = _parts[i];
                if (attachment.IsAttached)
                {
                    if (selectedIndex == -1)
                        selectedIndex = i;
                    else
                        Detach(attachment);
                }
            }

            if (selectedIndex != -1)
                return selectedIndex;

            Attach(_parts[0]);
            return 0;
        }

        private bool HasParts() => _parts.Length > 0;

        private static void Attach(FirearmAttachmentBehaviour attachment)
        {
            attachment.Attach();
            if (!Application.isPlaying)
                EditorUtility.SetDirty(attachment);
        }

        private static void Detach(FirearmAttachmentBehaviour attachment)
        {
            attachment.Detach();
            if (!Application.isPlaying)
                EditorUtility.SetDirty(attachment);
        }

        private static bool DrawToolbarButton(bool isSelected, string content, T part)
        {
            using (new GUILayout.HorizontalScope())
            {
                bool selected = GUILayout.Toggle(isSelected, content, GuiStyles.RadioToggle);

                if (GuiLayout.ColoredButton("Ping", GuiStyles.YellowColor, GUILayout.Height(BUTTON_HEIGHT),
                        GUILayout.Width(BUTTON_HEIGHT * 2f)))
                    EditorGUIUtility.PingObject(part);

                return selected;
            }
        }
    }
}
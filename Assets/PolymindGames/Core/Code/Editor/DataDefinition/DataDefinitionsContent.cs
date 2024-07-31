using PolymindGames;
using UnityEngine;
using System;

namespace PolymindGamesEditor
{
    public sealed class DataDefinitionsContent : IEditorDrawable
    {
        public readonly Element[] Elements;


        #region Logic
        public DataDefinitionsContent(params Element[] pairs)
        {
            Elements = pairs;
        }
        #endregion
        
        #region Internal
        public sealed class Element<T> : Element where T : DataDefinition<T>
        {
            public readonly DataDefinitionToolbar<T> Toolbar;
            private readonly CachedObjectEditor _inspector;
            private readonly EditorDrawableLayoutType _layout;
            private bool _isInspectorVisible;


            public Element(string name, EditorDrawableLayoutType editorToolDrawableLayout = EditorDrawableLayoutType.Vertical, float buttonHeight = 40f)
            {
                // Create the toolbar and inspector.
                Toolbar = new DataDefinitionToolbar<T>(name);
                _inspector = new CachedObjectEditor(Toolbar.Selected);

                Toolbar.DefinitionSelected += _inspector.SetObject;
                Toolbar.ButtonHeight = buttonHeight;

                _layout = editorToolDrawableLayout;

                Toolbar.RefreshDefinitions();
            }

            public override void Draw(float length)
            {
                switch (_layout)
                {
                    case EditorDrawableLayoutType.Vertical:
                        {
                            DoVerticalLayout(GUILayout.ExpandWidth(true));
                            break;
                        }
                    case EditorDrawableLayoutType.Horizontal:
                        {
                            DoHorizontalLayout(GUILayout.Width(Mathf.Clamp(length * 0.3f, 0f, 300f)), GUILayout.ExpandWidth(true));
                            break;
                        }
                    default: throw new ArgumentOutOfRangeException();
                }
            }

            private void DoVerticalLayout(GUILayoutOption option)
            {
                Toolbar.DoLayoutWithHeader("", layoutOption =>
                        _inspector.DrawGUIWithToggle(ref _isInspectorVisible, Toolbar.SelectedName, default(Color), layoutOption),
                    option);
            }

            private void DoHorizontalLayout(GUILayoutOption option, GUILayoutOption option2)
            {
                using (new GUILayout.HorizontalScope())
                {
                    Toolbar.DoLayoutWithHeader("", option);
                    _inspector.DrawGUI(Toolbar.SelectedName, default(Color), option2);
                }
            }
        }

        public abstract class Element
        {
            public abstract void Draw(float length);
        }
        #endregion

        #region Drawing
        public void Draw(Rect position, EditorDrawableLayoutType layoutType)
        {
            switch (layoutType)
            {
                case EditorDrawableLayoutType.Vertical:
                    DoVerticalLayout(position);
                    break;
                case EditorDrawableLayoutType.Horizontal:
                    DoHorizontalLayout(position);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private void DoVerticalLayout(Rect position)
        {
            using (new GUILayout.VerticalScope())
            {
                for (var i = 0; i < Elements.Length; i++)
                {
                    var pair = Elements[i];
                    pair.Draw(position.height / Elements.Length);
                }
            }
        }

        private void DoHorizontalLayout(Rect position)
        {
            using (new GUILayout.HorizontalScope())
            {
                for (var i = 0; i < Elements.Length; i++)
                {
                    var pair = Elements[i];
                    pair.Draw(position.width / Elements.Length);
                }
            }
        }
        #endregion
    }
}
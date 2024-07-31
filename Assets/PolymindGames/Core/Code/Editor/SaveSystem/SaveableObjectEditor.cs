using PolymindGames.SaveSystem;
using PolymindGames;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;
using System;

namespace PolymindGamesEditor.SaveSystem
{
    [CustomEditor(typeof(SaveableObject), true)]
    public sealed class SaveableObjectEditor : ToolboxEditor
    {
        private ISaveableComponent[] _components;
        private bool _componentsFoldout;

        private const string LABEL = "<b>Components</b>";


        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            using (new GUILayout.VerticalScope(GuiStyles.Box))
                _componentsFoldout = EditorGUILayout.Foldout(_componentsFoldout, LABEL, true, GuiStyles.StandardFoldout);

            if (_componentsFoldout)
            {
                using (new GUILayout.VerticalScope(GuiStyles.Box))
                {
                    GuiLayout.Separator();
                    _components ??= GetComponents();
                    for (var i = 0; i < _components.Length; i++)
                    {
                        DrawComponentLabel(_components[i], i);
                        GuiLayout.Separator();
                    }
                }
            }
        }

        private static void DrawComponentLabel(ISaveableComponent component, int index)
        {
            using (new GUILayout.HorizontalScope())
            {
                string componentName = ObjectNames.NicifyVariableName(component.GetType().Name);

                GUILayout.Label(componentName, EditorStyles.miniLabel);

                if (GuiLayout.ColoredButton("Ping", GuiStyles.YellowColor, GUILayout.Height(20), GUILayout.Width(45))
                    && component is Component unityComponent)
                {
                    EditorGUIUtility.PingObject(unityComponent);
                }
            }
        }

        private ISaveableComponent[] GetComponents()
        {
            var components = ((MonoBehaviour)target).GetComponentsInChildren<ISaveableComponent>();
            components ??= Array.Empty<ISaveableComponent>();
            return components;
        }
    }
}
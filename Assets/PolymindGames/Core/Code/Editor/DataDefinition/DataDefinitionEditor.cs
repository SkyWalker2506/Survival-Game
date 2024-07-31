using PolymindGamesEditor.ToolPages;
using PolymindGames;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public abstract class DataDefinitionEditor<T> : ObjectEditor where T : DataDefinition
    {
        protected override bool IgnoreScriptProperty => EditorWindow.HasOpenInstances<ToolsWindow>();
        protected T Definition { get; private set; }


        public override void DrawCustomInspector()
        {
            if (EditorWindow.HasOpenInstances<ToolsWindow>())
                DrawObjectField();
            
            base.DrawCustomInspector();
        }
        
        protected void DrawObjectField()
        {
            using (new GUILayout.HorizontalScope(GuiStyles.Box))
            {
                using (new EditorGUI.DisabledScope(true))
                    EditorGUILayout.ObjectField(target, target.GetType(), target);
            }
        }
        
        protected override void OnHeaderGUI()
        {
            base.OnHeaderGUI();

            if (!EditorWindow.HasOpenInstances<ToolsWindow>())
            {
                Rect headerRect = EditorGUILayout.GetControlRect(false, 35);
                Rect buttonRect = new Rect(headerRect.x + headerRect.width - 100, headerRect.y, 100, 20);

                using (new GuiLayout.BgColorScope(GuiStyles.BlueColor))
                {
                    if (GUI.Button(buttonRect, "Open In Tools"))
                    {
                        ToolsWindow.OpenPage(target);
                        Selection.activeObject = null;
                    }
                }
            }
        }

        protected virtual void OnEnable() => Definition = target as T;
    }

    [CustomEditor(typeof(DataDefinition), true, isFallback = true)]
    public class DataDefinitionEditor : DataDefinitionEditor<DataDefinition>
    { }
}
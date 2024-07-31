using PolymindGamesEditor.ToolPages;
using JetBrains.Annotations;
using PolymindGames;
using UnityEditor;
using UnityEngine;
using System.IO;

namespace PolymindGamesEditor
{
    using MessageType = UnityEditor.MessageType;

    [UsedImplicitly]
    [CustomEditor(typeof(UserOptions), true)]
    public class UserOptionsEditor : ObjectEditor
    {
        protected override bool IgnoreScriptProperty => true;


        public sealed override void DrawCustomInspector()
        {
            if (EditorWindow.HasOpenInstances<ToolsWindow>())
            {
                using (new GUILayout.HorizontalScope(GuiStyles.Box))
                {
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.ObjectField(target, target.GetType(), target);
                }
            }

            GUILayout.Label("Defaults", GuiStyles.LargeTitleLabelCentered);
            
            using (new EditorGUI.DisabledScope(Application.isPlaying))
                DrawInspector();
            
            DrawDeleteSettingsButton();
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

        private void DrawDeleteSettingsButton()
        {
            EditorGUILayout.HelpBox("Used as default options if no saved options files are found on the user's machine.", MessageType.Info);
            var savePath = UserOptionsUtility.GetSavePath(target.GetType());

            using (new EditorGUI.DisabledScope(!File.Exists(savePath)))
            {
                if (GUILayout.Button("Delete options file"))
                    File.Delete(savePath);
            }
        }

        protected virtual void DrawInspector() => base.DrawCustomInspector();
    }
}
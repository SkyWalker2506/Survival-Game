using PolymindGames.UserInterface;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.UISystem
{
    [CustomEditor(typeof(WorkstationInspectControllerUI))]
    public sealed class WorkstationInspectControllerUIEditor : ObjectEditor
    {
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            GuiLayout.Separator();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Show All Inspectable Panels"))
            {
                ShowAllChildPanels(true);
                return;
            }

            if (GUILayout.Button("Hide All Inspectable Panels"))
                ShowAllChildPanels(false);

            EditorGUILayout.EndHorizontal();
        }

        private void ShowAllChildPanels(bool show)
        {
            var obj = ((Component)target).gameObject;

            if (obj == null)
                return;

            var panels = obj.GetComponentsInChildren<PanelUI>();

            foreach (var panel in panels)
            {
                panel.GetComponentInChildren<CanvasGroup>().alpha = show ? 1f : 0f;
                panel.GetComponentInChildren<CanvasGroup>().blocksRaycasts = show;
            }
        }
    }
}
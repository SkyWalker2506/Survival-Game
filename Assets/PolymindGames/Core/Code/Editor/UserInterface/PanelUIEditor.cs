using PolymindGames.UserInterface;
using PolymindGames;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.UISystem
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PanelUI), true)]
    public class PanelUIEditor : ObjectEditor
    {
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            GuiLayout.Separator();

            if (serializedObject.targetObjects.Length <= 1)
            {
                using (new GUILayout.VerticalScope("box"))
                {
                    var panel = (PanelUI)serializedObject.targetObject;
                    GUILayout.Label("Info", EditorStyles.boldLabel);
                    GUILayout.Label($"Is Active: {panel.IsActive}", GuiStyles.BoldMiniGreyLabel);
                    GUILayout.Label($"Is Visible: {panel.IsVisible}", GuiStyles.BoldMiniGreyLabel);

                    using (new GUILayout.HorizontalScope())
                    {
                        bool hasEvents = panel.gameObject.HasComponent<PanelEventsUI>();
                        GUILayout.Label($"Has Events: {hasEvents}", GuiStyles.BoldMiniGreyLabel);

                        var btnStr = !hasEvents ? "Add Events Component" : "Remove Events Component";
                        if (GUILayout.Button(btnStr))
                            AddEventsComponent(panel, !hasEvents);
                    }
                }
            }
            
            using (new GUILayout.HorizontalScope("box"))
            {
                if (GUILayout.Button("Show"))
                {
                    foreach (var panels in serializedObject.targetObjects)
                        ShowPanel((PanelUI)panels, true);
                }

                if (GUILayout.Button("Hide"))
                {
                    foreach (var panels in serializedObject.targetObjects)
                        ShowPanel((PanelUI)panels, false);
                }
            }
        }

        private static void AddEventsComponent(PanelUI panel, bool add)
        {
            if (add)
            {
                Undo.AddComponent<PanelEventsUI>(panel.gameObject);
            }
            else
            {
                var events = panel.GetComponent<PanelEventsUI>();
                Undo.DestroyObjectImmediate(events);
            }
        }

        private static void ShowPanel(PanelUI panel, bool show)
        {
            if (Application.isPlaying)
            {
                if (show) panel.Show();
                else panel.Hide();
            }
            else if (panel.TryGetComponent<CanvasGroup>(out var canvasGroup))
            {
                canvasGroup.alpha = show ? 1f : 0f;
                canvasGroup.blocksRaycasts = show;
                canvasGroup.interactable = show;
                EditorUtility.SetDirty(canvasGroup);
            }
        }
    }
}
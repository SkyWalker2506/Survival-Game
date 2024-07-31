using PolymindGames.UserInterface;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.UISystem
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(SelectableGroupBaseUI), true)]
    public class SelectableGroupUIEditor : ObjectEditor
    {
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            if (serializedObject.targetObjects.Length <= 1)
            {
                using (new GUILayout.VerticalScope("box"))
                {
                    var group = (SelectableGroupBaseUI)serializedObject.targetObject;
                    GUILayout.Label("Info", EditorStyles.boldLabel);

                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.ObjectField("Selected", group.Selected, typeof(SelectableUI), group.Selected);
                    }
                }
            }
        }
    }
}

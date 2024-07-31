using PolymindGamesEditor.ToolPages;
using PolymindGames;
using UnityEditor;

namespace PolymindGamesEditor
{
    [CustomEditor(typeof(GroupDefinition<,>), true)]
    public sealed class DataDefinitionGroupEditor : DataDefinitionEditor
    {
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            if (!EditorWindow.HasOpenInstances<ToolsWindow>())
                DrawCustomPropertySkipIgnore("_members");
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            IgnoreProperty("_members");
        }
    }
}
using PolymindGames.WieldableSystem;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.WieldableSystem
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Wieldable), true)]
    public class WieldableEditor : FoldoutBaseTypeEditor<Wieldable>
    {
        protected override bool IgnoreScriptProperty => true;

        protected override void DrawEndInspector()
        {
            EditorGUILayout.Space();
            string label = Application.isPlaying ? "Debug Mode" : "Debug Mode (active during playmode)";

            bool isDebugMode = Wieldable.IsDebugMode;
            if (GuiLayout.ColoredToggle(ref isDebugMode, label, Color.white))
                Wieldable.EnableDebugMode(isDebugMode);
        }
    }
}
using PolymindGames.WorldManagement;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.World
{
    [CustomEditor(typeof(TimeManager))]
    public sealed class TimeManagerEditor : ToolboxEditor
    {
        private TimeManager Time => (TimeManager)target;
        
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            using (new GUILayout.VerticalScope(GuiStyles.Box))
            {
                GUILayout.Label("Info", EditorStyles.boldLabel);
                GUILayout.Label($"Hour: {Time.Hour}", GuiStyles.BoldMiniGreyLabel);
                GUILayout.Label($"Minute: {Time.Minute}", GuiStyles.BoldMiniGreyLabel);
                GUILayout.Label($"Second: {Time.Second}", GuiStyles.BoldMiniGreyLabel);
                EditorGUILayout.Space();
                GUILayout.Label($"Total Hours: {Time.TotalHours}", GuiStyles.BoldMiniGreyLabel);
                GUILayout.Label($"Total Minutes: {Time.TotalMinutes}", GuiStyles.BoldMiniGreyLabel);
                EditorGUILayout.Space();
                GUILayout.Label($"Real seconds per day: {Time.GetDayDurationInRealSeconds()}", GuiStyles.BoldMiniGreyLabel);
                GUILayout.Label($"Real minutes per day: {Time.GetDayDurationInRealMinutes()}", GuiStyles.BoldMiniGreyLabel);
            }
        }
    }
}
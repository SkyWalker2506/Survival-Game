using PolymindGames;
using PolymindGames.WieldableSystem;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.WieldableSystem
{
    [CustomEditor(typeof(Polearm))]
    public sealed class PolearmEditor : WieldableEditor
    {
        protected override void DrawChildInspector()
        {
            base.DrawChildInspector();

            if (((MonoBehaviour)target).gameObject.HasComponent<IMeleeAttackHandler>())
                EditorGUILayout.HelpBox("Aimer Component: found", UnityEditor.MessageType.Info);
            else
                EditorGUILayout.HelpBox("Aimer Component: missing!", UnityEditor.MessageType.Error);
        }
    }
}
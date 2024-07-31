using PolymindGames;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    [CustomEditor(typeof(LightEffect))]
    public sealed class LightEffectEditor : ObjectEditor
    {
        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            EditorGUILayout.Space();

            if (!Application.isPlaying)
                GUI.enabled = false;

            if (GUILayout.Button("Play (fadeIn = true)"))
                ((LightEffect)target).Play(true);

            if (GUILayout.Button("Play (fadeIn = false)"))
                ((LightEffect)target).Play(false);

            if (!Application.isPlaying)
                GUI.enabled = true;
        }
    }
}
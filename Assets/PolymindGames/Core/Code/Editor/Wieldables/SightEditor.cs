using PolymindGames;
using PolymindGames.WieldableSystem;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.WieldableSystem
{
    [CustomEditor(typeof(FirearmSightBehaviour), true)]
    public class SightEditor : ObjectEditor
    {
        private FirearmSightBehaviour _sight;


        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            EditorGUILayout.Space();

            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                using (new EditorGUI.DisabledScope(!Application.isPlaying || _sight.IsPrefab()))
                {
                    string text = _sight.IsAiming ? "Stop Aim" : "Start Aim";

                    if (GuiLayout.ColoredButton(text, GuiStyles.BlueColor, GUILayout.Width(300f)))
                    {
                        var aimInput = _sight.GetComponentInParent<IAimInputHandler>();
                        aimInput?.Aim(_sight.IsAiming ? WieldableInputPhase.End : WieldableInputPhase.Start);
                    }
                }

                GUILayout.FlexibleSpace();
            }
        }

        private void OnEnable()
        {
            _sight = (FirearmSightBehaviour)target;
        }
    }
}
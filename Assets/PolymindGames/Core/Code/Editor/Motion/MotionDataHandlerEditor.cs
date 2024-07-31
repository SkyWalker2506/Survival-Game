using System;
using System.Linq;
using PolymindGames;
using PolymindGames.ProceduralMotion;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.ProceduralMotion
{
    [CustomEditor(typeof(MotionDataHandler))]
    public sealed class MotionDataHandlerEditor : ObjectEditor
    {
        private MotionDataHandler _dataHandler;
        private string[] _motionTypeNames;
        private bool _isVisualizing;
        private int _selected;


        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                EditorGUILayout.Space();

                EditorGUILayout.HelpBox("Select a state and press on the ''visualize'' button to overwrite the current state of the motions.", UnityEditor.MessageType.Info);

                using (new GUILayout.HorizontalScope())
                {
                    _selected = EditorGUILayout.Popup(GUIContent.none, _selected, _motionTypeNames);

                    GUI.backgroundColor = GuiStyles.BlueColor;
                    bool wasVisualizing = _isVisualizing;
                    _isVisualizing = GUILayout.Toggle(_isVisualizing, "Visualize", "Button");
                    GUI.backgroundColor = Color.white;

                    if (wasVisualizing == _isVisualizing)
                        return;

                    if (_isVisualizing)
                    {
                        if (Enum.TryParse<MovementStateType>(_motionTypeNames[_selected], true, out var state))
                            _dataHandler.Visualize(state);
                    }
                    else
                        _dataHandler.Visualize(null);
                }
            }
        }

        private void OnEnable()
        {
            _dataHandler = (MotionDataHandler)target;
            _motionTypeNames = Enum.GetNames(typeof(MovementStateType)).ToArray();
        }
    }
}
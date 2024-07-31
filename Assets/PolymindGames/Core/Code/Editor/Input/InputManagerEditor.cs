using PolymindGames.InputSystem;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.InputSystem
{
    [CustomEditor(typeof(InputManager))]
    public sealed class InputManagerEditor : ManagerEditor
    {
        private InputManager _manager;
        
        
        public override void DrawCustomInspector()
        {
            using (new EditorGUI.DisabledScope(true))
            {
                if (!Application.isPlaying)
                {
                    EditorGUILayout.Space();
                    GUILayout.Label("Input info in only available while in playmode");
                }
                else
                {
                    DrawEscapeCallbacks();
                    DrawContextStack();
                }
            }
        }

        private void DrawEscapeCallbacks()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Escape Callbacks ({_manager.EscapeCallbacksCount})");

            EditorGUI.indentLevel++;

            int count = _manager.EscapeCallbacksCount;
            for (var i = count - 1; i >= 0; i--)
            {
                var callback = _manager.EscapeCallbacks[i];
                EditorGUILayout.LabelField($"{count - i}: {callback.Target} | {callback.Method.Name}");
            }

            EditorGUI.indentLevel--;
        }

        private void DrawContextStack()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"Context Stack ({_manager.ContextStackCount})");

            EditorGUI.indentLevel++;

            int count = _manager.ContextStackCount;
            for (var i = count - 1; i >= 0; i--)
            {
                var ctx = _manager.ContextStack[i];
                EditorGUILayout.ObjectField($"{count - i}: {ctx.name.Replace("(InputContextGroup)", "")}",
                    ctx, typeof(InputContext), ctx);
            }

            EditorGUI.indentLevel--;
        }

        private void OnEnable()
        {
            _manager = (InputManager)target;
            _manager.ContextChanged += Repaint;
        }

        private void OnDisable()
        {
            _manager.ContextChanged -= Repaint;
            _manager = null;
        }
    }
}
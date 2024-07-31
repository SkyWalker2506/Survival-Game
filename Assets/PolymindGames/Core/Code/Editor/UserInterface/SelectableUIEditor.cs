using System.Collections.Generic;
using System.Linq;
using PolymindGames.UserInterface;
using Toolbox.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace PolymindGamesEditor.UISystem
{
    [CustomEditor(typeof(SelectableUI), true)]
    public sealed class SelectableUIEditor : ObjectEditor
    {
        private static readonly List<SelectableUIEditor> s_Editors = new();
        private static bool s_ShowNavigation;
        
        private readonly GUIContent _visualizeContent
            = EditorGUIUtility.TrTextContent("Visualize", "Show navigation flows between selectable UI elements.");
        
        private SerializedProperty _navigationProperty;
        private SerializedProperty _script;
        private SerializedProperty _selectableProperty;

        private const string SHOW_NAVIGATION_KEY = "SelectableEditor.ShowNavigation";
        private const float ARROW_THICKNESS = 2.5f;
        private const float ARROW_HEAD_SIZE = 1.2f;


        private void OnEnable()
        {
            _script = serializedObject.FindProperty("m_Script");
            _selectableProperty = serializedObject.FindProperty("_isSelectable");
            _navigationProperty = serializedObject.FindProperty("_navigation");

            var ignoredProperties = new[]
            {
                _script.propertyPath, _navigationProperty.propertyPath, _selectableProperty.propertyPath
            };

            for (int i = 0; i < ignoredProperties.Length; i++)
                IgnoreProperty(ignoredProperties[i]);

            s_Editors.Add(this);
            RegisterOnSceneGUI();

            s_ShowNavigation = EditorPrefs.GetBool(SHOW_NAVIGATION_KEY);
        }

        private void OnDisable()
        {
            s_Editors.Remove(this);
            RegisterOnSceneGUI();
        }

        private void RegisterOnSceneGUI()
        {
            SceneView.duringSceneGui -= OnSceneViewGUI;
            if (s_Editors.Count > 0)
                SceneView.duringSceneGui += OnSceneViewGUI;
        }

        public override void DrawCustomInspector()
        {
            serializedObject.Update();

            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(_script);

            ToolboxEditorGui.DrawToolboxProperty(_selectableProperty);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(_navigationProperty);

            EditorGUI.BeginChangeCheck();
            Rect toggleRect = EditorGUILayout.GetControlRect();
            toggleRect.xMin += EditorGUIUtility.labelWidth;
            s_ShowNavigation = GUI.Toggle(toggleRect, s_ShowNavigation, _visualizeContent, EditorStyles.miniButton);
            if (EditorGUI.EndChangeCheck())
            {
                EditorPrefs.SetBool(SHOW_NAVIGATION_KEY, s_ShowNavigation);
                SceneView.RepaintAll();
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            base.DrawCustomInspector();

            if (target.GetInstanceID() < 0 && target.GetType() == typeof(SelectableUI))
            {
                var parent = ((SelectableUI)target).transform.parent;
                if (parent == null || parent.GetComponent<SelectableGroupBaseUI>() == null)
                    EditorGUILayout.HelpBox("No selectable group found on the parent of this object", MessageType.Error);
            }
        }

        private void OnSceneViewGUI(SceneView sceneView)
        {
            if (!s_ShowNavigation)
                return;

            Selectable[] selectables = Selectable.allSelectablesArray;

            for (int i = 0; i < selectables.Length; i++)
            {
                Selectable s = selectables[i];
                if (StageUtility.IsGameObjectRenderedByCamera(s.gameObject, Camera.current))
                    DrawNavigationForSelectable(s);
            }
        }

        private static void DrawNavigationForSelectable(Selectable sel)
        {
            if (sel == null)
                return;

            Transform transform = sel.transform;
            bool active = Selection.transforms.Any(e => e == transform);

            Handles.color = new Color(1.0f, 0.6f, 0.2f, active ? 1.0f : 0.4f);
            DrawNavigationArrow(-Vector2.right, sel, sel.FindSelectableOnLeft());
            DrawNavigationArrow(Vector2.up, sel, sel.FindSelectableOnUp());

            Handles.color = new Color(1.0f, 0.9f, 0.1f, active ? 1.0f : 0.4f);
            DrawNavigationArrow(Vector2.right, sel, sel.FindSelectableOnRight());
            DrawNavigationArrow(-Vector2.up, sel, sel.FindSelectableOnDown());
        }

        private static void DrawNavigationArrow(Vector2 direction, Selectable fromObj, Selectable toObj)
        {
            if (fromObj == null || toObj == null)
                return;
            
            Transform fromTransform = fromObj.transform;
            Transform toTransform = toObj.transform;

            Vector2 sideDir = new Vector2(direction.y, -direction.x);
            Vector3 fromPoint = fromTransform.TransformPoint(GetPointOnRectEdge(fromTransform as RectTransform, direction));
            Vector3 toPoint = toTransform.TransformPoint(GetPointOnRectEdge(toTransform as RectTransform, -direction));
            float fromSize = HandleUtility.GetHandleSize(fromPoint) * 0.05f;
            float toSize = HandleUtility.GetHandleSize(toPoint) * 0.05f;
            fromPoint += fromTransform.TransformDirection(sideDir) * fromSize;
            toPoint += toTransform.TransformDirection(sideDir) * toSize;
            float length = Vector3.Distance(fromPoint, toPoint);
            Vector3 fromTangent = fromTransform.rotation * direction * (length * 0.3f);
            var rotation = toTransform.rotation;
            Vector3 toTangent = rotation * -direction * (length * 0.3f);

            Handles.DrawBezier(fromPoint, toPoint, fromPoint + fromTangent, toPoint + toTangent, Handles.color, null, ARROW_THICKNESS);
            Handles.DrawAAPolyLine(ARROW_THICKNESS, toPoint, toPoint + rotation * (-direction - sideDir) * (toSize * ARROW_HEAD_SIZE));
            Handles.DrawAAPolyLine(ARROW_THICKNESS, toPoint, toPoint + rotation * (-direction + sideDir) * (toSize * ARROW_HEAD_SIZE));
        }

        private static Vector3 GetPointOnRectEdge(RectTransform rectTrs, Vector2 dir)
        {
            if (rectTrs == null)
                return Vector3.zero;

            if (dir != Vector2.zero)
                dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));

            var rect = rectTrs.rect;
            dir = rect.center + Vector2.Scale(rect.size, dir * 0.5f);

            return dir;
        }
    }
}
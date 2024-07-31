using PolymindGames.BuildingSystem;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.BuildingSystem
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(BuildingPiece), true)]
    public sealed class BuildingPieceEditor : ObjectEditor
    {
        private static Color s_BoundsColor = new(1f, 0.75f, 0f, 0.35f);
        private static float s_BoundsGroundOffset = 0.1f;
        private static float s_BoundsScale = 1f;


        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            GuiLayout.Separator();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical(GuiStyles.Box);

            if (GUILayout.Button("Calculate Bounds"))
            {
                target.SetFieldValue("_localBounds", CalculateBounds());
                EditorUtility.SetDirty(target);
            }

            EditorGUILayout.Space();

            s_BoundsGroundOffset = EditorGUILayout.Slider("Ground Offset", s_BoundsGroundOffset, 0f, 1f);
            s_BoundsScale = EditorGUILayout.Slider("Scale", s_BoundsScale, 0.1f, 2f);
            s_BoundsColor = EditorGUILayout.ColorField("Color", s_BoundsColor);

            EditorGUILayout.EndVertical();
        }
    
        private Bounds CalculateBounds()
        {
            var buildingPiece = (BuildingPiece)target;
            var trs = buildingPiece.transform;

            // Calculate the bounds without modifications
            var rawLocalBounds = new Bounds(trs.position, Vector3.zero);

            Quaternion initialRotation = trs.rotation;
            trs.rotation = Quaternion.identity;

            foreach (var meshRenderer in buildingPiece.GetComponentsInChildren<MeshRenderer>())
                rawLocalBounds.Encapsulate(meshRenderer.bounds);

            rawLocalBounds = new Bounds(rawLocalBounds.center - trs.position, rawLocalBounds.size);

            trs.rotation = initialRotation;

            // Calculate the bounds with modifications
            Vector3 center = rawLocalBounds.center;
            Vector3 extents = rawLocalBounds.extents;
            Vector3 offset = s_BoundsGroundOffset * extents.y * Vector3.up;

            center += offset;
            extents -= offset;
            extents = Vector3.Scale(extents, new Vector3(s_BoundsScale, 1f, s_BoundsScale));

            return new Bounds(center, extents * 2);
        }

        private void OnSceneGUI()
        {
            if (Event.current.type != EventType.Repaint)
                return;
            
            var buildingPiece = (BuildingPiece)target;
            var bounds = buildingPiece.GetWorldBounds();

            var oldColor = Handles.color;
            var oldMatrix = Handles.matrix;

            Handles.color = s_BoundsColor;
            Handles.matrix = Matrix4x4.TRS(bounds.center, buildingPiece.transform.rotation, bounds.size);

            Handles.CubeHandleCap(0, Vector3.zero, Quaternion.identity, 1f, EventType.Repaint);

            Handles.color = oldColor;
            Handles.matrix = oldMatrix;
        }
    }
}
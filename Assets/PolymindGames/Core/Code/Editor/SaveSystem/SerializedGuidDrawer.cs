using System;
using JetBrains.Annotations;
using PolymindGames;
using PolymindGames.SaveSystem;
using Toolbox.Editor;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolymindGamesEditor.SaveSystem
{
    [UsedImplicitly]
    public sealed class SerializedGuidDrawer : ToolboxTargetTypeDrawer
    {
        public override void OnGui(SerializedProperty property, GUIContent label)
        {
            Object targetObj = property.serializedObject.targetObject;

            using (new EditorGUI.DisabledScope(true))
            {
                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);

                if (property.isExpanded)
                {
                    using (new EditorGUILayout.HorizontalScope())
                        DrawGuidGui(property, targetObj);
                }
            }
        }

        public override Type GetTargetType() => typeof(SerializedGuid);
        public override bool UseForChildren() => true;
        
        private static void DrawGuidGui(SerializedProperty property, Object targetObj)
        {
            var attribute = PropertyUtility.GetAttribute<SerializedGuidDetailsAttribute>(property);
            bool isDisabled = (attribute == null || attribute.DisableForPrefabs) && UnityUtils.IsAssetOnDisk((Component)targetObj);

            if (isDisabled)
                EditorGUILayout.TextField("Guid", "Not available for prefabs");
            else
            {
                Guid guid = (SerializedGuid)property.GetProperValue(property.GetFieldInfo());
                EditorGUILayout.TextField("Guid", guid.ToString());

                if (attribute == null || attribute.HasNewGuidButton)
                {
                    GUI.enabled = true;
                    if (GUILayout.Button("New Guid"))
                    {
                        if (targetObj is GuidComponent guidComponent)
                            GuidManager.Add(guidComponent);

                        property.SetProperValue(property.GetFieldInfo(), new SerializedGuid(Guid.NewGuid()));
                        EditorUtility.SetDirty(targetObj);
                    }
                }
            }
        }
    }
}
using System;
using System.Globalization;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Toolbox.Editor.Drawers
{
    [UsedImplicitly]
    public sealed class LabelFromChildAttributeDrawer : ToolboxSelfPropertyDrawer<LabelFromChildAttribute>
    {
        private static GUIContent GetLabelByValue(SerializedProperty property, GUIContent label)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    break;
                case SerializedPropertyType.Integer:
                    label.text = property.intValue.ToString();
                    break;
                case SerializedPropertyType.Boolean:
                    label.text = property.boolValue.ToString();
                    break;
                case SerializedPropertyType.Float:
                    label.text = property.floatValue.ToString(CultureInfo.InvariantCulture);
                    break;
                case SerializedPropertyType.String:
                    label.text = property.stringValue;
                    break;
                case SerializedPropertyType.Color:
                    label.text = property.colorValue.ToString();
                    break;
                case SerializedPropertyType.ObjectReference:
                    label.text = property.objectReferenceValue ? property.objectReferenceValue.name : "null";
                    break;
                case SerializedPropertyType.LayerMask:
                    label.text = property.intValue switch
                    {
                        0 => "Nothing",
                        ~0 => "Everything",
                        _ => LayerMask.LayerToName((int)Mathf.Log(property.intValue, 2))
                    };
                    break;
                case SerializedPropertyType.Enum:
                    label.text = property.enumNames[property.enumValueIndex];
                    break;
                case SerializedPropertyType.Vector2:
                    label.text = property.vector2Value.ToString();
                    break;
                case SerializedPropertyType.Vector3:
                    label.text = property.vector3Value.ToString();
                    break;
                case SerializedPropertyType.Vector4:
                    label.text = property.vector4Value.ToString();
                    break;
                case SerializedPropertyType.Rect:
                    label.text = property.rectValue.ToString();
                    break;
                case SerializedPropertyType.Character:
                    label.text = ((char)property.intValue).ToString();
                    break;
                case SerializedPropertyType.Bounds:
                    label.text = property.boundsValue.ToString();
                    break;
                case SerializedPropertyType.Quaternion:
                    label.text = property.quaternionValue.ToString();
                    break;
                case SerializedPropertyType.ArraySize:
                    break;
                case SerializedPropertyType.AnimationCurve:
                    break;
                case SerializedPropertyType.Gradient:
                    break;
                case SerializedPropertyType.ExposedReference:
                    break;
                case SerializedPropertyType.FixedBufferSize:
                    break;
                case SerializedPropertyType.Vector2Int:
                    break;
                case SerializedPropertyType.Vector3Int:
                    break;
                case SerializedPropertyType.RectInt:
                    break;
                case SerializedPropertyType.BoundsInt:
                    break;
                case SerializedPropertyType.ManagedReference:
                    break;
                case SerializedPropertyType.Hash128:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return label;
        }

        protected override void OnGuiSafe(SerializedProperty property, GUIContent label, LabelFromChildAttribute attribute)
        {
            var propertyName = attribute.ChildName;
            var childProperty = property.FindPropertyRelative(propertyName);

            // validate availability of the child property
            if (childProperty != null)
            {
                // set new label if found (unknown types will be ignored)
                label = GetLabelByValue(childProperty, label);

                property.isExpanded = EditorGUILayout.Foldout(property.isExpanded, label, true);
                if (property.isExpanded)
                    ToolboxEditorGui.DrawPropertyChildren(property);
            }
            else
            {
                Debug.LogWarning($"{propertyName} does not exists.");
                ToolboxEditorGui.DrawNativeProperty(property, label);
            }
        }

        public override bool IsPropertyValid(SerializedProperty property) => property.hasChildren;
    }
}
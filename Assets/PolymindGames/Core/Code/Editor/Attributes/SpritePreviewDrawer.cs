using PolymindGames;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    [CustomPropertyDrawer(typeof(SpritePreviewAttribute))]
    public sealed class SpritePreviewDrawer : PropertyDrawerBase
    {
        private const float IMAGE_HEIGHT = 64;


        protected override float GetPropertyHeightSafe(SerializedProperty property, GUIContent label)
        {
            if (property.propertyType is SerializedPropertyType.ObjectReference && property.objectReferenceValue is Sprite)
                return EditorGUI.GetPropertyHeight(property, label, true) + IMAGE_HEIGHT + 10;

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        protected override void OnGUISafe(Rect position, SerializedProperty property, GUIContent label)
        {
            // Draw the normal property field
            EditorGUI.PropertyField(position, property, label, true);

            if (property.propertyType == SerializedPropertyType.ObjectReference)
            {
                if (property.objectReferenceValue is Sprite sprite)
                {
                    position.x += EditorGUIUtility.labelWidth;
                    position.y += EditorGUI.GetPropertyHeight(property, label, true) + 5;
                    position.height = IMAGE_HEIGHT;
                    position.width = position.height = IMAGE_HEIGHT;

                    GUI.DrawTexture(position, sprite.texture);
                }
            }
        }
    }
}
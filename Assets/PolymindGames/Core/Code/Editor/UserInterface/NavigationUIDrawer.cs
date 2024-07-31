using PolymindGames.UserInterface;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.UISystem
{
    /// <summary>
    /// This is a PropertyDrawer for NavigationUI. It is implemented using the standard Unity PropertyDrawer framework.
    /// </summary>
    [CustomPropertyDrawer(typeof(NavigationUI), true)]
    public sealed class NavigationUIDrawer : PropertyDrawerBase
    {
        private static readonly GUIContent s_NavigationContent = EditorGUIUtility.TrTextContent("Navigation");


        protected override void OnGUISafe(Rect pos, SerializedProperty prop, GUIContent label)
        {
            Rect bgRect = pos;
            bgRect.y -= EditorGUIUtility.singleLineHeight / 2;
            bgRect.height += EditorGUIUtility.singleLineHeight * 2f;
            EditorGUI.DrawRect(bgRect, new Color(0.2f, 0.2f, 0.2f));
            
            Rect drawRect = pos;
            drawRect.height = EditorGUIUtility.singleLineHeight;
            drawRect.x += 6f;
            drawRect.width -= 6f;

            SerializedProperty navigation = prop.FindPropertyRelative("_mode");
            SerializedProperty wrapAround = prop.FindPropertyRelative("_wrapAround");
            NavigationUI.NavigationMode navMode = GetNavigationUIMode(navigation);

            EditorGUI.PropertyField(drawRect, navigation, s_NavigationContent);
            
            ++EditorGUI.indentLevel;
            drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            switch (navMode)
            {
                case NavigationUI.NavigationMode.Horizontal:
                case NavigationUI.NavigationMode.Vertical:
                    {
                        EditorGUI.PropertyField(drawRect, wrapAround);
                        drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                    break;
                case NavigationUI.NavigationMode.Explicit:
                    {
                        SerializedProperty selectOnUp = prop.FindPropertyRelative("_selectOnUp");
                        SerializedProperty selectOnDown = prop.FindPropertyRelative("_selectOnDown");
                        SerializedProperty selectOnLeft = prop.FindPropertyRelative("_selectOnLeft");
                        SerializedProperty selectOnRight = prop.FindPropertyRelative("_selectOnRight");

                        EditorGUI.PropertyField(drawRect, selectOnUp);
                        drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(drawRect, selectOnDown);
                        drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(drawRect, selectOnLeft);
                        drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                        EditorGUI.PropertyField(drawRect, selectOnRight);
                        drawRect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                    }
                    break;
            }

            --EditorGUI.indentLevel;
        }

        protected override float GetPropertyHeightSafe(SerializedProperty prop, GUIContent label)
        {
            SerializedProperty navigation = prop.FindPropertyRelative("_mode");
            if (navigation == null)
                return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            NavigationUI.NavigationMode navMode = GetNavigationUIMode(navigation);

            switch (navMode)
            {
                case NavigationUI.NavigationMode.None:
                    return EditorGUIUtility.singleLineHeight;
                case NavigationUI.NavigationMode.Horizontal:
                case NavigationUI.NavigationMode.Vertical:
                    return 2 * EditorGUIUtility.singleLineHeight + 2 * EditorGUIUtility.standardVerticalSpacing;
                case NavigationUI.NavigationMode.Explicit:
                    return 5 * EditorGUIUtility.singleLineHeight + 5 * EditorGUIUtility.standardVerticalSpacing;
                default:
                    return EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            }
        }

        private static NavigationUI.NavigationMode GetNavigationUIMode(SerializedProperty navigation)
        {
            return (NavigationUI.NavigationMode)navigation.enumValueIndex;
        }
    }
}
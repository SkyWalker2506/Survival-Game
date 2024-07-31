using PolymindGames;
using Toolbox.Editor.Drawers;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    [CustomPropertyDrawer(typeof(DataIdReference<>))]
    [CustomPropertyDrawer(typeof(DataNameReference<>))]
    [CustomPropertyDrawer(typeof(DataReferenceDetailsAttribute))]
    public sealed class DataReferencePropertyDrawer : PropertyDrawerBase
    {
        private SerializedProperty _property;
        private DataReferenceDrawer _referenceDrawer;


        public override bool IsPropertyValid(SerializedProperty property) => !property.hasMultipleDifferentValues;

        protected override float GetPropertyHeightSafe(SerializedProperty property, GUIContent label)
        {
            HandlePropertyChanges(property);
            return _referenceDrawer.GetHeight();
        }

        protected override void OnGUISafe(Rect position, SerializedProperty property, GUIContent label)
        {
            HandlePropertyChanges(property);
            var valueProperty = property.FindPropertyRelative("_value");
            _referenceDrawer.OnGUI(valueProperty, position, label);
        }
 
        private void HandlePropertyChanges(SerializedProperty property)
        {
            if (property != _property)
            {
                _property = property;
                _referenceDrawer = new DataReferenceDrawer(property, fieldInfo);
            }
        }
    }
}
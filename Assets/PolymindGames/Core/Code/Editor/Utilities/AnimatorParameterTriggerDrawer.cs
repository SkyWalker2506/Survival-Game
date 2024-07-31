using System.Linq;
using PolymindGames;
using Toolbox.Editor;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace PolymindGamesEditor
{
    [CustomPropertyDrawer(typeof(AnimatorParameterTrigger))]
    public class AnimatorParameterTriggerDrawer : PropertyDrawer
    {
        private AnimatorControllerParameterType _animatorParameterType;
        private SerializedProperty _valueProp;
        private SerializedProperty _typeProp;
        private SerializedProperty _nameProp;
        private int _selectedValue;

        private const float INDENTATION = 6f;
        

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Application.isPlaying)
                return;

            position.height = EditorGUIUtility.singleLineHeight;
            property.isExpanded = true;

            _typeProp = property.FindPropertyRelative("Type");
            _nameProp = property.FindPropertyRelative("Name");
            _valueProp = property.FindPropertyRelative("Value");

            EditorGUI.PropertyField(position, _typeProp, true);

            AnimatorControllerParameterType paramType = (AnimatorControllerParameterType)_typeProp.GetProperValue(_typeProp.GetFieldInfo());
            position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;

            // Draw animator param
            if (paramType != _animatorParameterType)
                _selectedValue = 0;

            _animatorParameterType = paramType;

            if (AnimatorParameters(position, _nameProp))
            {
                position.x += INDENTATION;
                position.y = position.yMax + EditorGUIUtility.standardVerticalSpacing;
                position.width -= INDENTATION;

                switch (paramType)
                {
                    case AnimatorControllerParameterType.Bool:
                        {
                            bool value = !Mathf.Approximately(_valueProp.floatValue, 0f);
                            value = EditorGUI.Toggle(position, "Bool ", value);

                            _valueProp.floatValue = value ? 1f : 0f;
                            break;
                        }
                    case AnimatorControllerParameterType.Float or AnimatorControllerParameterType.Int:
                        {
                            if (paramType == AnimatorControllerParameterType.Float)
                                _valueProp.floatValue = EditorGUI.FloatField(position, "Float ", _valueProp.floatValue);
                            else
                            {
                                int value = EditorGUI.IntField(position, "Integer ", Mathf.RoundToInt(_valueProp.floatValue));
                                _valueProp.floatValue = Mathf.Clamp(value, -9999999, 9999999);
                            }

                            break;
                        }
                    default:
                        EditorGUI.LabelField(position, "Trigger ");
                        break;
                }
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (Application.isPlaying)
                return EditorGUIUtility.singleLineHeight;

            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            return height * 3f;
        }

        public override bool CanCacheInspectorGUI(SerializedProperty property) => false;

        private bool AnimatorParameters(Rect position, SerializedProperty property)
        {
            var animatorController = GetAnimatorController(property);
            if (animatorController == null)
            {
                position.height *= 2f;
                EditorGUI.HelpBox(position, $"No animator found..", UnityEditor.MessageType.Error);
                return false;
            }

            var parameters = animatorController.parameters;
            if (parameters.Length == 0)
            {
                position.height *= 2f;
                EditorGUI.HelpBox(position, $"No animation parameters found.", UnityEditor.MessageType.Warning);
                return false;
            }

            var eventNames = parameters
                .Where(t => CanAddEventName(t.type))
                .Select(t => t.name).ToList();

            if (eventNames.Count == 0)
            {
                position.height *= 2f;
                EditorGUI.HelpBox(position, $"No animation parameters of type {_animatorParameterType.ToString()} found.", UnityEditor.MessageType.Info);
                return false;
            }

            var eventNamesArray = eventNames.ToArray();
            var matchIndex = eventNames.FindIndex(eventName => eventName.Equals(property.stringValue));

            if (matchIndex != -1)
                _selectedValue = matchIndex;

            _selectedValue = EditorGUI.IntPopup(position, "Param ", _selectedValue, eventNamesArray, SetOptionValues(eventNamesArray));

            property.stringValue = eventNamesArray[_selectedValue];

            return true;
        }

        private AnimatorController GetAnimatorController(SerializedProperty property)
        {
            if (property.serializedObject.targetObject is not Component component)
                return null;

            // Try get animator in children
            var anim = component.GetComponentInChildren<Animator>();

            // Try get animator in sibling
            if (anim == null && component.transform.parent != null)
                anim = component.transform.parent.GetComponentInChildren<Animator>();

            // Try get animator in parent
            if (anim == null)
                anim = component.GetComponentInParent<Animator>();

            if (anim == null)
            {
                Debug.LogException(new MissingComponentException("Missing Animator Component"));
                return null;
            }

            return anim.runtimeAnimatorController as AnimatorController;
        }

        private bool CanAddEventName(AnimatorControllerParameterType animatorControllerParameterType)
        {
            return (int)animatorControllerParameterType == (int)_animatorParameterType;
        }

        private int[] SetOptionValues(string[] eventNames)
        {
            int[] optionValues = new int[eventNames.Length];
            for (int i = 0; i < eventNames.Length; i++)
            {
                optionValues[i] = i;
            }
            return optionValues;
        }
    }
}
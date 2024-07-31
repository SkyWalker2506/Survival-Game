using System;
using System.Reflection;
using PolymindGames;
using Toolbox.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PolymindGamesEditor
{
    [CustomEditor(typeof(CharacterBehaviour), true)]
    public class CharacterBehaviourEditor : ToolboxEditor
    {
        private DependenciesData _dependenciesData;
        private Character _parentCharacter;
        private string _dependenciesLabel;
        private bool _dependenciesFoldout;


        public override void DrawCustomInspector()
        {
            base.DrawCustomInspector();

            if (_parentCharacter == null)
            {
                using (new GUILayout.VerticalScope(GuiStyles.Box))
                    EditorGUILayout.HelpBox("No Parent Character Found", UnityEditor.MessageType.Error);

                return;
            }

            if (_dependenciesData.IsValid)
            {
                EditorGUILayout.Space();
                _dependenciesFoldout = EditorGUILayout.Foldout(_dependenciesFoldout, _dependenciesLabel, true, GuiStyles.StandardFoldout);
                if (_dependenciesFoldout)
                {
                    using (new GUILayout.VerticalScope(GuiStyles.Box))
                    {
                        GUILayout.Label("Required", EditorStyles.miniBoldLabel);
                        DrawComponentLabels(_parentCharacter, _dependenciesData.RequiredComponents, UnityEditor.MessageType.Error);
                    }

                    using (new GUILayout.VerticalScope(GuiStyles.Box))
                    {
                        GUILayout.Label("Optional", EditorStyles.miniBoldLabel);
                        DrawComponentLabels(_parentCharacter, _dependenciesData.OptionalComponents, UnityEditor.MessageType.Warning);
                    }
                }
            }
        }

        protected virtual void OnEnable()
        {
            _dependenciesData = CreateBehaviourData(target.GetType());
            _parentCharacter = ((MonoBehaviour)target).gameObject.GetComponentInRoot<Character>();

            (int required, int optional) = GetFoundCount(_parentCharacter, _dependenciesData);
            _dependenciesLabel = GetDependenciesText(_dependenciesData, required, optional);
            _dependenciesFoldout = _dependenciesData != null && required != _dependenciesData.RequiredComponents.Length;
        }

        private static (int required, int optional) GetFoundCount(Character character, DependenciesData data)
        {
            if (character == null || !data.IsValid)
                return default((int required, int optional));

            int requiredFound = 0;
            foreach (var component in data.RequiredComponents)
                requiredFound += character.GetCC_Editor(component) != null ? 1 : 0;

            int optionalFound = 0;
            foreach (var component in data.OptionalComponents)
                optionalFound += character.GetCC_Editor(component) != null ? 1 : 0;

            return (requiredFound, optionalFound);
        }

        private static string GetDependenciesText(DependenciesData data, int requiredCount, int optionalCount)
        {
            if (!data.IsValid)
                return string.Empty;

            string requiredColor = requiredCount == data.RequiredComponents.Length ? "white" : "red";
            string optionalColor = optionalCount == data.OptionalComponents.Length ? "white" : "yellow";

            return $"<b>Dependencies</b>  " +
                   $"(<color={requiredColor}>{requiredCount}/{data.RequiredComponents.Length}</color>) " +
                   $"(<color={optionalColor}>{optionalCount}/{data.OptionalComponents.Length}</color>)";
        }

        private static DependenciesData CreateBehaviourData(Type behaviourType)
        {
            var requiredTypes = behaviourType.GetCustomAttribute<RequireCharacterComponentAttribute>()?.Types ?? Array.Empty<Type>();
            var optionalTypes = behaviourType.GetCustomAttribute<OptionalCharacterComponentAttribute>()?.Types ?? Array.Empty<Type>();

            return new DependenciesData(requiredTypes, optionalTypes);
        }

        private static void DrawComponentLabels(Character parentCharacter, Type[] componentTypes, UnityEditor.MessageType errorType)
        {
            for (int i = 0; i < componentTypes.Length; i++)
            {
                var type = componentTypes[i];
                DrawComponentLabel(type, parentCharacter.GetCC_Editor(type) as Object, errorType);
            }
        }

        private static void DrawComponentLabel(Type type, Object component, UnityEditor.MessageType messageType)
        {
            using (new GUILayout.HorizontalScope())
            {
                string componentName = ObjectNames.NicifyVariableName(type.Name[1..].Replace("CC", ""));

                GUILayout.Label(componentName, EditorStyles.miniLabel);

                if (GuiLayout.ColoredButton("Ping", GuiStyles.YellowColor, GUILayout.Height(20), GUILayout.Width(45))
                    && component is Component unityComponent)
                {
                    EditorGUIUtility.PingObject(unityComponent);
                }
            }

            if (component == null)
            {
                string errorLabel = messageType == UnityEditor.MessageType.Error
                    ? "Not found in the parent character, this behaviour will not function."
                    : "Not found in the parent character, this behaviour might not behave properly.";

                EditorGUILayout.HelpBox(errorLabel, messageType);
                EditorGUILayout.Space(3f);
            }
        }

        #region Internal
        private sealed class DependenciesData
        {
            public readonly Type[] OptionalComponents;

            public readonly Type[] RequiredComponents;

            public DependenciesData(Type[] required, Type[] optional)
            {
                RequiredComponents = required;
                OptionalComponents = optional;
            }

            public bool IsValid => RequiredComponents.Length > 0 || OptionalComponents.Length > 0;
        }
        #endregion
    }
}
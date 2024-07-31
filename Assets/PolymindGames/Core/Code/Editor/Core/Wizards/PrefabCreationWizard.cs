using Toolbox.Editor;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public abstract class PrefabCreationWizardData : AssetCreationWizardData
    {
        public GameObject Model;
    }

    public abstract class PrefabCreationWizard<T, C> : AssetCreationWizard
        where T : PrefabCreationWizardData
        where C : Component
    {
        private static T s_DataContainer;
        
        private CachedObjectEditor _dataContainerDrawer;

        private const string DEFAULT_WIZARD_TITLE = "Default Wizard Title";


        public override Vector2 GetWindowSize() => new(400f, 500f);

        public sealed override void OnGUI(Rect rect)
        {
            // Draw header
            using (new GuiLayout.HorizontalScope(GuiStyles.Box))
            {
                GUILayout.FlexibleSpace();
                DrawHeader();
                GUILayout.FlexibleSpace();
            }

            // Draw model field
            using (new GuiLayout.VerticalScope(GuiStyles.Box))
            {
                EditorGUILayout.HelpBox("Corresponding mesh or prefab", MessageType.Info);

                var serializedObject = _dataContainerDrawer.Editor.serializedObject;
                serializedObject.Update();
                var modelProperty = serializedObject.FindProperty(nameof(PrefabCreationWizardData.Model));
                EditorGUILayout.PropertyField(modelProperty);
                serializedObject.ApplyModifiedProperties();
            }

            // Draw main body
            using (new GuiLayout.VerticalScope(GuiStyles.Box))
            {
                if (s_DataContainer.Model != null)
                    DrawBody(_dataContainerDrawer);

                GUILayout.FlexibleSpace();
            }

            // Draw create button
            using (new EditorGUI.DisabledScope(s_DataContainer.Model == null))
            {
                if (GuiLayout.ColoredButton("Create Prefab", GuiStyles.GreenColor))
                    CreateAsset();
            }
        }

        public override void OnOpen()
        {
            if (s_DataContainer == null)
                s_DataContainer = ScriptableObject.CreateInstance<T>();

            _dataContainerDrawer = new CachedObjectEditor(s_DataContainer);
            if (_dataContainerDrawer.Editor is ToolboxEditor toolboxEditor)
                toolboxEditor.IgnoreProperty(nameof(PrefabCreationWizardData.Model));
        }

        protected virtual void DrawHeader() => GUILayout.Label(DEFAULT_WIZARD_TITLE);
        protected virtual void DrawBody(CachedObjectEditor objectDrawer) => objectDrawer.DrawGUI();

        private void CreateAsset()
        {
            // Instantiate model..
            GameObject objToInstantiate = s_DataContainer.Model;
            GameObject gameObject = Object.Instantiate(objToInstantiate);
            gameObject.name = $"Rename_{typeof(T)}";

            // Add the custom components to it (e.g. colliders, rigidbody etc.)
            HandleComponents(gameObject, s_DataContainer);

            var creationPath = AssetDatabaseUtility.GetDefaultCreationPathForPrefab(gameObject);
            GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(gameObject, creationPath, InteractionMode.UserAction);

            editorWindow.Close();

            ObjectCreatedCallback?.Invoke(prefab.GetComponent<C>());

            EditorUtility.SetDirty(s_DataContainer);
        }

        protected abstract void HandleComponents(GameObject gameObject, T data);
    }
}
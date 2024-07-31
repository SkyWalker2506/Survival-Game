using System;
using PolymindGames;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor
{
    public static class DataDefinitionEditorUtility
    {
        private const string DEFAULT_CREATION_PATH = "Assets/PolymindGames/TEMP/Resources/";


        /// <returns> The GUI contents (Name, Description and icon) of all of the definitions.</returns>
        public static GUIContent[] GetAllGUIContents<T>(bool name, bool tooltip, bool icon, GUIContent including = null)
            where T : DataDefinition<T>
        {
            bool hasExtraElement = including != null;

            var definitions = DataDefinition<T>.Definitions;
            var contents = new GUIContent[definitions.Length + (hasExtraElement ? 1 : 0)];

            if (hasExtraElement)
            {
                contents[0] = including;
                for (int i = 1; i < contents.Length; i++)
                {
                    var definition = definitions[i - 1];
                    contents[i] = new GUIContent
                    {
                        text = name ? definition.FullName : string.Empty,
                        tooltip = tooltip ? definition.Description : string.Empty,
                        image = definition.Icon != null && icon ? AssetPreview.GetAssetPreview(definition.Icon) : null
                    };
                }
            }
            else
            {
                for (int i = 0; i < contents.Length; i++)
                {
                    var definition = definitions[i];
                    contents[i] = new GUIContent
                    {
                        text = name ? definition.FullName : string.Empty,
                        tooltip = tooltip ? definition.Description : string.Empty,
                        image = definition.Icon != null && icon ? AssetPreview.GetAssetPreview(definition.Icon) : null
                    };
                }
            }

            return contents;
        }
        
        /// <returns> Index of given definition in the internal array.</returns>
        public static int GetIndexOfId<T>(int id) where T : DataDefinition<T>
        {
            var definitions = DataDefinition<T>.Definitions;

            for (int i = 0; i < definitions.Length; i++)
            {
                if (id == definitions[i].Id)
                    return i;
            }

            return -1;
        }

        /// <returns> The definition id at the given index.</returns>
        public static int GetIdAtIndex<T>(int index) where T : DataDefinition<T>
        {
            var definitions = DataDefinition<T>.Definitions;
            index = Mathf.Max(index, 0);

            if (definitions.Length > 0 && index < definitions.Length)
                return definitions[index].Id;

            return -1;
        }

        public static void ResetAllAssetDefinitionNamesAndFix()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            foreach (var dataDef in Resources.LoadAll<DataDefinition>(string.Empty))
            {
                ResetDefinitionAssetName(dataDef);
                dataDef.FixIssues();
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Resets the asset name to the Name of this definition + a prefix.
        /// </summary>
        private static void ResetDefinitionAssetName(DataDefinition dataDef)
        {
            if (dataDef == null)
            {
                Debug.LogError("The passed definition cannot be null");
                return;
            }

            var assetPath = AssetDatabase.GetAssetPath(dataDef);

            if (assetPath != null && !string.IsNullOrEmpty(assetPath))
            {
                string prefix = GetAssetNamePrefix(dataDef.GetType());
                AssetDatabase.RenameAsset(assetPath, $"{prefix}_" + dataDef.Name);
            }
        }

        public static bool TryGetAssetCreationPath<T>(in string defName, out string defPath) where T : DataDefinition<T>
        {
            string creationPath = GetAssetCreationRootPath<T>();

            if (string.IsNullOrEmpty(creationPath) || !AssetDatabase.IsValidFolder(creationPath))
            {
                defPath = string.Empty;
                return false;
            }

            string prefix = GetAssetNamePrefix(typeof(T));
            var newDefPath = $"{creationPath}/{prefix}_{defName}.asset";

            defPath = AssetDatabase.GenerateUniqueAssetPath(newDefPath);

            return true;
        }

        public static string GetDefinitionNameWithPrefix(DataDefinition definition)
        {
            string typeName = GetAssetNamePrefix(definition.GetType());
            return $"{typeName}_{definition.Name}";
        }

        public static string GetAssetNamePrefix(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            return type.Name.Replace("Definition", "");
        }

        public static string GetAssetCreationRootPath<T>() where T : DataDefinition<T>
        {
            var definitions = DataDefinition<T>.Definitions;
            if (definitions.Length > 0)
            {
                string path = AssetDatabase.GetAssetPath(definitions[0]);
                int idx = path.LastIndexOf('/');
                return path.Remove(idx);
            }

            var allFolders = AssetDatabase.GetSubFolders("Assets/");
            foreach (var folder in allFolders)
            {
                if (folder.Contains("Resources"))
                    return folder + typeof(T).Name.Replace("Definition", "");
            }

            string definitionName = typeof(T).Name.Replace("Definition", "");
            return DEFAULT_CREATION_PATH + definitionName;
        }
    }
}
using System.Collections.Generic;
using PolymindGames;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace PolymindGamesEditor
{
    using UnityObject = UnityEngine.Object;

    /// <summary>
    /// Utility class for working with assets in the Unity Editor.
    /// </summary>
    public static class AssetDatabaseUtility
    {
        public static List<T> FindAllAssetsOfType<T>(string folder = "Assets/") where T : UnityObject
        {
            List<T> assets = new();
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", new string[] { folder });
            
            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (asset != null)
                {
                    assets.Add(asset);
                }
            }

            return assets;
        }
        
        /// <summary>
        /// Finds all .unitypackage files under the specified folder path.
        /// </summary>
        /// <param name="folderPath">The folder path to search for .unitypackage files.</param>
        /// <returns>A list containing the paths of all found .unitypackage files.</returns>
        public static List<string> FindAllPackages(string folderPath)
        {
            // Find all .unitypackage files under the specified folder
            string[] packageGUIDs = AssetDatabase.FindAssets("t:DefaultAsset", new string[] { folderPath });

            // List to store the paths of found package files
            var packagePaths = new List<string>();

            // Convert GUIDs to file paths
            foreach (string packageGUID in packageGUIDs)
            {
                string packagePath = AssetDatabase.GUIDToAssetPath(packageGUID);

                // Check if the file has a .unitypackage extension
                if (packagePath.EndsWith(".unitypackage"))
                {
                    packagePaths.Add(packagePath);
                }
            }

            return packagePaths;
        }

        /// <summary>
        /// Finds and returns the most similar asset name wise.
        /// </summary>
        public static UnityObject FindClosestMatchingObjectWithName(Type assetType, string nameToCompare, string ignoredStr)
        {
            var guids = AssetDatabase.FindAssets($"t:{assetType.Name}");

            int mostSimilarIndex = -1;
            int similarityValue = int.MaxValue;

            for (int i = 0; i < guids.Length; i++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[i]);
                string assetName = AssetPathToName(assetPath, ignoredStr);
                
                int similarity = assetName.DamerauLevenshteinDistanceTo(nameToCompare);

                if (similarity < similarityValue)
                {
                    similarityValue = similarity;
                    mostSimilarIndex = i;
                }
            }

            return mostSimilarIndex != -1
                ? AssetDatabase.LoadAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[mostSimilarIndex]), assetType)
                : null;
        }

        /// <summary>
        /// Finds and returns the most similar prefab with the given component name wise.
        /// </summary>
        public static Component FindClosestMatchingPrefab(Type componentType, string nameToCompare, string ignoredStr)
        {
            var guids = AssetDatabase.FindAssets("t:Prefab");

            int similarityValue = int.MaxValue;
            int mostSimilarIndex = -1;

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                var component = AssetDatabase.LoadAssetAtPath<GameObject>(path).GetComponent(componentType);
                if (component == null)
                    continue;

                string prefabName = AssetPathToName(path, ignoredStr);
                int similarity = prefabName.DamerauLevenshteinDistanceTo(nameToCompare);

                if (similarity < similarityValue)
                {
                    similarityValue = similarity;
                    mostSimilarIndex = i;
                }
            }

            return mostSimilarIndex != -1
                ? AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[mostSimilarIndex])).GetComponent(componentType)
                : null;
        }

        public static string AssetPathToName(string path, string ignoredStr)
        {
            string name = Path.GetFileNameWithoutExtension(path);
            if (!string.IsNullOrEmpty(ignoredStr))
                name = name.Replace(ignoredStr, "");

            return name;
        }
        
        public static void DeleteAllAssetsInFolder(string folderPath, bool includeFolder)
        {
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.LogWarning("No folder with path: ''{folderPath}'' found.");
                return;
            }

            DeleteAllAssetsInFolder(folderPath);

            if (includeFolder)
                AssetDatabase.DeleteAsset(folderPath);
        }

        private static void DeleteAllAssetsInFolder(string folderPath)
        {
            string[] assetPaths = AssetDatabase.FindAssets("", new[] { folderPath });

            foreach (string assetPath in assetPaths)
            {
                string path = AssetDatabase.GUIDToAssetPath(assetPath);
                if (AssetDatabase.IsValidFolder(path))
                {
                    // Recursively delete contents of subfolders
                    DeleteAllAssetsInFolder(path);
                }
                else
                {
                    // Delete asset
                    AssetDatabase.DeleteAsset(path);
                }
            }
        }

        public static List<string> FindAllAssetsWithExtension(string extension, string[] paths)
        {
            string[] guids = AssetDatabase.FindAssets("", paths);
            var matchingAssetPaths = new List<string>();
            
            foreach (string guid in guids)
            {
                // Check if the asset path ends with the specified extension
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
                    matchingAssetPaths.Add(assetPath);
            }

            return matchingAssetPaths;
        }

        public static void DeleteAssets(string[] assetGuids)
        {
            // Delete each found asset
            foreach (string guid in assetGuids)
                DeleteAsset(guid);
        }     
        
        public static void DeleteAsset(string assetGuid)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
            AssetDatabase.DeleteAsset(assetPath);
        }
        
        public static string GetDefaultCreationPathForPrefab(GameObject gameObject)
        {
            if (!AssetDatabase.IsValidFolder("Assets/PolymindGames/_Custom"))
                AssetDatabase.CreateFolder("Assets/PolymindGames", "_Custom");

            string localPath = "Assets/PolymindGames/_Custom/" + gameObject.name + ".prefab";

            return AssetDatabase.GenerateUniqueAssetPath(localPath);
        }
    }
}
using PolymindGames;
using UnityEditor;
using UnityEngine;
using System;

namespace PolymindGamesEditor
{
    /// <summary>
    /// Utility class for managing materials across different render pipelines.
    /// </summary>
    public static class RenderPipelineMaterialUtility
    {
        private const string CONVERT_MATERIALS_TO_HDRP_MENU = "Edit/Rendering/Materials/Convert All Built-in Materials to HDRP";

        
        /// <summary>
        /// Converts all materials in the project to the specified render pipeline type.
        /// </summary>
        /// <param name="targetPipelineType">The render pipeline type to convert materials to.</param>
        public static void ConvertAllMaterials(RenderPipelineType targetPipelineType)
        {
            var settings = Resources.Load<MaterialConversionSettings>("Editor/MaterialConversionSettings");
            if (settings == null)
            {
                Debug.LogError("No convert settings found.");
                return;
            }
            
            switch (targetPipelineType)
            {
                case RenderPipelineType.BuiltIn: ConvertMaterialsToBuiltIn(settings); break;
                case RenderPipelineType.Hdrp: ConvertMaterialsToHdrp(settings); break;
                case RenderPipelineType.Urp: ConvertMaterialsToUrp(settings); break;
                default: throw new ArgumentOutOfRangeException(nameof(targetPipelineType), targetPipelineType, null);
            }
            
            Resources.UnloadAsset(settings);
        }

        /// <summary>
        /// Converts all materials in the project to HDRP.
        /// </summary>
        private static void ConvertMaterialsToHdrp(MaterialConversionSettings settings)
        {
            EditorApplication.ExecuteMenuItem(CONVERT_MATERIALS_TO_HDRP_MENU);
        }
        
        /// <summary>
        /// Converts all materials in the project to URP.
        /// </summary>
        private static void ConvertMaterialsToUrp(MaterialConversionSettings settings)
        {
            // TODO: Convert materials to URP
        }

        /// <summary>
        /// Converts all materials in the project to the Built-in Render Pipeline.
        /// </summary>
        private static void ConvertMaterialsToBuiltIn(MaterialConversionSettings settings)
        {
            ConvertAllMaterials(settings);
        }

        private static void ConvertAllMaterials(MaterialConversionSettings settings)
        {
            var materialGuids = AssetDatabase.FindAssets($"t:{nameof(Material)}", new string[] { "Assets/PolymindGames" });
            var dict = settings.GetDictionary();
            
            // Get all materials in the project
            foreach (var materialGuid in materialGuids)
            {
                var materialPath = AssetDatabase.GUIDToAssetPath(materialGuid);
                var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                if (material == null)
                    continue;
                
                if (dict.TryGetValue(material.shader, out var convertInfo))
                    MaterialConvertUtility.ConvertMaterial(material, convertInfo, materialPath);
            }

            // Refresh the AssetDatabase to apply changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
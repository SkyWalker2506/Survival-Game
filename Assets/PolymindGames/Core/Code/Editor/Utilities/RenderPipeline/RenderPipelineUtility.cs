using System.Runtime.CompilerServices;
using System.Collections.Generic;
using UnityEditor.Compilation;
using JetBrains.Annotations;
using UnityEngine.Rendering;
using UnityEditor.Build;
using PolymindGames;
using UnityEditor;
using UnityEngine;
using System;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using Object = UnityEngine.Object;

namespace PolymindGamesEditor
{
    [InitializeOnLoad]
    public static class RenderPipelineUtility
    {
        private static readonly string[] s_RenderPipelineNames = { "Built-In", "HDRP", "URP" };
        private static readonly string[] s_HdrpDependencies = { "com.unity.render-pipelines.high-definition", "com.unity.render-pipelines.high-definition-config" };
        private static readonly string[] s_UrpDependencies = { "com.unity.render-pipelines.universal", "com.unity.render-pipelines.universal-config" };
        private static readonly string[] s_BuiltInDependencies = { "com.unity.postprocessing" };

        private const string BUILT_IN_PIPELINE_DEFINE = "UNITY_POST_PROCESSING_STACK_V2";
        private const string HDRP_PIPELINE_DEFINE = "POLYMIND_GAMES_FPS_HDRP";
        private const string URP_PIPELINE_DEFINE = "POLYMIND_GAMES_FPS_URP";

        private const string RENDER_PIPELINE_STEP_PREF = "RenderPipelineStep";
        private const string RENDER_PIPELINE_FROM_INDEX_PREF = "RenderPipelineFromIndex";
        private const string RENDER_PIPELINE_TARGET_INDEX_PREF = "RenderPipelineTargetIndex";
        

        static RenderPipelineUtility()
        {
            int conversionStep = EditorPrefs.GetInt(RENDER_PIPELINE_STEP_PREF, -1); 
            if (conversionStep == -1)
                return;

            var fromPipeline = (RenderPipelineType)EditorPrefs.GetInt(RENDER_PIPELINE_FROM_INDEX_PREF, (int)GetRenderingPipeline());
            var targetPipeline = (RenderPipelineType)EditorPrefs.GetInt(RENDER_PIPELINE_TARGET_INDEX_PREF, 0);
            SetRenderingPipeline(fromPipeline, targetPipeline, conversionStep);
        }

        /// <summary>
        /// Gets the current rendering pipeline type.
        /// </summary>
        /// <returns>The current rendering pipeline type.</returns>
        [Pure, MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderPipelineType GetRenderingPipeline()
        {
#if POLYMIND_GAMES_FPS_HDRP
            return RenderPipelineType.Hdrp;
#elif POLYMIND_GAMES_FPS_URP
            return RenderPipelineType.Urp;
#else
            return RenderPipelineType.BuiltIn;
#endif
        }

        /// <summary>
        /// Gets the names of the supported render pipelines.
        /// </summary>
        /// <returns>An array of render pipeline names.</returns>
        public static string[] GetRenderPipelineNames() => s_RenderPipelineNames;

        /// <summary>
        /// Sets the active rendering pipeline for the project, if the specified pipeline is valid.
        /// </summary>
        /// <param name="pipelineType">The desired rendering pipeline type.</param>
        public static void SetActiveRenderingPipeline(RenderPipelineType pipelineType)
        {
            if (IsPipelineValid(pipelineType))
                SetRenderingPipeline(GetRenderingPipeline(), pipelineType);
        }

        /// <summary>
        /// Checks if the specified rendering pipeline is valid and can be set as the active pipeline.
        /// </summary>
        /// <param name="pipelineType">The rendering pipeline type to be validated.</param>
        /// <returns>True if the specified pipeline is valid; otherwise, false.</returns>
        private static bool IsPipelineValid(RenderPipelineType pipelineType)
        {
            // Check if the application is currently playing
            if (Application.isPlaying)
            {
                Debug.LogError("You can only change the active rendering pipeline while not playing.");
                return false;
            }
    
            // Check if the desired pipeline is already active
            if (GetRenderingPipeline() == pipelineType)
            {
                Debug.LogError("The selected rendering pipeline is already active.");
                return false;
            }

            if (pipelineType == RenderPipelineType.Urp)
            {
                Debug.LogWarning("URP is not currently supported.");
                return false;
            }
            
            // Check if any conversion packages exist for the desired pipeline
            if (GetPackagesForPipeline(pipelineType).Count == 0)
            {
                Debug.LogError($"No conversion package found for {pipelineType}");
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Sets the rendering pipeline for the project and performs necessary updates.
        /// The process is split into multiple steps to handle compilation and asset modifications.
        /// </summary>
        /// <param name="fromPipeline">The current rendering pipeline type.</param>
        /// <param name="targetPipeline">The target rendering pipeline type.</param>
        /// <param name="conversionStep">The current step of the conversion process.</param>
        private static void SetRenderingPipeline(RenderPipelineType fromPipeline, RenderPipelineType targetPipeline, int conversionStep = 0)
        {
            // Reset the render pipeline step preference
            SetNextStepPref(-1);
            SetFromAndTargetPrefs(fromPipeline, targetPipeline);

            switch (conversionStep)
            {
                case 0: Part1(targetPipeline, fromPipeline);
                    break;
                case 1: EditorCoroutineUtility.StartCoroutineOwnerless(Part2(fromPipeline, targetPipeline));
                    break;
                case 2: Part3(fromPipeline, targetPipeline);
                    break;
                default:
                    return;
            }

            static void Part1(RenderPipelineType targetPipeline, RenderPipelineType fromPipeline)
            {
                // Remove dependencies of the old pipeline
                var dependenciesToAdd = GetDependenciesForPipeline(targetPipeline);
                ProjectModificationUtility.ModifyDependencies(dependenciesToAdd, null);
                
                DeleteVolumeProfilesForPipeline(fromPipeline);
                DeleteRequiredFilesForPipeline(fromPipeline);
                SaveAssetsAndFreeMemory();
                
                // Modify scripting defines for the target pipeline
                var defineToAdd = GetDefinesForPipeline(targetPipeline);
                var defineToRemove = GetDefinesForPipeline(fromPipeline);
                ProjectModificationUtility.ModifyDefines(defineToAdd, defineToRemove, NamedBuildTarget.Standalone);

                SetNextStepPref(1);
                CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.None);
            }

            static IEnumerator Part2(RenderPipelineType fromPipeline, RenderPipelineType targetPipeline)
            {
                // Convert all materials to the target rendering pipeline
                yield return null;
                yield return null;
                yield return null;
                RenderPipelineMaterialUtility.ConvertAllMaterials(targetPipeline);

                SaveAssetsAndFreeMemory();
                ImportPackagesForRenderPipeline(targetPipeline);
                SaveAssetsAndFreeMemory();

                // Remove dependencies of the old pipeline
                var dependenciesToRemove = GetDependenciesForPipeline(fromPipeline);
                ProjectModificationUtility.ModifyDependencies(null, dependenciesToRemove);

                ReimportShaderGraphs();

                // Log success message
                Debug.Log($"The project has been successfully converted to the {targetPipeline} Render Pipeline.");
                
                SetNextStepPref(2);
                CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
            }
            
            static void Part3(RenderPipelineType fromPipeline, RenderPipelineType targetPipeline)
            {
                if (AssetDatabase.IsValidFolder("Assets/HDRPSettings"))
                    AssetDatabaseUtility.DeleteAllAssetsInFolder("Assets/HDRPSettings", true);
                
                // Log success message
                Debug.Log($"The project has been successfully converted to the {targetPipeline} Render Pipeline.");

                SetNextStepPref(-1);
            }
            
            static void SaveAssetsAndFreeMemory()
            {
                AssetDatabase.SaveAssets();
                GC.Collect();
                EditorUtility.UnloadUnusedAssetsImmediate();
                AssetDatabase.Refresh();
            }

            static void SetNextStepPref(int i)
            {
                EditorPrefs.SetInt(RENDER_PIPELINE_STEP_PREF, i);
            }
            
            static void SetFromAndTargetPrefs(RenderPipelineType fromPipeline, RenderPipelineType targetPipeline)
            {
                EditorPrefs.SetInt(RENDER_PIPELINE_FROM_INDEX_PREF, (int)fromPipeline);
                EditorPrefs.SetInt(RENDER_PIPELINE_TARGET_INDEX_PREF, (int)targetPipeline);
            }
        }

        private static void ReimportShaderGraphs()
        {
            var shaderGraphs = AssetDatabaseUtility.FindAllAssetsWithExtension(".shadergraph", new [] { "Assets/PolymindGames" });
            foreach (var shaderGraphPath in shaderGraphs)
                AssetDatabase.ImportAsset(shaderGraphPath, ImportAssetOptions.ForceUpdate);
        }

        private static void DeleteVolumeProfilesForPipeline(RenderPipelineType pipelineType)
        {
            switch (pipelineType)
            {
                case RenderPipelineType.BuiltIn: DeletePostProcessProfiles();
                    break;
                case RenderPipelineType.Hdrp: DeleteVolumeProfiles(); 
                    break;
                case RenderPipelineType.Urp: DeleteVolumeProfiles(); 
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pipelineType), pipelineType, null);
            }

            static void DeleteVolumeProfiles()
            {
                var volumeProfiles = AssetDatabase.FindAssets($"t:{typeof(VolumeProfile)}", new[] { "Assets/PolymindGames" });
                AssetDatabaseUtility.DeleteAssets(volumeProfiles);
            }

            static void DeletePostProcessProfiles()
            {
                var assetGuids = AssetDatabase.FindAssets("t:PostProcessProfile", new[] { "Assets/PolymindGames" });
                AssetDatabaseUtility.DeleteAssets(assetGuids);
            }
        }

        /// <summary>
        /// Handles the destruction of specific pipeline-related files.
        /// </summary>
        private static void DeleteRequiredFilesForPipeline(RenderPipelineType pipelineType)
        {
            switch (pipelineType)
            {
                case RenderPipelineType.BuiltIn:
                    DeleteCtiFiles();
                    break;
                case RenderPipelineType.Hdrp:
                    DeletePipelineSettings(pipelineType);
                    DeleteHdComponents();
                    DeleteBotdFiles();
                    break;
                case RenderPipelineType.Urp:
                    DeletePipelineSettings(pipelineType);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pipelineType), pipelineType, null);
            }

            static void DeletePipelineSettings(RenderPipelineType type)
            {
                var packagesPaths = GetPackagesForPipeline(type);
                foreach (var packagePath in packagesPaths)
                {
                    int index = packagePath.LastIndexOf('/');
                    var settingsFolder = packagePath.Substring(0, index) + "/Settings";
                    AssetDatabaseUtility.DeleteAllAssetsInFolder(settingsFolder, true); 
                }
            }
            
            static void DeleteHdComponents()
            {
                // Find all prefabs in the specified folder
                var guids = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/PolymindGames" });

                foreach (var guid in guids)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guid);
                    var prefabInstance = PrefabUtility.LoadPrefabContents(path);

                    if (prefabInstance != null)
                    {
#if POLYMIND_GAMES_FPS_HDRP
                        bool changed = false;

                        var lightDataComponents = prefabInstance.GetComponentsInChildren<UnityEngine.Rendering.HighDefinition.HDAdditionalLightData>();
                        foreach (var lightData in lightDataComponents)
                        {
                            UnityEngine.Object.DestroyImmediate(lightData, true);
                            changed = true;
                        }
                        
                        var cameraDataComponents = prefabInstance.GetComponentsInChildren<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        foreach (var cameraData in cameraDataComponents)
                        {
                            UnityEngine.Object.DestroyImmediate(cameraData, true);
                            changed = true;
                        }
                        
                        var volumeComponents = prefabInstance.GetComponentsInChildren<Volume>();
                        foreach (var volumeComponent in volumeComponents)
                        {
                            UnityEngine.Object.DestroyImmediate(volumeComponent, true);
                            changed = true;
                        }

                        var reflectionDataComponents = prefabInstance.GetComponentsInChildren<UnityEngine.Rendering.HighDefinition.HDAdditionalReflectionData>();
                        foreach (var reflectionData in reflectionDataComponents)
                        {
                            UnityEngine.Object.DestroyImmediate(reflectionData, true);
                            changed = true;
                        }

                        // Save changes if any components were removed
                        if (changed)
                        {
                            PrefabUtility.SaveAsPrefabAsset(prefabInstance, path);
                        }
#endif

                        PrefabUtility.UnloadPrefabContents(prefabInstance);
                    }
                }
            }
            
            // TODO: Refactor
            static void DeleteCtiFiles()
            {
                const string CTI_FOLDER = "Assets/PolymindGames/ThirdParty/CTI Runtime Components";
                AssetDatabaseUtility.DeleteAllAssetsInFolder(CTI_FOLDER, true);
            }

            // TODO: Refactor
            static void DeleteBotdFiles()
            {
                const string BOTD_FOLDER = "Assets/PolymindGames/ThirdParty/Book Of The Dead";
                AssetDatabaseUtility.DeleteAllAssetsInFolder(BOTD_FOLDER, true);
            }
        }

        /// <summary>
        /// Imports packages required for the specified render pipeline type.
        /// </summary>
        /// <param name="pipelineType">The render pipeline type for which packages are to be imported.</param>
        private static void ImportPackagesForRenderPipeline(RenderPipelineType pipelineType)
        {
            // Get packages for the specified render pipeline type and import them
            var packages = GetPackagesForPipeline(pipelineType);
            foreach (var package in packages)
                AssetDatabase.ImportPackage(package, false);
        }

        /// <summary>
        /// Retrieves packages required for the specified render pipeline type.
        /// </summary>
        /// <param name="pipelineType">The render pipeline type for which packages are to be retrieved.</param>
        /// <returns>A list containing paths to packages required for the specified render pipeline type.</returns>
        private static List<string> GetPackagesForPipeline(RenderPipelineType pipelineType)
        {
            // Find all packages under the specified folder
            var packages = AssetDatabaseUtility.FindAllPackages("Assets/PolymindGames");

            // Filter packages based on the render pipeline type
            string pipelineName = GetPipelineName(pipelineType);
            for (int i = packages.Count - 1; i >= 0; i--)
            {
                if (!packages[i].Contains(pipelineName, StringComparison.CurrentCultureIgnoreCase))
                {
                    packages.RemoveAt(i);
                }
            }

            return packages;
        }

        private static string GetPipelineName(RenderPipelineType pipelineType)
        {
            return pipelineType switch
            {
                RenderPipelineType.BuiltIn => "Built In RP",
                RenderPipelineType.Urp => "Universal RP",
                RenderPipelineType.Hdrp => "High Definition RP",
                _ => throw new ArgumentOutOfRangeException(nameof(pipelineType), pipelineType, null)
            };
        }

        private static string[] GetDefinesForPipeline(RenderPipelineType pipelineType)
        {
            return pipelineType switch
            {
                RenderPipelineType.BuiltIn => new [] { BUILT_IN_PIPELINE_DEFINE },
                RenderPipelineType.Hdrp => new [] { HDRP_PIPELINE_DEFINE },
                RenderPipelineType.Urp => new [] { URP_PIPELINE_DEFINE },
                _ => throw new ArgumentOutOfRangeException(nameof(pipelineType), pipelineType, null)
            };
        }
        
        /// <summary>
        /// Gets the dependencies for the specified rendering pipeline.
        /// </summary>
        /// <param name="pipelineType">The selected rendering pipeline type.</param>
        /// <returns>An array of dependencies for the specified rendering pipeline.</returns>
        private static string[] GetDependenciesForPipeline(RenderPipelineType pipelineType) => pipelineType switch
        {
            RenderPipelineType.Hdrp => s_HdrpDependencies,
            RenderPipelineType.Urp => s_UrpDependencies,
            RenderPipelineType.BuiltIn => s_BuiltInDependencies,
            _ => throw new ArgumentOutOfRangeException(nameof(pipelineType), pipelineType, null)
        };
    }
}

//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using HQFPSTemplate.Equipment;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace HQFPSTemplate.Examples
{
    public class HQHDRPSetupWindow : EditorWindow
    {
        private bool m_DeleteHDRPFolderAfterUpgrade = false;

        private Vector2 m_ScrollPosition;
        private bool m_ShowHowToInstallInfo = true;
        private bool m_ShowMaterialConverterInfo;
        private SerializedObject m_HDRPConverterInfoSerz;
        private HQFPS_HDRPConverterInfo.HDRPConverterInfo m_HDRPInfo;

        private readonly Dictionary<Material, HQFPS_HDRPConverterInfo.ShaderSetup> m_MaterialSetups = new Dictionary<Material, HQFPS_HDRPConverterInfo.ShaderSetup>();
        private readonly Dictionary<ScriptableObject, ScriptableObject> m_ScriptableObjectSetups = new Dictionary<ScriptableObject, ScriptableObject>();
        private readonly Dictionary<GameObject, GameObject> m_PrefabSetups = new Dictionary<GameObject, GameObject>();
        private readonly List<EquipmentItem> m_EquipmentItems = new List<EquipmentItem>();


        [MenuItem("HQ FPS Template/HDRP Setup...", false)]
        public static void Init()
        {
            var window = GetWindow<HQHDRPSetupWindow>(true, "HDRP Setup");
            window.minSize = new Vector2(256, 256);         
        }

        private bool UsingHDRP() 
        {
            if (GraphicsSettings.currentRenderPipeline)
            {
                if (GraphicsSettings.currentRenderPipeline.GetType().ToString().Contains("HighDefinition"))
                    return true;  // HDRP
                else
                    return false; // URP
            }
            else
                return false;
        }

        private void OnEnable()
        {
            var hdrpConverterInfo = Resources.LoadAll<HQFPS_HDRPConverterInfo>("");

            if (hdrpConverterInfo.Length > 0 )
            {
                m_HDRPConverterInfoSerz = new SerializedObject(hdrpConverterInfo[0]);
                GenerateDictionaries(hdrpConverterInfo[0].m_HDRPConverterInfo);

                m_HDRPInfo = hdrpConverterInfo[0].m_HDRPConverterInfo;
            }
            else
            {
                EditorUtility.DisplayDialog("HDRP Converter Info not found!", "Make sure the HDRP_Support package from ''_Integrations'' is installed", "OK");
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.HelpBox("Use this tool to quickly convert your ''HQ FPS Template'' project to HDRP.", MessageType.Info);

            GUILayout.BeginVertical(EditorStyles.textArea);
            m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);

            // Draw Help Foldout
            GUILayout.BeginVertical(EditorStyles.helpBox);
            m_ShowHowToInstallInfo = EditorGUILayout.Foldout(m_ShowHowToInstallInfo, "How to Install...", EditorGUICustom.FoldOutStyle);
            if (m_ShowHowToInstallInfo) DrawHowToInstallInfo();
            GUILayout.EndVertical();

            if (m_HDRPConverterInfoSerz != null)
            {
                m_HDRPConverterInfoSerz.Update();

                // Draw Settings Foldout
                GUILayout.BeginVertical(EditorStyles.helpBox);
                m_ShowMaterialConverterInfo = EditorGUILayout.Foldout(m_ShowMaterialConverterInfo, "Settings...", EditorGUICustom.FoldOutStyle);
                if (m_ShowMaterialConverterInfo) EditorGUILayout.PropertyField(m_HDRPConverterInfoSerz.FindProperty("m_HDRPConverterInfo"));
                GUILayout.EndVertical();

                GUILayout.EndVertical();
                GUILayout.EndScrollView();

                GUILayout.FlexibleSpace();

                if (!UsingHDRP())
                {
                    EditorGUILayout.HelpBox("Rendering pipeline not set to HDRP!", MessageType.Error);
                    GUI.enabled = false;
                }

                EditorGUICustom.Separator(new Color(0.45f, 0.45f, 0.45f), 1);

                GUILayout.BeginHorizontal();
                GUILayout.Label("Upgradeable: ", EditorGUICustom.BottomLeftBoldMiniLabel);
                GUILayout.Label("Materials: (" + m_MaterialSetups.Count.ToString() + ")", EditorStyles.whiteMiniLabel);
                GUILayout.Label("Scriptable Objects: (" + m_ScriptableObjectSetups.Count.ToString() + ")", EditorStyles.whiteMiniLabel);
                GUILayout.Label("Prefabs: (" + m_PrefabSetups.Count.ToString() + ")", EditorStyles.whiteMiniLabel);
                GUILayout.Label("Equipment: (" + m_EquipmentItems.Count.ToString() + ")", EditorStyles.whiteMiniLabel);
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal(EditorStyles.helpBox);
                GUILayout.Space(10f);

                m_DeleteHDRPFolderAfterUpgrade = EditorGUILayout.ToggleLeft("Delete HDRP Folder After Upgrade", m_DeleteHDRPFolderAfterUpgrade, GUILayout.Width(215f));

                // Upgrade HQ FPS Template to HDRP button
                if (GUILayout.Button("Upgrade to HDRP", EditorGUICustom.LargeButtonStyle))
                {
                    try
                    {
                        UpgradeProjectToHDRP();

                        EditorUtility.DisplayDialog("HDRP Upgrade Complete!", "This ''HQ FPS Template'' project is now HDRP ready.", "OK");
                    }
                    catch
                    {
                        EditorUtility.DisplayDialog("HDRP Upgrade Failed!", "This project is not ready to be used with HDRP.", "OK");

                        return;
                    }

                    Close();
                }

                GUILayout.EndHorizontal();

                m_HDRPConverterInfoSerz.ApplyModifiedProperties();
                GUI.enabled = true;
            }
            else
            {
                GUILayout.EndVertical();
                GUILayout.EndScrollView();
            }
        }

        private void DrawHowToInstallInfo()
        {
            string step1 = " 1) Install the latest available HDRP package using the Package Manager.";
            string step2 = " 2) Upgrade the standard materials to HDRP using the built in converter. (Found here: Edit/Render Pipeline/Upgrade Project Materials to HDRP).";
            string step3 = " 3) Install the HQFPS HDRP Package (Found here: HQFPSTemplate/_Integrations/Packages/HQFPSTemplate_HDRPSupport.";
            string step4 = " 4) Set the Scriptable Render Pipeline asset (Edit/Project Settings/Graphics) to the one from the HDRP Package. (Found here: HQFPSTemplate/_Integrations/HDRP Support/Resources)";
            string step5 = " 5) Press the Upgrade button at the bottom of this window.";
            string step6 = " 6) Recommended Demo Scene: HQFPSTemplate/_Integrations/HDRP Support/Scenes.";

            EditorGUILayout.HelpBox(string.Join(" \n", step1, step2, step3, step4, step5, step6), MessageType.None);
        }

        private void GenerateDictionaries(HQFPS_HDRPConverterInfo.HDRPConverterInfo hdrpConverterInfo)
        {
            // Materials
            Material[] materials = EditorProjectUtils.GetAllAssetsOfType<Material>(hdrpConverterInfo.MaterialsPath, hdrpConverterInfo.HDRPFilesPath);

            foreach (var material in materials)
            {
                foreach (var hdrpShaderSetup in hdrpConverterInfo.ShaderSetups)
                {
                    if (material.shader == hdrpShaderSetup.CurrentShader)
                    {
                        m_MaterialSetups.Add(material, hdrpShaderSetup);
                        break;
                    }
                }
            }

            // Scriptable Objects
            foreach (var sObjectSetup in hdrpConverterInfo.ScriptableObjectSetups)
            {
                if (sObjectSetup.ScriptableObjectType != null)
                {
                    var allSObjectOfSameType = EditorProjectUtils.GetAllAssetObjectsOfType<ScriptableObject>(sObjectSetup.ScriptableObjectType.GetType(), hdrpConverterInfo.HDRPFilesPath, sObjectSetup.SearchPath);

                    if (allSObjectOfSameType != null)
                    {
                        var suffixedPairedSObjects = ObjectUtils.GetSuffixedObjectsDictionary(allSObjectOfSameType, hdrpConverterInfo.ObjectMatchSuffix);

                        foreach (var suffixedSObject in suffixedPairedSObjects)
                            m_ScriptableObjectSetups.Add(suffixedSObject.Key, suffixedSObject.Value);
                    }
                }
            }

            // Prefabs
            GameObject[] allHDRPPrefabs = EditorProjectUtils.GetAllPrefabsAtPath(hdrpConverterInfo.HDRPFilesPath);

            foreach (var prefab in hdrpConverterInfo.PrefabsToConvert)
            {
                if (prefab != null)
                    m_PrefabSetups.Add(prefab, ObjectUtils.TryGetSuffixedObject(prefab.name, allHDRPPrefabs, hdrpConverterInfo.ObjectMatchSuffix));
            }

            // HQ FPS Specific
            if (!string.IsNullOrEmpty(hdrpConverterInfo.EquipmentPrefabsPath))
            {
                var eItems = EditorProjectUtils.GetAllPrefabsWithComponent<EquipmentItem>(hdrpConverterInfo.EquipmentPrefabsPath, hdrpConverterInfo.HDRPFilesPath);

                foreach (var eItem in eItems)
                {
                    var lightEffect = eItem.GetComponentInChildren<LightEffect>();

                    if (lightEffect != null && lightEffect.Intensity < 10)
                        m_EquipmentItems.Add(eItem);
                }
            }
        }

        private void UpgradeProjectToHDRP() 
        {
            // Convert the custom materials to HDRP
            foreach (var setup in m_MaterialSetups)
                ConvertMaterial(setup.Key, setup.Value);

            // Convert the Prefabs
            foreach (var prefabSetup in m_PrefabSetups)
                EditorProjectUtils.ReplacePrefab(prefabSetup.Key, prefabSetup.Value);

            // Convert the Scriptable Objects
            foreach (var sObjectSetup in m_ScriptableObjectSetups)
                EditorProjectUtils.ReplaceSerializedObject(sObjectSetup.Key, sObjectSetup.Value);

            // Convert the Equipment Prefabs
            ConvertTheEquipmentItems(m_EquipmentItems.ToArray());

            if (m_DeleteHDRPFolderAfterUpgrade)
                DeleteFolder(m_HDRPInfo.HDRPFilesPath);
        }

        private void ConvertMaterial(Material material, HQFPS_HDRPConverterInfo.ShaderSetup shaderSetup)
        {
            if (shaderSetup.TargetShader != null)
            {
                Dictionary<string, ShaderPropertyType> shaderProperties = new Dictionary<string, ShaderPropertyType>();

                for (int i = 0; i < material.shader.GetPropertyCount(); i++)
                {
                    if(material.HasProperty(material.shader.GetPropertyName(i)))
                        shaderProperties.Add(material.shader.GetPropertyName(i), material.shader.GetPropertyType(i));
                }

                Material prevMaterial = new Material(material);
                material.shader = shaderSetup.TargetShader;

                foreach (var property in shaderSetup.ShaderProperties)
                {
                    if (shaderProperties.TryGetValue(property.CurrentPropertyName, out ShaderPropertyType type))
                    {
                        if (type == ShaderPropertyType.Color)
                        {
                            Color prevColor = prevMaterial.GetColor(property.CurrentPropertyName);
                            material.SetColor(property.TargetPropertyName, prevColor);
                        }
                        else if (type == ShaderPropertyType.Float)
                        {
                            float prevFloat = prevMaterial.GetFloat(property.CurrentPropertyName);
                            material.SetFloat(property.TargetPropertyName, prevFloat);
                        }
                        else if (type == ShaderPropertyType.Vector)
                        {
                            Vector4 prevVector = prevMaterial.GetVector(property.CurrentPropertyName);
                            material.SetVector(property.TargetPropertyName, prevVector);
                        }
                        else if (type == ShaderPropertyType.Texture)
                        {
                            Texture prevTexture = prevMaterial.GetTexture(property.CurrentPropertyName);
                            material.SetTexture(property.TargetPropertyName, prevTexture);
                        }
                    }
                }

                foreach (var customTargetProperty in shaderSetup.TargetShaderCustomProperties)
                {
                    if (customTargetProperty.PropertyType == ShaderPropertyType.Color)
                        material.SetColor(customTargetProperty.PropertyName, customTargetProperty.Color);
                    else if (customTargetProperty.PropertyType == ShaderPropertyType.Float)
                        material.SetFloat(customTargetProperty.PropertyName, customTargetProperty.Float);
                    else if (customTargetProperty.PropertyType == ShaderPropertyType.Vector)
                        material.SetVector(customTargetProperty.PropertyName, customTargetProperty.Vector);
                    else if (customTargetProperty.PropertyType == ShaderPropertyType.Texture)
                        material.SetTexture(customTargetProperty.PropertyName, customTargetProperty.Texture);
                }
            }
        }

        private void ConvertTheEquipmentItems(EquipmentItem[] eItems) 
        {
            foreach (var eItem in eItems)
            {
                var lightEffect = eItem.GetComponentInChildren<LightEffect>();

                if (lightEffect != null)
                {
                    lightEffect.Intensity = 500;

                    if (PrefabUtility.IsPartOfAnyPrefab(eItem.gameObject))
                        PrefabUtility.SavePrefabAsset(eItem.gameObject);
                }
            }    
        }

        private void DeleteFolder(string folderPath)
        {
            foreach (var asset in AssetDatabase.FindAssets("t:Object", new string[] { "Assets/" + folderPath }))
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                AssetDatabase.MoveAssetToTrash(path);
            }
        }
    }
}

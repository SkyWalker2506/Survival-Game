//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using HQFPSTemplate.UserInterface;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HQFPSTemplate.Examples
{
    public class HQSceneSetupWindow : EditorWindow
    {
        private string m_CorePrefabsSearchPath = "HQFPSTemplate/Core/_Prefabs/_Core";
        private readonly string[] m_RootObjNames = new string[] { "Lighting", "Structure", "Pickups", "Destructibles" };
        private Vector2 m_CurrentScrollPosition;

        private bool m_SetupDefault = true;
        private bool m_SetupPlayer = false;
        private bool m_SetupGameManager = false;
        private bool m_SetupSceneUI = false;
        private bool m_SetupPlayerUI = false;
        private bool m_SetupPostProcessingManager = false;
        private bool m_SetupBasicRootObjects = false;

        private bool m_AddSceneToBuildSettings = true;
        private bool m_DestroyExtraCameras = true;


        [MenuItem("HQ FPS Template/Scene Setup...", false)]
        public static void Init() 
        {
            var window = GetWindow<HQSceneSetupWindow>(true, "Scene Setup");
            window.minSize = new Vector2(512, 512);
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical(EditorStyles.textArea);
            m_CurrentScrollPosition = EditorGUILayout.BeginScrollView(m_CurrentScrollPosition);

            EditorGUILayout.HelpBox("Use this tool to quickly Clean/Setup your scenes for ''HQ FPS Template''.", MessageType.Info);

            DrawCorePrefabsInfo();
            EditorGUILayout.Space(2);
            DrawMiscInfo();

            GUILayout.FlexibleSpace();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Spawn selected to Scene ", EditorGUICustom.LargeButtonStyle))
                StartSceneSetup(false);

            if (GUILayout.Button("Clean selected from Scene", EditorGUICustom.LargeButtonStyle))
                StartSceneSetup(true);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }

        private void DrawMiscInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Misc Scene Changes", EditorGUICustom.BottomLeftBoldMiniLabel);

            m_AddSceneToBuildSettings = EditorGUILayout.ToggleLeft("Add Scene To Build Settings", m_AddSceneToBuildSettings);
            m_DestroyExtraCameras = EditorGUILayout.ToggleLeft("Delete Extra Cameras", m_DestroyExtraCameras);

            EditorGUILayout.EndVertical();
        }

        private void DrawCorePrefabsInfo() 
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField("Core Prefabs", EditorGUICustom.BottomLeftBoldMiniLabel);

            m_CorePrefabsSearchPath = EditorGUILayout.TextField("Core Prefabs Search Path:", m_CorePrefabsSearchPath);
            EditorGUICustom.Separator(new Color(0.4f, 0.4f, 0.4f, 1f));

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            m_SetupDefault = EditorGUILayout.Toggle("Select Default", m_SetupDefault);

            if (m_SetupDefault)
                GUI.enabled = false;

            if (m_SetupDefault)
            {
                m_SetupPlayer = true;
                m_SetupGameManager = true;
                m_SetupSceneUI = true;
                m_SetupPlayerUI = true;
                m_SetupPostProcessingManager = true;
                m_SetupBasicRootObjects = false;
            }

            m_SetupPlayer = EditorGUILayout.ToggleLeft("Player", m_SetupPlayer);
            m_SetupGameManager = EditorGUILayout.ToggleLeft("Game Manager", m_SetupGameManager);
            m_SetupSceneUI = EditorGUILayout.ToggleLeft("Scene UI", m_SetupSceneUI);
            m_SetupPlayerUI = EditorGUILayout.ToggleLeft("Player UI", m_SetupPlayerUI);
            m_SetupPostProcessingManager = EditorGUILayout.ToggleLeft("Post Processing Manager", m_SetupPostProcessingManager);

            m_SetupBasicRootObjects = EditorGUILayout.ToggleLeft("Basic Root Objects (Recommended: Don't toggle this when cleaning the scene)", m_SetupBasicRootObjects);

            GUI.enabled = true;

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
        }

        private void StartSceneSetup(bool deletePrefabs) 
        {
            try
            {
                if (m_DestroyExtraCameras) DestroyExtraCameras();
                if (m_AddSceneToBuildSettings) AddSceneToBuildSettings();

                var corePrefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/" + m_CorePrefabsSearchPath });
                GameObject[] corePrefabs = new GameObject[corePrefabGuids.Length];

                for (int i = 0; i < corePrefabGuids.Length; i++)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(corePrefabGuids[i]);
                    corePrefabs[i] = AssetDatabase.LoadAssetAtPath(assetPath, typeof(GameObject)) as GameObject;
                }

                if (!deletePrefabs)
                {
                    SpawnCorePrefabs(corePrefabs);
                    EditorUtility.DisplayDialog("Scene Setup Complete", "This scene is ready to use with HQ FPS Template.", "OK");
                }
                else
                {
                    DestroyCorePrefabs();
                    EditorUtility.DisplayDialog("Scene Setup Reverted", "This scene can't be used with HQ FPS Template.", "OK");
                }

                // Close the Window
                Close();
            }
            catch
            {
                EditorUtility.DisplayDialog("Scene Setup Failed!", "The Scene Setup was unsuccessful.", "OK");
            }    
        }

        private void SpawnCorePrefabs(GameObject[] corePrefab)
        {
            foreach (var prefab in corePrefab)
            {
                bool shouldInstantiate = false;

                if (m_SetupPlayer && GameObject.FindObjectOfType<Player>() == null && prefab.GetComponent<Player>()) shouldInstantiate = true;
                else if (m_SetupGameManager && GameObject.FindObjectOfType<GameManager>() == null && prefab.GetComponent<GameManager>()) shouldInstantiate = true;
                else if (m_SetupPlayerUI && GameObject.FindObjectOfType<UIManager>() == null && prefab.GetComponent<UIManager>()) shouldInstantiate = true;
                else if (m_SetupPostProcessingManager && GameObject.FindObjectOfType<PostProcessingManager>() == null && prefab.GetComponent<PostProcessingManager>()) shouldInstantiate = true;
                else if (m_SetupSceneUI && GameObject.Find("SceneUI") == null && prefab.name == "SceneUI") shouldInstantiate = true;

                if (shouldInstantiate)
                    PrefabUtility.InstantiatePrefab(prefab);
            }

            if (m_SetupBasicRootObjects)
            {
                GameObject tempObj;

                for (int i = 0; i < m_RootObjNames.Length; i++)
                {
                    tempObj = GameObject.Find(m_RootObjNames[i]);

                    if (tempObj != null && tempObj.transform.root == tempObj.transform)
                        continue;

                    new GameObject(m_RootObjNames[i]);
                }
            }
        }

        private void DestroyCorePrefabs()
        {
            List<GameObject> prefabsToDestroy = new List<GameObject>();

            if (m_SetupPlayer && GameObject.FindObjectOfType<Player>())
                prefabsToDestroy.Add(GameObject.FindObjectOfType<Player>().gameObject);
            if (m_SetupGameManager && GameObject.FindObjectOfType<GameManager>())
                prefabsToDestroy.Add(GameObject.FindObjectOfType<GameManager>().gameObject);
            if (m_SetupPlayerUI && GameObject.FindObjectOfType<UIManager>())
                prefabsToDestroy.Add(GameObject.FindObjectOfType<UIManager>().gameObject);
            if (m_SetupPostProcessingManager && GameObject.FindObjectOfType<PostProcessingManager>())
                prefabsToDestroy.Add(GameObject.FindObjectOfType<PostProcessingManager>().gameObject);
            if (m_SetupSceneUI && GameObject.Find("SceneUI"))
                prefabsToDestroy.Add(GameObject.Find("SceneUI").gameObject);

            foreach (var prefab in prefabsToDestroy)
                DestroyImmediate(prefab);

            if (m_SetupBasicRootObjects)
            {
                GameObject ObjectToDestroy; 

                for (int i = 0; i < m_RootObjNames.Length; i++)
                {
                    ObjectToDestroy = GameObject.Find(m_RootObjNames[i]);

                    if (ObjectToDestroy == null)
                        continue;
                    else
                    {
                        if (ObjectToDestroy.transform.root == ObjectToDestroy.transform)
                            continue;
                    }

                    Debug.Log(ObjectToDestroy.name);
                    DestroyImmediate(ObjectToDestroy);
                }
            }
        }

        private void AddSceneToBuildSettings() 
        {
            List<EditorBuildSettingsScene> editorBuildSettingsScenes = new List<EditorBuildSettingsScene>();
            string currentScenePath = SceneManager.GetActiveScene().path;

            foreach (var scene in EditorBuildSettings.scenes)
            {
                // Check if the current scene is already in the build settings
                if (scene.path == currentScenePath)
                    return;

                // Check if the added scenes are not duplicate/null
                if (scene != null && !editorBuildSettingsScenes.Contains(scene))
                    editorBuildSettingsScenes.Add(scene);
            }
            
            // Add the current scene to the build settings
            editorBuildSettingsScenes.Add(new EditorBuildSettingsScene(currentScenePath, true));
            EditorBuildSettings.scenes = editorBuildSettingsScenes.ToArray();
        }

        private void DestroyExtraCameras() 
        {
            Camera[] cameras = GameObject.FindObjectsOfType<Camera>();

            foreach (var camera in cameras)
            {
                if (camera.GetComponentInParent<Player>() == null)
                    DestroyImmediate(camera.gameObject);
            }
        }
    }
}

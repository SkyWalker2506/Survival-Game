using UnityEditor.SceneManagement;
using System.Collections.Generic;
using JetBrains.Annotations;
using PolymindGames;
using UnityEditor;
using UnityEngine;
using System;

namespace PolymindGamesEditor.ToolPages
{
    using MessageType = UnityEditor.MessageType;
    using UnityObject = UnityEngine.Object;
    
    [UsedImplicitly]
    public sealed class ProjectPage : RootPage
    {
        private PolymindAssetInfo[] _assetsInfo;
        private Vector2 _assetsScrollPosition;
        private int _selectedInfo;
        
        public override string DisplayName => "Project";
        public override int Order => -1;
        public override bool DisableInPlaymode => true;
        public override bool IsCompatibleWithObject(UnityObject unityObject) => unityObject == null;
        public override IEnumerable<IEditorToolPage> GetSubPages() => Array.Empty<IEditorToolPage>();
        
        public override void DrawPage(Rect rect)
        {
            using (new GUILayout.VerticalScope(GUILayout.Width(rect.width - 16)))
            {
                GUILayout.Space(6f);
                DrawAssetsInfo();
                GUILayout.FlexibleSpace();
                GuiLayout.Separator();
                DrawRenderPipelines();
                GuiLayout.Separator();
                DrawFixFiles();
                EditorGUILayout.Space();
            }
        }

        private void DrawAssetsInfo()
        {
            _assetsInfo ??= PolymindAssetInfo.GetAll();

            using (new GUILayout.VerticalScope(GuiStyles.Box))
            {
                if (_assetsInfo.Length > 0)
                    EditorGUILayout.HelpBox($"Imported Templates ({_assetsInfo.Length})", MessageType.Info);
                else
                    EditorGUILayout.HelpBox("No imported templates found, this should not happen.", MessageType.Error);
            }

            GuiLayout.Separator();
            
            using (var scrollView = new EditorGUILayout.ScrollViewScope(_assetsScrollPosition))
            {
                _assetsScrollPosition = scrollView.scrollPosition;

                foreach (var assetInfo in _assetsInfo)
                {
                    using (new EditorGUILayout.VerticalScope(GuiStyles.Box))
                        Draw(assetInfo);
                }
            }
        }

        private static void DrawRenderPipelines()
        {
            using (new EditorGUILayout.VerticalScope(GuiStyles.Box))
            {
                EditorGUILayout.HelpBox("Here's the best place to change the active render pipeline for any Polymind Games template.", MessageType.None);

                int selected = GUILayout.Toolbar((int)RenderPipelineUtility.GetRenderingPipeline(), RenderPipelineUtility.GetRenderPipelineNames());
                if (selected != (int)RenderPipelineUtility.GetRenderingPipeline())
                {
                    if (EditorUtility.DisplayDialog("Change Render Pipeline", $"Are you sure you want to change the render pipeline to {RenderPipelineUtility.GetRenderPipelineNames()[selected]}? You might want to back up your project.", "Ok", "Cancel"))
                        RenderPipelineUtility.SetActiveRenderingPipeline((RenderPipelineType)selected);
                }
            }
        }

        private void DrawFixFiles()
        {
            using (new EditorGUILayout.VerticalScope(GuiStyles.Box))
            {
                bool requiresFiles = RequiresFilesFix();
                
                if (requiresFiles)
                {
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.HelpBox("Action Required: This fix is required after importing a Polymind template.", MessageType.Error);
                }
                
                if (requiresFiles)
                    GUILayout.BeginVertical();
                
                EditorGUILayout.HelpBox("Press this button after importing a Polymind template or when instructed by the developer.", MessageType.None);

                if (GUILayout.Button("Fix Files"))
                {
                    if (EditorUtility.DisplayDialog("Fix Files", $"Are you sure you want to fix the files? This includes updating the player, data definitions (items, surfaces etc.) and others.", "Ok", "Cancel"))
                    {
                        FixFiles();

                        foreach (var assetInfo in _assetsInfo)
                        {
                            assetInfo.RequiresFix = false;
                            EditorUtility.SetDirty(assetInfo);
                        }
                    }
                }

                if (requiresFiles)
                {
                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                }
            }
        }

        private bool RequiresFilesFix()
        {
            _assetsInfo ??= PolymindAssetInfo.GetAll();
            foreach (var assetInfo in _assetsInfo)
            {
                if (assetInfo.RequiresFix)
                    return true;
            }

            return false;
        }

        private static void Draw(PolymindAssetInfo assetInfo)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Box(assetInfo.Icon, GUILayout.Width(380), GUILayout.Height(380));

                using (new EditorGUILayout.VerticalScope(GUILayout.Height(380)))
                {
                    EditorGUILayout.Space();
                    GUILayout.Label($"{assetInfo.AssetName} | v{assetInfo.VersionStr}", GuiStyles.LargeTitleLabel);
                    GuiLayout.Separator();
                    GUILayout.Label(assetInfo.Description, GuiStyles.BoldMiniGreyLabel);
                    
                    GUILayout.Space(20f);
                    
                    GUILayout.Label("Getting Started:");
                    GUILayout.Label("- To quickly create your own scene that works perfectly\nwith this asset, you can duplicate the Prototype scene.\n- In the meantime, you can try out one of these scenes:", GuiStyles.BoldMiniGreyLabel);
                    GuiLayout.Separator();
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        foreach (var scene in assetInfo.Scenes)
                        {
                            if (scene.CanBeLoaded && EditorGUILayout.LinkButton(scene.SceneName))
                                EditorSceneManager.OpenScene(scene.ScenePath);
                        }
                    }
                    
                    GUILayout.Space(6f);
                    GuiLayout.Separator();

                    GUILayout.FlexibleSpace();
                    
                    GuiLayout.Separator();
                    DrawUnityVersion(assetInfo);
                    GuiLayout.Separator();
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (EditorGUILayout.LinkButton("Discord"))
                            Application.OpenURL(PolymindAssetInfo.DISCORD_URL);

                        if (EditorGUILayout.LinkButton("Support"))
                            Application.OpenURL(PolymindAssetInfo.SUPPORT_URL);
                
                        if (EditorGUILayout.LinkButton("Youtube"))
                            Application.OpenURL(PolymindAssetInfo.YOUTUBE_URL);
                    }

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (EditorGUILayout.LinkButton("Documentation"))
                            Application.OpenURL(assetInfo.DocsUrl);
                        
                        if (EditorGUILayout.LinkButton("Store Page"))
                            Application.OpenURL(assetInfo.StoreUrl);

                        foreach (var extraSite in assetInfo.ExtraSites)
                        {
                            if (EditorGUILayout.LinkButton(extraSite.Name))
                                Application.OpenURL(extraSite.Url);
                        }
                    }
                    
                    EditorGUILayout.Space();
                }
            }
        }

        private static void DrawUnityVersion(PolymindAssetInfo assetInfo)
        {
            GUILayout.Label("Recommended Unity Versions:");
            
            bool isVersionValid = PolymindAssetInfo.IsUnityVersionValid(Application.unityVersion);
            string message = isVersionValid
                ? $"Active version ({Application.unityVersion}) is compatible."
                : $"Active version ({Application.unityVersion}) is not compatible, use it at your own risk.";

            var messageType = isVersionValid ? MessageType.None : MessageType.Error;
            EditorGUILayout.HelpBox(message, messageType);
            
            foreach (var version in assetInfo.RecommendedUnityVersions)
                GUILayout.Label(version, GuiStyles.BoldMiniGreyLabel);
        }

        private static void FixFiles()
        {
            var allPlayers = FindAllPlayers();
            foreach (var player in allPlayers)
                FixPlayer(player);
            
            DataDefinitionEditorUtility.ResetAllAssetDefinitionNamesAndFix();
        }

        private static List<Player> FindAllPlayers()
        {
            var playerGuids = AssetDatabase.FindAssets("t:GameObject");
            var players = new List<Player>();
            foreach (var guid in playerGuids)
            { 
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                if (obj.TryGetComponent(out Player player))
                    players.Add(player);
            }

            return players;
        }

        private static void FixPlayer(Player player)
        {
            EditorUtility.SetDirty(player);
            foreach (var fixable in player.GetComponentsInChildren<IEditorFixable>())
            {
                fixable.Fix();
                EditorUtility.SetDirty(fixable.gameObject);
            }
        }
    }
}

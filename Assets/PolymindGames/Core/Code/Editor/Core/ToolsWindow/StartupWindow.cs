using PolymindGames;
using UnityEditor;
using UnityEngine;

namespace PolymindGamesEditor.ToolPages
{
    using MessageType = UnityEditor.MessageType;
    
    public sealed class StartupWindow : EditorWindow
    {
        private PolymindAssetInfo[] _assetsInfo;
        private Texture2D _polymindIcon;
        private bool _showOnStartup;
        private bool _showActionRequired;

        private const string SHOW_ON_STARTUP_KEY = "Show_On_Startup";
        
        
        [MenuItem("Tools/Polymind Games/Startup", priority = 1000)]
        private static void Init() => GetStartupWindow();

        [InitializeOnLoadMethod]
        private static void Init2()
        {
            // Subscribe to the EditorApplication.update event
                EditorApplication.update += OnEditorUpdate;
        }

        private static void OnEditorUpdate()
        {
            // Unsubscribe from the EditorApplication.update event to avoid opening the window multiple times
            EditorApplication.update -= OnEditorUpdate;

            // Open your editor window when Unity starts
            if (ShowOnStartup())
                GetStartupWindow();
        }

        private static bool ShowOnStartup()
        {
            bool showOnStartup = RequiresFix() || (EditorApplication.timeSinceStartup < 60f && EditorPrefs.GetBool(SHOW_ON_STARTUP_KEY, true));
            return showOnStartup;
        }

        private static bool RequiresFix()
        {
            var assetsInfo = PolymindAssetInfo.GetAll();
            foreach (var assetInfo in assetsInfo)
            {
                if (!assetInfo.AreRequirementsMet())
                    return true;
            }

            return false;
        }

        private static StartupWindow GetStartupWindow()
        {
            var window = GetWindow<StartupWindow>(true);

            const float WINDOW_WIDTH = 600f;
            const float WINDOW_HEIGHT = 400f;
            
            float x = (Screen.currentResolution.width - WINDOW_WIDTH) / 2f;
            float y = (Screen.currentResolution.height - WINDOW_HEIGHT) / 2f;
            
            window.position = new Rect(x, y, WINDOW_WIDTH, WINDOW_HEIGHT);
            window.minSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
            window.maxSize = new Vector2(WINDOW_WIDTH, WINDOW_HEIGHT);
            
            window.titleContent = new GUIContent("Polymind Startup", Resources.Load<Texture2D>("Icons/Editor_PolymindLogoSmall"));

            return window;
        }
        
        private void OnEnable()
        {
            _polymindIcon = Resources.Load<Texture2D>("Icons/Editor_PolymindLogo");
            _showActionRequired = RequiresFix();
        }

        private void OnGUI()
        {
            GUILayout.FlexibleSpace();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace(); 
                GUILayout.Box(_polymindIcon, GUIStyle.none, GUILayout.Width(400), GUILayout.Height(200f));
                GUILayout.FlexibleSpace();
            }
            GUILayout.FlexibleSpace();
            
            GuiLayout.Separator();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                
                if (EditorGUILayout.LinkButton("Discord"))
                    Application.OpenURL(PolymindAssetInfo.DISCORD_URL);

                if (EditorGUILayout.LinkButton("Support"))
                    Application.OpenURL(PolymindAssetInfo.SUPPORT_URL);
                
                if (EditorGUILayout.LinkButton("Youtube"))
                    Application.OpenURL(PolymindAssetInfo.YOUTUBE_URL);
            }
            
            GuiLayout.Separator();
            
            if (_showActionRequired)
                EditorGUILayout.HelpBox("Action Required!", MessageType.Error);
            
            if (GUILayout.Button("Open Tools Window"))
            {
                ToolsWindow.OpenPage(null);
                Close();
            }

            GuiLayout.Separator();
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                EditorPrefs.SetBool(SHOW_ON_STARTUP_KEY, EditorGUILayout.Toggle("Show On Startup", EditorPrefs.GetBool(SHOW_ON_STARTUP_KEY, true)));
            }
        }
    }
}

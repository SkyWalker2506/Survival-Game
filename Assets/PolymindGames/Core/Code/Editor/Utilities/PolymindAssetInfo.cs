using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace PolymindGames
{
    [CreateAssetMenu(menuName = "Polymind Games/Editor/Asset Info", fileName = "Asset Info")]
    public sealed class PolymindAssetInfo : ScriptableObject
    {
        [Serializable]
        public struct SiteInfo
        {
            public string Name;
            public string Url;

            public SiteInfo(string name, string url)
            {
                Name = name;
                Url = url;
            }
        }

        [SerializeField, BeginGroup]
        private string _assetName;
        
        [SerializeField]
        private Texture2D _icon;

        [SerializeField, Multiline]
        private string _description;

        [SerializeField]
        private SerializedDateTime _date;

        [SerializeField]
        private int _priority;

        [SerializeField]
        private bool _requiresFix = false;
        
        [SerializeField, EndGroup]
        private string _versionString;

        [SerializeField, ReorderableList(ListStyle.Boxed, HasLabels = false)]
        private SerializedScene[] _scenes = Array.Empty<SerializedScene>();

        [SerializeField, ReorderableList(ListStyle.Boxed, HasLabels = false)]
        private string[] _recommendedUnityVersions = Array.Empty<string>();

        [SerializeField, BeginGroup]
        private string _storeUrl;

        [SerializeField, EndGroup]
        private string _docsUrl;

        [SerializeField, ReorderableList(ListStyle.Boxed, elementLabel: "Site")]
        private SiteInfo[] _extraSites = Array.Empty<SiteInfo>();


        public const string YOUTUBE_URL = "https://www.youtube.com/channel/UCYqSdzP7URQzOVlWr-M5Krg";
        public const string DISCORD_URL = "https://discord.com/invite/pkwPNEy";
        public const string SUPPORT_URL = "mailto:" + "Polymindgames@gmail.com";

        public Texture2D Icon => _icon;
        public string AssetName => _assetName;
        public int Priority => _priority;
        public string VersionStr => _versionString;
        
        public bool RequiresFix
        {
            get => _requiresFix;
            set => _requiresFix = value;
        }

        public string StoreUrl => _storeUrl;
        public string DocsUrl => _docsUrl;
        public string Description => _description;
        public IEnumerable<SerializedScene> Scenes => _scenes;
        public IEnumerable<SiteInfo> ExtraSites => _extraSites;
        public IEnumerable<string> RecommendedUnityVersions => _recommendedUnityVersions;

        public static bool IsUnityVersionValid(string version)
        {
            var yearSpan = version.AsSpan().Slice(0, 4);
            return yearSpan.Contains("2022", StringComparison.CurrentCultureIgnoreCase) || yearSpan.Contains("2023", StringComparison.CurrentCultureIgnoreCase);
        }

        public static PolymindAssetInfo[] GetAll()
        {
            var assetInfos = Resources.LoadAll<PolymindAssetInfo>("Editor/");
            return assetInfos != null ? assetInfos.OrderByDescending(info => info.Priority).ToArray() : Array.Empty<PolymindAssetInfo>();
        }
        
        public bool AreRequirementsMet()
        {
            return IsUnityVersionValid(Application.unityVersion) && !_requiresFix;
        }
    }
}

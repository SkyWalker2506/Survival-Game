using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PolymindGames.InventorySystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Items/Rarity Level", fileName = "RarityLevel_")]
    public sealed class ItemRarityLevel : ScriptableObject
    {
        [SerializeField, NewLabel("Name"), BeginGroup]
        private string _rarityName = COMMON_RARITY_NAME;

        [SerializeField, Range(0f, 1f)]
        private float _chance = 0.5f;

        [SerializeField, EndGroup]
        private Color _color = Color.grey;

        private static ItemRarityLevel s_CommonRarity;

        private const string COMMON_RARITY_NAME = "Common";
        private const string RARITY_LEVELS_PATH = "RarityLevels";


        public string Name => _rarityName;
        public float Chance => _chance;
        public Color Color => _color;

        public static ItemRarityLevel CommonRarity
        {
            get
            {
                if (s_CommonRarity == null)
                {
                    // 1) Find the first rarity level in the predefined path that contains "common" in the name.
                    var levelsDefaultPath = Resources.LoadAll<ItemRarityLevel>(RARITY_LEVELS_PATH);
                    s_CommonRarity = levelsDefaultPath.FirstOrDefault((lev) => lev.Name.Contains(COMMON_RARITY_NAME, StringComparison.OrdinalIgnoreCase));

                    if (s_CommonRarity != null)
                        return s_CommonRarity;

                    // 2) If step 1 failed, use the first found rarity level instead. 
                    s_CommonRarity = levelsDefaultPath.FirstOrDefault();

                    if (s_CommonRarity != null)
                        return s_CommonRarity;

                    // 3) If step 2 failed, repeat step 1 but with every rarity level in the project.
                    var levelsNoPath = Resources.LoadAll<ItemRarityLevel>(string.Empty);
                    s_CommonRarity = levelsNoPath.FirstOrDefault((lev) => lev.Name.Contains(COMMON_RARITY_NAME));

                    if (s_CommonRarity != null)
                        return s_CommonRarity;

                    // 4) If step 3 failed, repeat step 2 but with every rarity level in the project.
                    s_CommonRarity = levelsNoPath.FirstOrDefault();

                    if (s_CommonRarity != null)
                        return s_CommonRarity;

                    // 5) If every step failed create a new common rarity level. 
                    s_CommonRarity = CreateInstance<ItemRarityLevel>();

#if UNITY_EDITOR
                    if (!Application.isPlaying)
                    {
                        AssetDatabase.Refresh();
                        AssetDatabase.CreateAsset(s_CommonRarity, $"Assets/PolymindGames/Core/Data/Resources/{RARITY_LEVELS_PATH}/RarityLevel_Common.asset");
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }

                    s_CommonRarity = Resources.Load<ItemRarityLevel>($"{RARITY_LEVELS_PATH}/RarityLevel_Common");
#endif
                }

                return s_CommonRarity;
            }
        }
    }
}
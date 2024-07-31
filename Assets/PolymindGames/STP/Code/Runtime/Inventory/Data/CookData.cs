using UnityEngine;

namespace PolymindGames.InventorySystem
{
    public sealed class CookData : ItemData
    {
        [SerializeField, Range(0, 1440)]
        [Help("The cooking time in game minutes.")]
        private int _cookTime = 60;

        [SerializeField]
        private DataIdReference<ItemDefinition> _cookOutput;
        
        
        public int CookTimeInMinutes => _cookTime;
        public DataIdReference<ItemDefinition> CookedOutput => _cookOutput;
    }
}
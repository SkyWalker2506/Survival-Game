using UnityEngine;

namespace PolymindGames
{
    public sealed class CraftStation : Workstation
    {
        [SerializeField, Range(1, 10), BeginGroup, EndGroup]
        private int _craftableLevel = 1;
        
        
        public int CraftableLevel => _craftableLevel;
    }
}
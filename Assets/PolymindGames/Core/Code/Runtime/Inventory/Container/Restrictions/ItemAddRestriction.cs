using System;

namespace PolymindGames.InventorySystem
{
    [Serializable]
    public sealed class ItemAddRestriction : ItemRestriction
    {
        public override int GetAllowedAddAmount(IItem item, int count) => 0;
    }
}
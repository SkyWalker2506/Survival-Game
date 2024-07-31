// ReSharper disable InconsistentNaming
using PolymindGames.InventorySystem;

namespace PolymindGames.WieldableSystem
{
    /// <summary>
    /// You can extend this by creating another partial class with the same name.
    /// </summary>
    public static partial class WieldableItemConstants
    {
        public static readonly int DURABILITY = ItemPropertyDefinition.GetWithName("Durability").Id;
        public static readonly int AMMO_IN_MAGAZINE = ItemPropertyDefinition.GetWithName("Ammo In Magazine").Id;
        public static readonly int WIELDABLE_TAG = ItemTagDefinition.GetWithName("Wieldable").Id;

        public const float MAX_DURABILITY_VALUE = 100f;
    }
}

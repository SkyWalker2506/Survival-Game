using PolymindGames.InventorySystem;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public interface IWieldableItem
    {
        IItem AttachedItem { get; }
        IWieldable Wieldable { get; }

        event UnityAction<IItem> AttachedItemChanged;
    }
}
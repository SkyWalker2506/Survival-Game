using PolymindGames.WieldableSystem;
using UnityEngine.Events;

namespace PolymindGames
{
    /// <summary>
    /// Takes care of selecting wieldables based on indexes.
    /// </summary>
    public interface IWieldableInventory : ICharacterComponent
    {
        /// <summary>
        /// The current selected index.
        /// </summary>
        int SelectedIndex { get; }
        
        /// <summary>
        /// The previously selected index.
        /// </summary>
        int PreviousIndex { get; }
        
        /// <summary>
        /// Event raised when the selected index has changed. 
        /// </summary>
        event UnityAction<int> SelectedIndexChanged;

        /// <summary>
        /// Equips a wieldable at the given index.
        /// </summary>
        /// <param name="index">index of wieldable/slot.</param>
        void SelectAtIndex(int index);

        /// <summary>
        /// Drops the selected wieldable into the world.
        /// </summary>
        /// <param name="forceDrop">Should the wieldable be dropped in all circumstances? (e.g. when equipping other wieldables)</param>
        void DropWieldable(bool forceDrop = false);

        /// <summary>
        /// Returns a wieldable instance or prefab with the given id.
        /// </summary>
        IWieldable GetWieldableWithId(int id);
        
        /// <summary>
        /// Returns or instantiates a wieldable instance from a prefab with the given id.
        /// </summary>
        IWieldable GetWieldableInstanceWithId(int id);
    }
}
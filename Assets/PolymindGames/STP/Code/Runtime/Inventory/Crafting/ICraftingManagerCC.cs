using PolymindGames.InventorySystem;

namespace PolymindGames
{
    /// <summary>
    /// Manages crafting actions for a character.
    /// </summary>
    public interface ICraftingManagerCC : ICharacterComponent
    {
        /// <summary> Gets a value indicating whether the character is currently crafting. </summary>
        bool IsCrafting { get; }

        /// <summary> Crafts the specified item. </summary>
        /// <param name="itemInfo">Information about the item to craft.</param>
        void Craft(ItemDefinition itemInfo);

        /// <summary> Cancels the current crafting process. </summary>
        void CancelCrafting();
    }

}
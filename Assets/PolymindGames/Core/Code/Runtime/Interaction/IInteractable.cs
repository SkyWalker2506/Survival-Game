namespace PolymindGames
{
    public interface IInteractable : IMonoBehaviour
    {
        bool InteractionEnabled { get; }
        float HoldDuration { get; }

        event InteractEventHandler Interacted;

        /// <summary>
        /// Called when a character interacts with this object.
        /// </summary>
        void OnInteract(ICharacter character);
    }

    public delegate void InteractEventHandler(IInteractable interactable, ICharacter character);
}
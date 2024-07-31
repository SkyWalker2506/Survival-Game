using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public interface IFirearmIndexModeHandler
    {
        FirearmAttachmentBehaviour CurrentMode { get; }
        IFirearm Firearm { get; }

        event UnityAction<FirearmAttachmentBehaviour> ModeChanged;

        void ToggleNextMode();
    }
}
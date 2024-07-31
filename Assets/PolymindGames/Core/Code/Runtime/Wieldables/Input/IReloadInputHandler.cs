namespace PolymindGames.WieldableSystem
{
    public interface IReloadInputHandler
    {
        bool IsReloading { get; }
        ActionBlockHandler ReloadBlocker { get; }

        bool Reload(WieldableInputPhase inputPhase);
    }
}
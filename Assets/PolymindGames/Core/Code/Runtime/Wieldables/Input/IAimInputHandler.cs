namespace PolymindGames.WieldableSystem
{
    public interface IAimInputHandler
    {
        bool IsAiming { get; }
        ActionBlockHandler AimBlocker { get; }

        bool Aim(WieldableInputPhase inputPhase);
    }
}
namespace PolymindGames.WieldableSystem
{
    public interface IUseInputHandler
    {
        bool IsUsing { get; }
        ActionBlockHandler UseBlocker { get; }
        
        bool Use(WieldableInputPhase inputPhase);
    }
}
using PolymindGames.ProceduralMotion;

namespace PolymindGames.WieldableSystem
{
    public interface IWieldableMotion
    {
        IShakeHandler ShakeHandler { get; }
        IMotionDataHandler HandsDataHandler { get; }
        IMotionMixer HandsMotionMixer { get; }
        IMotionDataHandler HeadDataHandler { get; }
        IMotionMixer HeadMotionMixer { get; }
    }
}
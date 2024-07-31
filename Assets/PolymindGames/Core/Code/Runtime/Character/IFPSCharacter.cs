using PolymindGames.ProceduralMotion;

namespace PolymindGames
{
    public interface IFPSCharacter : ICharacter
    {
        IShakeHandler ShakeHandler { get; }
        IMotionMixer HeadMotionMixer { get; }
        IMotionMixer HandsMotionMixer { get; }
        IMotionDataHandler HeadMotionDataHandler { get; }
        IMotionDataHandler HandsMotionDataHandler { get; }
    }
}

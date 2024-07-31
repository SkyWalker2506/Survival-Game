namespace PolymindGames.ProceduralMotion
{
    public interface IShakeHandler : IMonoBehaviour
    {
        void AddShake(ShakeData shake, BodyPoint point = BodyPoint.Head);
        void AddShake(ShakeMotionData shake, float multiplier = 1f, BodyPoint point = BodyPoint.Head);
    }
}

namespace PolymindGames.WieldableSystem
{
    public interface IFirearmRecoilGrip
    {
        float RecoilMultiplier { get; }
    }

    public sealed class DefaultFirearmRecoilGrip : IFirearmRecoilGrip
    {
        public float RecoilMultiplier => 1f;
    }
}
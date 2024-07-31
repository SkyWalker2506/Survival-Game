namespace PolymindGames.WieldableSystem
{
    public interface IFirearmBarrel
    {
        void DoFireEffect();
        void DoFireStopEffect();

        void Attach();
        void Detach();
    }

    public sealed class DefaultFirearmBarrel : IFirearmBarrel
    {
        public static readonly DefaultFirearmBarrel Instance = new();

        private DefaultFirearmBarrel() { }
        
        public void DoFireEffect() { }
        public void DoFireStopEffect() { }
        public void Attach() { }
        public void Detach() { }
    }
}
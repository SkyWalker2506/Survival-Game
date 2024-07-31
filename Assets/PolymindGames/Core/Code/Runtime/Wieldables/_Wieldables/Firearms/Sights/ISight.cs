namespace PolymindGames.WieldableSystem
{
    public interface ISight
    {
        bool IsAiming { get; }
        float AimAccuracyMod { get; }

        bool StartAim();
        bool EndAim();
        void Attach();
        void Detach();
    }

    public sealed class DefaultFirearmSight : ISight
    {
        public static readonly DefaultFirearmSight Instance = new();

        private DefaultFirearmSight() { }
        
        public bool IsAiming => false;
        public float AimAccuracyMod => 1f;
        
        public bool StartAim() => false;
        public bool EndAim() => false;
        public void Attach() { }
        public void Detach() { }
    }
}
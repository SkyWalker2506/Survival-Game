namespace PolymindGames.WieldableSystem
{
    public interface IFirearmRecoilStock
    {
        float RecoilHeatRecover { get; }
        float RecoilHeatRecoverDelay { get; }
        float HipfireAccuracyKick { get; }
        float HipfireAccuracyRecover { get; }
        float AimAccuracyKick { get; }
        float AimAccuracyRecover { get; }

        void DoRecoil(bool isAiming, float heatValue, float triggerValue);
        void SetRecoilMultiplier(float multiplier);
        void Attach();
        void Detach();
    }

    public sealed class DefaultFirearmRecoilStock : IFirearmRecoilStock
    {
        public static readonly DefaultFirearmRecoilStock Instance = new();

        private DefaultFirearmRecoilStock() { }
        
        public float RecoilHeatRecoverDelay => 0.1f;
        public float RecoilHeatRecover => 0.3f;
        public float HipfireAccuracyKick => 0f;
        public float HipfireAccuracyRecover => 0.3f;
        public float AimAccuracyKick => 0f;
        public float AimAccuracyRecover => 0.3f;

        public void DoRecoil(bool isAiming, float heatValue, float triggerValue) { }
        public void SetRecoilMultiplier(float multiplier) { }
        public void Attach() { }
        public void Detach() { }
    }
}
namespace PolymindGames.WieldableSystem
{
    public interface IFirearmFiringSystem
    {
        int AmmoPerShot { get; }

        void Shoot(float accuracy, IFirearmProjectileEffect projectileEffect, float triggerValue);
        void DryFire();

        void Attach();
        void Detach();
    }

    public sealed class DefaultFirearmFiringSystem : IFirearmFiringSystem
    {
        public static readonly DefaultFirearmFiringSystem Instance = new();

        private DefaultFirearmFiringSystem() { }
        
        public int AmmoPerShot => 1;

        public void Shoot(float accuracy, IFirearmProjectileEffect projectileEffect, float triggerValue) { }
        public void DryFire() { }
        public void Attach() { }
        public void Detach() { }
    }
}
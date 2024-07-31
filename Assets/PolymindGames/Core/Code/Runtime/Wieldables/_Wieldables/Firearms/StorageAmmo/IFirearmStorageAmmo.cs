using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public interface IFirearmStorageAmmo
    {
        event UnityAction<int> AmmoCountChanged;

        int RemoveAmmo(int amount);
        int AddAmmo(int amount);
        int GetAmmoCount();
        bool HasAmmo();
        void Attach();
        void Detach();
    }

    public sealed class DefaultFirearmStorageAmmo : IFirearmStorageAmmo
    {
        public static readonly DefaultFirearmStorageAmmo Instance = new();

        private DefaultFirearmStorageAmmo() { }
        
        public event UnityAction<int> AmmoCountChanged
        {
            add { }
            remove { }
        }

        public int RemoveAmmo(int amount) => amount;
        public int AddAmmo(int amount) => amount;
        public int GetAmmoCount() => int.MaxValue;
        public bool HasAmmo() => true;
        public void Attach() { }
        public void Detach() { }
    }
}
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public abstract class FirearmStorageAmmoBehaviour : FirearmAttachmentBehaviour, IFirearmStorageAmmo
    {
        public abstract event UnityAction<int> AmmoCountChanged;


        public abstract int RemoveAmmo(int amount);
        public abstract int AddAmmo(int amount);
        public abstract int GetAmmoCount();
        public abstract bool HasAmmo();

        protected virtual void OnEnable()
        {
            if (Firearm != null)
                Firearm.StorageAmmo = this;
        }
    }
}
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public abstract class FirearmMagazineBehaviour : FirearmAttachmentBehaviour, IFirearmMagazine
    {
        [SerializeField, Range(0, 500), BeginGroup, EndGroup]
        private int _magazineSize;
        
        private int _ammoInMagazine = -1;
        private bool _isReloading;
        

        public bool IsReloading
        {
            get => _isReloading;
            protected set
            {
                if (value == _isReloading)
                    return;

                _isReloading = value;

                if (_isReloading)
                    ReloadStarted?.Invoke(AmmoToLoad);
            }
        }

        public int AmmoInMagazine
        {
            get => _ammoInMagazine;
            protected set
            {
                int clampedValue = Mathf.Clamp(value, 0, MagazineSize);

                if (clampedValue != _ammoInMagazine)
                {
                    int prevInMagazine = _ammoInMagazine;
                    _ammoInMagazine = clampedValue;
                    AmmoInMagazineChanged?.Invoke(prevInMagazine, _ammoInMagazine);
                }
            }
        }

        public int MagazineSize => _magazineSize;

        protected int AmmoToLoad { get; set; }
        protected bool IsMagazineEmpty => AmmoInMagazine <= 0;
        protected bool IsMagazineFull => AmmoInMagazine >= MagazineSize;

        public event UnityAction<int, int> AmmoInMagazineChanged;
        public event UnityAction<int> ReloadStarted;

        protected virtual void OnEnable()
        {
            if (Firearm != null)
                Firearm.Magazine = this;
        }

        protected virtual void OnDisable()
        {
            TryCancelReload(Firearm.StorageAmmo, out _);
            IsReloading = false;
        }

        public abstract bool TryStartReload(IFirearmStorageAmmo ammo);
        public abstract bool TryUseAmmo(int amount);
        public void ForceSetAmmo(int amount) => AmmoInMagazine = amount;

        public virtual bool TryCancelReload(IFirearmStorageAmmo ammo, out float endDuration)
        {
            endDuration = 0.5f;
            if (!IsReloading)
                return false;

            ammo.AddAmmo(AmmoToLoad);
            IsReloading = false;

            return true;
        }
    }
}
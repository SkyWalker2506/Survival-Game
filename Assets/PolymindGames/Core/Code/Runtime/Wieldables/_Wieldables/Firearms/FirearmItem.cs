using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [RequireComponent(typeof(Firearm))]
    [AddComponentMenu("Polymind Games/Wieldables/Behaviours/Firearm Item")]
    public class FirearmItem : WieldableItem
    {
        private ItemProperty _ammoInMagazineProperty;
        private IFirearmMagazine _magazine;
        private IFirearm _firearm;


        protected override void OnItemChanged(IItem item)
        {
            base.OnItemChanged(item);

            if (_firearm == null)
                Init();

            _ammoInMagazineProperty = null;

            if (item != null)
            {
                // Load the current 'ammo in magazine count' that's saved in one of the properties on the given item.
                if (item.TryGetPropertyWithId(WieldableItemConstants.AMMO_IN_MAGAZINE, out _ammoInMagazineProperty))
                {
                    int ammoInMagazine = _ammoInMagazineProperty.Integer >= 0
                        ? _ammoInMagazineProperty.Integer
                        : _magazine.MagazineSize;

                    _magazine.ForceSetAmmo(ammoInMagazine);
                }
            }
        }

        private void Init()
        {
            _firearm = Wieldable as IFirearm;

            if (_firearm == null)
            {
                Debug.LogError("No firearm found on this object!", gameObject);
                return;
            }

            _magazine = _firearm.Magazine;
            _magazine.AmmoInMagazineChanged += OnAmmoInMagazineChanged;
            _firearm.AddChangedListener(AttachmentType.Reloader, OnReloaderChanged);
        }

        private void OnReloaderChanged()
        {
            _magazine.AmmoInMagazineChanged -= OnAmmoInMagazineChanged;
            _magazine = _firearm.Magazine;
            _magazine.AmmoInMagazineChanged += OnAmmoInMagazineChanged;
        }

        private void OnAmmoInMagazineChanged(int prevAmmo, int ammo)
        {
            if (_ammoInMagazineProperty != null)
                _ammoInMagazineProperty.Integer = ammo;
        }
    }
}
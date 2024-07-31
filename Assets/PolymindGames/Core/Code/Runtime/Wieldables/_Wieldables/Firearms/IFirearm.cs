using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public interface IFirearm : IMonoBehaviour
    {
        ISight Sight { get; set; }
        IFirearmTrigger Trigger { get; set; }
        IFirearmFiringSystem FiringSystem { get; set; }
        IFirearmStorageAmmo StorageAmmo { get; set; }
        IFirearmMagazine Magazine { get; set; }
        IFirearmRecoilStock RecoilStock { get; set; }
        IFirearmProjectileEffect ProjectileEffect { get; set; }
        IFirearmCasingEjector CasingEjector { get; set; }
        IFirearmBarrel Barrel { get; set; }

        void AddChangedListener(AttachmentType type, UnityAction callback);
        void RemoveChangedListener(AttachmentType type, UnityAction callback);
    }

    public enum AttachmentType
    {
        Sight,
        Trigger,
        Shooter,
        Ammo,
        Reloader,
        Recoil,
        ProjectileEffect,
        CasingEjector,
        MuzzleEffect
    }
}
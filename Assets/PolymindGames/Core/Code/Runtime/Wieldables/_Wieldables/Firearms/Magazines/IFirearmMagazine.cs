using System.Runtime.CompilerServices;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    public interface IFirearmMagazine
    {
        bool IsReloading { get; }
        int AmmoInMagazine { get; }
        int MagazineSize { get; }

        /// <summary> Prev ammo, Current ammo </summary>
        event UnityAction<int, int> AmmoInMagazineChanged;

        /// <summary> Ammo to load </summary>
        event UnityAction<int> ReloadStarted;

        bool TryStartReload(IFirearmStorageAmmo ammo);
        bool TryCancelReload(IFirearmStorageAmmo ammo, out float endDuration);
        bool TryUseAmmo(int amount);
        void ForceSetAmmo(int amount);
        void Attach();
        void Detach();
    }

    public static class ReloaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMagazineEmpty(this IFirearmMagazine magazine)
        {
            return magazine.AmmoInMagazine == 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsMagazineFull(this IFirearmMagazine magazine)
        {
            return magazine.AmmoInMagazine != 0 && magazine.AmmoInMagazine == magazine.MagazineSize;
        }
    }

    public sealed class DefaultFirearmMagazine : IFirearmMagazine
    {
        public static readonly DefaultFirearmMagazine Instance = new();

        private DefaultFirearmMagazine() { }
        
        public int AmmoInMagazine => 0;
        public bool IsReloading => false;
        public int MagazineSize => 0;

        public event UnityAction<int, int> AmmoInMagazineChanged
        {
            add { }
            remove { }
        }

        public event UnityAction<int> ReloadStarted
        {
            add { }
            remove { }
        }

        public bool TryStartReload(IFirearmStorageAmmo ammo) => false;
        public bool TryUseAmmo(int amount) => false;
        public void ForceSetAmmo(int amount) { }

        public bool TryCancelReload(IFirearmStorageAmmo ammo, out float endDuration)
        {
            endDuration = 0f;
            return false;
        }

        public void Attach() { }
        public void Detach() { }
    }
}
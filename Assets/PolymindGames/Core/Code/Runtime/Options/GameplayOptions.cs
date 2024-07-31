using UnityEngine;

namespace PolymindGames
{
    [CreateAssetMenu(menuName = OPTIONS_MENU_PATH + "Gameplay Options", fileName = nameof(GameplayOptions))]
    public sealed partial class GameplayOptions : UserOptions<GameplayOptions>
    {
        [SerializeField, BeginGroup("HUD"), EndGroup]
        private Option<Color> _crosshairColor = new(Color.white);

        [SerializeField, BeginGroup("Shoot")]
        private Option<bool> _infiniteMagazineAmmo = new(false);

        [SerializeField]
        private Option<bool> _infiniteStorageAmmo = new(false);

        [SerializeField, EndGroup]
        private Option<bool> _manualCasingEjection = new(false);

        [SerializeField, BeginGroup("Aim"), EndGroup]
        private Option<bool> _canAimWhileReloading = new(false); 

        [SerializeField, BeginGroup("Reload")]
        private Option<bool> _cancelReloadOnShoot = new(false);

        [SerializeField, EndGroup]
        private Option<bool> _autoReloadOnDry = new(true);

        [SerializeField, BeginGroup("Saving"), EndGroup]
        private Option<bool> _autosaveEnabled = new(false);


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init() => CreateInstance();

        public Option<Color> CrosshairColor => _crosshairColor;
        public Option<bool> InfiniteStorageAmmo => _infiniteStorageAmmo;
        public Option<bool> InfiniteMagazineAmmo => _infiniteMagazineAmmo;
        public Option<bool> ManualCasingEjection => _manualCasingEjection;
        public Option<bool> CanAimWhileReloading => _canAimWhileReloading;
        public Option<bool> CancelReloadOnShoot => _cancelReloadOnShoot;
        public Option<bool> AutoReloadOnDry => _autoReloadOnDry;
        public Option<bool> AutosaveEnabled => _autosaveEnabled;
    }
}
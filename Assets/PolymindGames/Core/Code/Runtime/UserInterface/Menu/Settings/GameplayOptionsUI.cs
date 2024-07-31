using UnityEngine;
using UnityEngine.UI;

namespace PolymindGames.UserInterface
{
    public sealed class GameplayOptionsUI : UserOptionsUI<GameplayOptions>
    {
        [SerializeField, BeginGroup("Toggles")]
        private Toggle _autoSave;
        
        [SerializeField]
        private Toggle _cancelReloadOnShoot;
        
        [SerializeField]
        private Toggle _autoReloadOnDry;
        
        [SerializeField]
        private Toggle _canAimWhileReloading; 
        
        [SerializeField, EndGroup]
        private Toggle _manualCasingEjection;

        [SerializeField, BeginGroup("Editor")]
        private Toggle _infiniteMagazineAmmo;

        [SerializeField, EndGroup]
        private Toggle _infiniteStorageAmmo;


        protected override void InitializeWidgets()
        {
            _autoSave.onValueChanged.AddListener(UpdateAutoSave);
            _cancelReloadOnShoot.onValueChanged.AddListener(UpdateCancelReloadOnShoot);
            _autoReloadOnDry.onValueChanged.AddListener(UpdateAutoReloadOnDry);
            _canAimWhileReloading.onValueChanged.AddListener(UpdateCanAimWhileShooting);
            _manualCasingEjection.onValueChanged.AddListener(UpdateManualCasingEjection);
            
#if DEBUG
            if (_infiniteMagazineAmmo != null)
                _infiniteMagazineAmmo.onValueChanged.AddListener(UpdateInfiniteMagazineAmmo);
            
            if (_infiniteStorageAmmo != null)
                _infiniteStorageAmmo.onValueChanged.AddListener(UpdateInfiniteStorageAmmo);
            
#else
            _infiniteStorageAmmo.transform.parent.gameObject.SetActive(false);
            _infiniteMagazineAmmo.transform.parent.gameObject.SetActive(false);
#endif
        }

        protected override void ResetWidgets()
        {
            _autoSave.isOn = Settings.AutosaveEnabled;
            _cancelReloadOnShoot.isOn = Settings.CancelReloadOnShoot;
            _autoReloadOnDry.isOn = Settings.AutoReloadOnDry;
            _canAimWhileReloading.isOn = Settings.CanAimWhileReloading;
            _manualCasingEjection.isOn = Settings.ManualCasingEjection;
            
#if DEBUG
            if (_infiniteMagazineAmmo != null)
                _infiniteMagazineAmmo.isOn = Settings.InfiniteMagazineAmmo;
            
            if (_infiniteStorageAmmo != null)
                _infiniteStorageAmmo.isOn = Settings.InfiniteStorageAmmo;
#endif
        }

        private void UpdateAutoSave(bool value) => Settings.AutosaveEnabled.Value = value;
        private void UpdateCancelReloadOnShoot(bool value) => Settings.CancelReloadOnShoot.Value = value;
        private void UpdateAutoReloadOnDry(bool value) => Settings.AutoReloadOnDry.Value = value;
        private void UpdateCanAimWhileShooting(bool value) => Settings.CanAimWhileReloading.Value = value;
        private void UpdateManualCasingEjection(bool value) => Settings.ManualCasingEjection.Value = value;
        
#if DEBUG
        private void UpdateInfiniteMagazineAmmo(bool value) => Settings.InfiniteMagazineAmmo.Value = value;
        private void UpdateInfiniteStorageAmmo(bool value) => Settings.InfiniteStorageAmmo.Value = value;
#endif
    }
}
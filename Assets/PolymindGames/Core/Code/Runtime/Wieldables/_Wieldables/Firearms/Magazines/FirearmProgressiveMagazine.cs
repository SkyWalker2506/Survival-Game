using System.Collections;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Reloaders/Progressive-Reload Magazine")]
    public class FirearmProgressiveMagazine : FirearmMagazineBehaviour
    {
        [BeginGroup("Tactical Reload")]
        [SerializeField, Range(0f, 10f), Line(Order = 2000)]
        private float _reloadStartDuration = 0.5f;

        [SerializeField, Range(0.1f, 2f)]
        private float _reloadStartAnimSpeed = 1f;

        [SerializeField]
        private AdvancedAudioData _reloadStartAudio;

        [SerializeField, Range(0f, 10f), Line]
        private float _reloadLoopDuration = 0.35f;

        [SerializeField, Range(0.1f, 2f)]
        private float _reloadLoopAnimSpeed = 1f;

        [SerializeField]
        private AdvancedAudioData _reloadLoopAudio;

        [SerializeField, Range(0f, 10f), Line]
        private float _reloadEndDuration = 0.5f;

        [SerializeField, Range(0.1f, 2f)]
        private float _reloadEndAnimSpeed = 1f;

        [SerializeField, EndGroup]
        private AdvancedAudioData _reloadEndAudio;

        [SerializeField, BeginGroup("Empty Reload")]
        private ReloadType _emptyReloadType = ReloadType.Standard;

        [SerializeField, Range(0f, 15f)]
        [HideIf(nameof(_emptyReloadType), ReloadType.None)]
        private float _emptyReloadDuration = 3f;

        [SerializeField, Range(0.1f, 2f)]
        [HideIf(nameof(_emptyReloadType), ReloadType.None)]
        private float _emptyReloadAnimSpeed = 1f;

        [SerializeField, EndGroup]
        [HideIf(nameof(_emptyReloadType), ReloadType.None)]
        private AdvancedAudioData _emptyReloadAudio;


        public override bool TryUseAmmo(int amount)
        {
            if (IsMagazineEmpty || AmmoInMagazine < amount)
                return false;

            AmmoInMagazine -= amount;
            return true;
        }

        public override bool TryCancelReload(IFirearmStorageAmmo ammo, out float endDuration)
        {
            endDuration = _reloadEndDuration;

            if (!IsReloading)
                return false;

            AmmoToLoad = 0;
            IsReloading = false;

            return true;
        }

        public override bool TryStartReload(IFirearmStorageAmmo ammo)
        {
            if (IsReloading || IsMagazineFull)
                return false;

            AmmoToLoad = MagazineSize - AmmoInMagazine;
            int currentInStorage = ammo.GetAmmoCount();

            if (currentInStorage < AmmoToLoad)
                AmmoToLoad = currentInStorage;

            if (!IsMagazineFull && AmmoToLoad > 0)
            {
                IsReloading = true;

                if (IsMagazineEmpty && _emptyReloadType != ReloadType.None)
                    OnEmptyReloadStart(ammo);
                else
                    OnTacticalReloadStart(ammo);

                return true;
            }

            return false;
        }

        protected virtual void OnTacticalReloadStart(IFirearmStorageAmmo ammo) => StartCoroutine(C_ReloadLoop(ammo));
        protected virtual void OnEmptyReloadStart(IFirearmStorageAmmo ammo) => StartCoroutine(C_EmptyReloadLoop(ammo));

        private IEnumerator C_ReloadLoop(IFirearmStorageAmmo ammo)
        {
            Wieldable.Animation.SetTrigger(WieldableAnimationConstants.RELOAD_START);
            Wieldable.Animation.SetFloat(WieldableAnimationConstants.RELOAD_SPEED, _reloadStartAnimSpeed);
            Wieldable.AudioPlayer.Play(_reloadStartAudio);

            // Reload start (e.g. go to reload position)
            float startTimer = Time.time + _reloadStartDuration;
            while (IsReloading && startTimer > Time.time)
                yield return null;

            // Reload loop (e.g. insert shell)
            while (AmmoToLoad > 0 && IsReloading)
            {
                Wieldable.Animation.SetTrigger(WieldableAnimationConstants.RELOAD);
                Wieldable.Animation.SetFloat(WieldableAnimationConstants.RELOAD_SPEED, _reloadLoopAnimSpeed);
                Wieldable.AudioPlayer.Play(_reloadLoopAudio);

                float loopTimer = Time.time + _reloadLoopDuration;
                while (IsReloading && loopTimer > Time.time)
                    yield return null;

                ammo.RemoveAmmo(1);
                AmmoInMagazine++;
                AmmoToLoad--;

                yield return null;
            }

            // Reload end (e.g. come back to idle)
            IsReloading = true;
            Wieldable.Animation.SetTrigger(WieldableAnimationConstants.RELOAD_END);
            Wieldable.Animation.SetFloat(WieldableAnimationConstants.RELOAD_SPEED, _reloadEndAnimSpeed);
            Wieldable.AudioPlayer.Play(_reloadEndAudio);

            float endTimer = Time.time + _reloadEndDuration;
            while (endTimer > Time.time)
                yield return null;

            IsReloading = false;
        }

        private IEnumerator C_EmptyReloadLoop(IFirearmStorageAmmo ammo)
        {
            Wieldable.Animation.SetTrigger(WieldableAnimationConstants.EMPTY_RELOAD);
            Wieldable.Animation.SetFloat(WieldableAnimationConstants.RELOAD_SPEED, _emptyReloadAnimSpeed);
            Wieldable.AudioPlayer.Play(_emptyReloadAudio);

            // Wait for the empty reload animation
            float startTimer = Time.time + _emptyReloadDuration;
            while (IsReloading && startTimer > Time.time)
                yield return null;

            if (_emptyReloadType == ReloadType.Progressive)
            {
                ammo.RemoveAmmo(1);
                AmmoInMagazine++;
                AmmoToLoad--;

                yield return C_ReloadLoop(ammo);
            }
            else
            {
                ammo.RemoveAmmo(AmmoToLoad);
                AmmoInMagazine += AmmoToLoad;
                AmmoToLoad = 0;

                // No need to start the loop since the firearm is fully reloaded
                IsReloading = false;
            }
        }

        #region Internal
        protected enum ReloadType
        {
            None,
            Standard,
            Progressive
        }
        #endregion
    }
}
using System.Collections;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Reloaders/Basic-Reload Magazine")]
    public class FirearmBasicMagazine : FirearmMagazineBehaviour
    {
        [SerializeField, Range(0.1f, 15f), BeginGroup("Tactical Reload")]
        private float _reloadDuration;

        [SerializeField, Range(0.1f, 2f)]
        private float _reloadAnimSpeed = 1f;

        [SerializeField, EndGroup]
        private AdvancedAudioData _reloadAudio = AdvancedAudioData.Default;

        private float _reloadEndTime;


        public override bool TryUseAmmo(int amount)
        {
            if (AmmoInMagazine < amount)
                return false;

            AmmoInMagazine -= amount;
            return true;
        }

        public override bool TryStartReload(IFirearmStorageAmmo ammo)
        {
            if (IsReloading || IsMagazineFull)
                return false;

            AmmoToLoad = ammo.RemoveAmmo(MagazineSize - AmmoInMagazine);

            if (AmmoToLoad > 0)
            {
                Wieldable.Animation.SetTrigger(WieldableAnimationConstants.RELOAD);
                Wieldable.Animation.SetFloat(WieldableAnimationConstants.RELOAD_SPEED, _reloadAnimSpeed);
                Wieldable.AudioPlayer.Play(_reloadAudio);

                StartCoroutine(C_ReloadLoop(_reloadDuration));
                return true;
            }

            return false;
        }

        protected IEnumerator C_ReloadLoop(float duration)
        {
            IsReloading = true;

            _reloadEndTime = Time.time + duration;
            while (IsReloading && _reloadEndTime > Time.time)
                yield return null;

            AmmoInMagazine += AmmoToLoad;
            IsReloading = false;

            OnReloadFinished();
        }

        protected virtual void OnReloadFinished() { }
    }
}
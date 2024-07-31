using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Reloaders/Standard-Reload Magazine")]
    public class FirearmStandardMagazine : FirearmBasicMagazine
    {
        [SerializeField, Range(0f, 15f), BeginGroup("Empty Reload")]
        [Help("Enable the empty reload by setting a duration greater than 0.", UnityMessageType.None, Order = 2000)]
        private float _emptyReloadDuration = 3f;

        [SerializeField, Range(0.1f, 2f)]
        [ShowIf(nameof(_emptyReloadDuration), 0.01f, Comparison = UnityComparisonMethod.GreaterEqual)]
        private float _emptyReloadAnimSpeed = 1f;

        [SerializeField, EndGroup]
        [ShowIf(nameof(_emptyReloadDuration), 0.01f, Comparison = UnityComparisonMethod.GreaterEqual)]
        private AdvancedAudioData _emptyReloadAudio = AdvancedAudioData.Default;
        
        private bool HasEmptyReload => _emptyReloadDuration > 0.01f;


        public override bool TryStartReload(IFirearmStorageAmmo ammo)
        {
            if (IsReloading || IsMagazineFull)
                return false;

            // Tactical Reload
            if (!HasEmptyReload || !IsMagazineEmpty)
                return TacticalReload(ammo);

            return EmptyReload(ammo);
        }

        protected virtual bool TacticalReload(IFirearmStorageAmmo ammo) =>
            base.TryStartReload(ammo);

        protected virtual bool EmptyReload(IFirearmStorageAmmo ammo)
        {
            AmmoToLoad = ammo.RemoveAmmo(MagazineSize - AmmoInMagazine);

            if (AmmoToLoad > 0)
            {
                Wieldable.Animation.SetTrigger(WieldableAnimationConstants.EMPTY_RELOAD);
                Wieldable.Animation.SetFloat(WieldableAnimationConstants.RELOAD_SPEED, _emptyReloadAnimSpeed);
                Wieldable.AudioPlayer.Play(_emptyReloadAudio);

                StartCoroutine(C_ReloadLoop(_emptyReloadDuration));
                return true;
            }

            return false;
        }
    }
}
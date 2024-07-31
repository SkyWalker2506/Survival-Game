using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public abstract class FirearmSightBehaviour : FirearmAttachmentBehaviour, ISight
    {
        [SerializeField, Range(-1, 100), BeginGroup("Settings")]
        private int _aimCrosshairIndex;

        [SerializeField, Range(0f, 1f)]
        private float _aimMovementSpeed = 1f;

        [SerializeField, Range(0f, 1f)]
        private float _aimAccuracy = 1f;

        [SerializeField, Range(0f, 10f), EndGroup]
        private float _aimRecoil = 1f;
        
        
        public bool IsAiming { get; private set; }
        public float AimAccuracyMod => _aimAccuracy;
        
        public virtual bool StartAim()
        {
            if (IsAiming)
                return false;

            Firearm?.RecoilStock.SetRecoilMultiplier(_aimRecoil);

            if (Wieldable is ICrosshairHandler crosshair)
                crosshair.CrosshairIndex = _aimCrosshairIndex;

            if (Wieldable is IMovementSpeedHandler speed)
                speed.SpeedModifier.AddModifier(GetMovementSpeedMultiplier);

            IsAiming = true;

            return true;
        }

        public virtual bool EndAim()
        {
            if (!IsAiming)
                return false;

            Firearm?.RecoilStock.SetRecoilMultiplier(1f);

            if (Wieldable is ICrosshairHandler crosshair)
                crosshair.ResetCrosshair();

            if (Wieldable is IMovementSpeedHandler speed)
                speed.SpeedModifier.RemoveModifier(GetMovementSpeedMultiplier);

            IsAiming = false;

            return true;
        }

        protected virtual void OnEnable()
        {
            if (Firearm != null)
            {
                Firearm.Sight = this;
                Firearm.RecoilStock.SetRecoilMultiplier(1f);
                Firearm.AddChangedListener(AttachmentType.Recoil, RecoilChanged);
            }
            
        }

        protected virtual void OnDisable()
        {
            Firearm?.RemoveChangedListener(AttachmentType.Recoil, RecoilChanged);
            EndAim();
        }

        private void RecoilChanged() => Firearm.RecoilStock.SetRecoilMultiplier(_aimRecoil);
        private float GetMovementSpeedMultiplier() => _aimMovementSpeed;
    }
}
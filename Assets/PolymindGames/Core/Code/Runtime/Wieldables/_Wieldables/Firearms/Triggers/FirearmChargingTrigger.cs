using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Triggers/Charging Trigger")]
    public class FirearmChargingTrigger : FirearmTriggerBehaviour, IChargeHandler
    {
        [SerializeField, Range(0f, 10f), BeginGroup("Charging")]
        [Tooltip("The minimum time that can pass between consecutive shots.")]
        private float _pressCooldown;

        [SerializeField, Range(0f, 0.95f)]
        [Tooltip("Minimum charge needed to shoot")]
        private float _minChargeTime;

        [SerializeField, Range(0f, 10f)]
        private float _maxChargeTime = 1f;

        [SerializeField, EndGroup]
        private AnimationCurve _chargeCurve;

        [SerializeField, Range(0f, 2f), BeginGroup("Field of View")]
        private float _fovSetDuration = 0.4f;

        [SerializeField, Range(0f, 2f)]
        private float _worldFOVMod = 0.75f;

        [SerializeField, Range(0f, 2f), EndGroup]
        private float _overlayFOVMod = 0.75f;

        [SerializeField, BeginGroup("Audio")]
        private AudioData _chargeStartAudio = AudioData.Default;

        [SerializeField]
        private AudioData _chargeCancelAudio = AudioData.Default;

        [SerializeField, EndGroup]
        private AudioData _chargeMaxAudio = AudioData.Default;

        [SerializeField, BeginGroup("Camera")]
        private SimpleBobMotionData _chargeCameraBob;

        [SerializeField, EndGroup]
        private NoiseMotionData _chargeCameraNoise;

        private WieldableFOV _fovHandler;
        private float _triggerChargeStartTime;
        private bool _triggerChargeStarted;
        private float _canHoldTimer;
        private bool _chargeMaxed;


        public override void HoldTrigger()
        {
            if (Time.time < _canHoldTimer)
                return;

            if (Firearm.Magazine.IsReloading || Firearm.Magazine.IsMagazineEmpty())
            {
                RaiseShootEvent(0f);
                return;
            }

            IsTriggerHeld = true;

            if (!_triggerChargeStarted && GetNormalizedCharge() > _minChargeTime)
                OnChargeStart();

            if (!_chargeMaxed && GetNormalizedCharge() > _maxChargeTime - 0.01f)
                OnChargeMaxed();
        }

        public override void ReleaseTrigger()
        {
            if (!IsTriggerHeld)
                return;

            if (Firearm is IUseInputHandler useHandler && useHandler.UseBlocker.IsBlocked)
                OnChargeCancel();
            else
                OnChargeShoot();

            ResetCharge();
        }

        public float GetNormalizedCharge()
        {
            if (!IsTriggerHeld)
                return 0f;

            float normalizedCharge = (Time.time - _triggerChargeStartTime) / _maxChargeTime;
            normalizedCharge = Mathf.Clamp(normalizedCharge, 0.05f, 1f);

            return normalizedCharge;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            ResetCharge();
        }

        protected override void Awake()
        {
            base.Awake();
            _fovHandler = Wieldable.gameObject.GetComponentInFirstChildren<WieldableFOV>();
        }

        private void ResetCharge()
        {
            _triggerChargeStarted = false;
            _chargeMaxed = false;
            _canHoldTimer = Time.time + _pressCooldown;
            IsTriggerHeld = false;
        }

        private void OnChargeStart()
        {
            _fovHandler.SetViewModelFOV(_overlayFOVMod, _fovSetDuration * 1.1f);
            _fovHandler.SetCameraFOV(_worldFOVMod, _fovSetDuration);

            Wieldable.Animation.SetBool(WieldableAnimationConstants.IS_CHARGING, true);
            Wieldable.AudioPlayer.Play(_chargeStartAudio);

            var cameraData = Wieldable.Motion.HeadDataHandler;
            cameraData.SetDataOverride<IBobMotionData>(_chargeCameraBob);
            cameraData.SetDataOverride<NoiseMotionData>(_chargeCameraNoise);

            _triggerChargeStarted = true;
            _triggerChargeStartTime = Time.time;
        }

        private void OnChargeCancel()
        {
            _fovHandler.SetViewModelFOV(1f, _fovSetDuration * 0.9f);
            _fovHandler.SetCameraFOV(1f, _fovSetDuration * 0.9f);

            var cameraData = Wieldable.Motion.HeadDataHandler;
            cameraData.SetDataOverride<IBobMotionData>(_chargeCameraBob);
            cameraData.SetDataOverride<NoiseMotionData>(_chargeCameraNoise);

            Wieldable.Animation.SetBool(WieldableAnimationConstants.IS_CHARGING, false);
            Wieldable.AudioPlayer.Play(_chargeCancelAudio);
        }

        private void OnChargeShoot()
        {
            _fovHandler.SetViewModelFOV(1f, _fovSetDuration * 0.9f);
            _fovHandler.SetCameraFOV(1f, _fovSetDuration * 0.9f);

            var cameraData = Wieldable.Motion.HeadDataHandler;
            cameraData.SetDataOverride<IBobMotionData>(null);
            cameraData.SetDataOverride<NoiseMotionData>(null);

            if (GetNormalizedCharge() >= _minChargeTime)
            {
                float normalizedCharge = GetNormalizedCharge();
                float chargeAmount = normalizedCharge * _chargeCurve.Evaluate(normalizedCharge);

                var animator = Wieldable.Animation;
                animator.SetTrigger(WieldableAnimationConstants.SHOOT);
                animator.SetBool(WieldableAnimationConstants.IS_CHARGING, false);

                RaiseShootEvent(Mathf.Clamp(chargeAmount, 0.3f, 1f));
            }
        }

        private void OnChargeMaxed()
        {
            if (_chargeMaxAudio.IsPlayable)
                Wieldable.AudioPlayer.Play(_chargeMaxAudio);

            Wieldable.Animation.SetTrigger(WieldableAnimationConstants.FULL_CHARGE);
            _chargeMaxed = true;
        }
    }
}
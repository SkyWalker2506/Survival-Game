using PolymindGames.ProceduralMotion;
using PolymindGames.PostProcessing;
using UnityEngine;
using System;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Aimers/Basic Sight")]
    public class FirearmBasicSight : FirearmSightBehaviour
    {
        [SerializeField, Range(0f, 2f), BeginGroup("Field of View")]
        private float _fovSetDuration = 0.4f;

        [SerializeField, Range(0f, 2f)]
        private float _headFOVMod = 0.75f;

        [SerializeField, Range(0f, 2f), EndGroup]
        private float _handsFOVMod = 0.75f;

        [SerializeField, Range(0f, 1f), BeginGroup("Offset")]
        private float _motionMultiplier = 0.25f;

        [SerializeField, EndGroup]
        private AimOffset _motionOffset;

        [SerializeField, Range(0f, 1f), BeginGroup("Camera")]
        private float _cameraMotionMultiplier = 0.9f;

        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false)]
        private MotionData[] _cameraMotions;

        [SerializeField, SpaceArea, EndGroup]
        private VolumeAnimationProfile _cameraEffect;

        [SerializeField, InLineEditor, BeginGroup("Audio")]
        private AudioDataSO _aimStartAudio;

        [SerializeField, InLineEditor, EndGroup]
        private AudioDataSO _aimStopAudio;

        private WieldableFOV _wieldableFOV;
        private float _prevViewOffsetWeight;


        public override bool StartAim()
        {
            if (!base.StartAim())
                return false;

            HandleOffset(true);
            HandleCamera(true);
            HandleFOV(true);

            // Animation
            Wieldable.Animation.SetBool(WieldableAnimationConstants.IS_AIMING, true);

            // Post processing animation
            PostProcessingManager.Instance.TryPlayAnimation(this, _cameraEffect);

            // Audio
            Wieldable.AudioPlayer.PlaySafe(_aimStartAudio);

            return true;
        }

        public override bool EndAim()
        {
            if (!base.EndAim())
                return false;

            HandleOffset(false);
            HandleFOV(false);
            HandleCamera(false);

            // Animation
            Wieldable.Animation.SetBool(WieldableAnimationConstants.IS_AIMING, false);

            // Post processing animation
            PostProcessingManager.Instance.CancelAnimation(this, _cameraEffect);

            // Audio
            Wieldable.AudioPlayer.PlaySafe(_aimStopAudio);

            return true;
        }

        private void HandleOffset(bool aim)
        {
            var motion = Wieldable.Motion;
            var mixer = motion.HandsMotionMixer;
            var dataHandler = motion.HandsDataHandler;

            if (aim)
            {
                mixer.WeightMultiplier = _motionMultiplier;
                dataHandler.SetDataOverride<IOffsetMotionData>(_motionOffset);
                if (mixer.TryGetMotionOfType(out ViewOffsetMotion viewOffset))
                {
                    _prevViewOffsetWeight = viewOffset.Multiplier;
                    viewOffset.Multiplier = 0f;
                }
            }
            else
            {
                dataHandler.SetDataOverride<IOffsetMotionData>(null);
                mixer.WeightMultiplier = 1f;

                if (mixer.TryGetMotionOfType(out ViewOffsetMotion viewOffset))
                    viewOffset.Multiplier = _prevViewOffsetWeight;
            }

            mixer.GetMotionOfType<OffsetMotion>().IgnoreParentMultiplier = aim;
        }

        private void HandleCamera(bool aim)
        {
            var motion = Wieldable.Motion;
            var dataHandler = motion.HeadDataHandler;
            var mixer = motion.HeadMotionMixer;

            mixer.WeightMultiplier = aim ? _cameraMotionMultiplier : 1f;
            foreach (var motionData in _cameraMotions)
                dataHandler.SetDataOverride(motionData, aim);
        }

        private void HandleFOV(bool aim)
        {
            if (_wieldableFOV == null)
                return;

            var fovParams = GetFieldOfViewParams(aim);
            _wieldableFOV.SetViewModelFOV(fovParams.HandsMultiplier, fovParams.SetDuration, fovParams.HandsDelay);
            _wieldableFOV.SetCameraFOV(fovParams.HeadMultiplier, fovParams.SetDuration, fovParams.HeadDelay);
        }

        protected virtual FieldOfViewParams GetFieldOfViewParams(bool aim)
        {
            return aim ?
                new FieldOfViewParams(_fovSetDuration, 0f, _handsFOVMod, 0f, _headFOVMod)
                : new FieldOfViewParams(_fovSetDuration * 0.9f, 0f, 1f, 0f, 1f);
        }

        protected override void Awake()
        {
            base.Awake();
            _wieldableFOV = Wieldable.gameObject.GetComponentInFirstChildren<WieldableFOV>();
        }

        #region Internal
        [Serializable]
        public sealed class AimOffset : IOffsetMotionData
        {
            [SerializeField]
            private SpringSettings _positionSpring = SpringSettings.Default;

            [SerializeField]
            private SpringSettings _rotationSpring = SpringSettings.Default;

            [SpaceArea]
            [SerializeField]
            private SpringForce3D _enterForce;

            [SerializeField]
            private SpringForce3D _exitForce;

            [SpaceArea]
            [SerializeField]
            private Vector3 _positionOffset;

            [SerializeField]
            private Vector3 _rotationOffset;
            
            
            public SpringSettings PositionSettings => _positionSpring;
            public SpringSettings RotationSettings => _rotationSpring;
            public SpringForce3D EnterForce => _enterForce;
            public SpringForce3D ExitForce => _exitForce;
            public Vector3 PositionOffset => _positionOffset;
            public Vector3 RotationOffset => _rotationOffset;
        }

        protected readonly struct FieldOfViewParams
        {
            public readonly float SetDuration;
            public readonly float HandsDelay;
            public readonly float HandsMultiplier;
            public readonly float HeadDelay;
            public readonly float HeadMultiplier;

            public FieldOfViewParams(float setDuration, float handsDelay, float handsMultiplier, float headDelay, float headMultiplier)
            {
                SetDuration = setDuration;
                HandsDelay = handsDelay;
                HandsMultiplier = handsMultiplier;
                HeadDelay = headDelay;
                HeadMultiplier = headMultiplier;
            }
        }
        #endregion
    }
}
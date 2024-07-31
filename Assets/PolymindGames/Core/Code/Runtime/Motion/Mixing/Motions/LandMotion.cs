using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Polymind Games/Motion/Land Motion")]
    [RequireCharacterComponent(typeof(IMotorCC))]
    public sealed class LandMotion : DataMotionBehaviour<CurvesMotionData>
    {
        [BeginGroup("Settings")]
        [SerializeField, Range(1f, 100f)]
        private float _minLandSpeed = 4f;

        [SerializeField, Range(0f, 100f), EndGroup]
        private float _maxLandSpeed = 11f;

        private float _currentFallTime;
        private float _landSpeedFactor;

        private const float POSITION_FORCE_MOD = 0.02f;
        private const float ROTATION_FORCE_MOD = 5f;


        protected override void OnBehaviourEnable(ICharacter character)
        {
            base.OnBehaviourEnable(character);
            character.GetCC<IMotorCC>().FallImpact += OnFallImpact;
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            base.OnBehaviourDisable(character);
            character.GetCC<IMotorCC>().FallImpact -= OnFallImpact;
        }

        protected override CurvesMotionData GetDataFromPreset(IMotionDataHandler dataHandler)
        {
            return dataHandler.TryGetData<GeneralMotionData>(out var data)
                ? data.Land
                : null;
        }

        protected override void OnDataChanged(CurvesMotionData data)
        {
            if (data != null)
            {
                PositionSpring.Adjust(data.PositionSettings);
                RotationSpring.Adjust(data.RotationSettings);
            }
        }

        public override void UpdateMotion(float deltaTime)
        {
            if (Data == null || _landSpeedFactor <= 0f)
            {
                _currentFallTime = 100f;
                return;
            }

            bool playPosLand = Data.PositionCurves.Duration > _currentFallTime;
            if (playPosLand)
            {
                // Evaluate position landing curves.
                Vector3 posLand = Data.PositionCurves.Evaluate(_currentFallTime) * _landSpeedFactor;
                posLand = MotionMixer.TargetTransform.InverseTransformVector(posLand);
                SetTargetPosition(posLand, POSITION_FORCE_MOD);
            }

            bool playRotLand = Data.RotationCurves.Duration > _currentFallTime;
            if (playRotLand)
            {
                // Evaluate rotation landing curves.
                Vector3 rotLand = Data.RotationCurves.Evaluate(_currentFallTime) * _landSpeedFactor;
                SetTargetRotation(rotLand, ROTATION_FORCE_MOD);
            }

            _currentFallTime += deltaTime;

            if (!playPosLand && !playRotLand)
            {
                _landSpeedFactor = -1f;
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
            }
        }

        private void OnFallImpact(float landSpeed)
        {
            float impactVelocityAbs = Mathf.Abs(landSpeed);

            if (impactVelocityAbs > _minLandSpeed)
            {
                _currentFallTime = 0f;
                _landSpeedFactor = Mathf.Clamp01(impactVelocityAbs / _maxLandSpeed);
            }
        }
    }
}
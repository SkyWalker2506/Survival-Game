using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Polymind Games/Motion/Jump Motion")]
    [RequireCharacterComponent(typeof(IMovementControllerCC))]
    public sealed class JumpMotion : DataMotionBehaviour<CurvesMotionData>
    {
        private float _currentJumpTime;
        private bool _playJumpAnim;
        private float _randomFactor = -1f;

        private const float POSITION_FORCE_MOD = 0.02f;
        private const float ROTATION_FORCE_MOD = 5f;


        protected override void OnBehaviourEnable(ICharacter character)
        {
            character.GetCC<IMovementControllerCC>()
                .AddStateTransitionListener(MovementStateType.Jump, OnJump);
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            character.GetCC<IMovementControllerCC>()
                .RemoveStateTransitionListener(MovementStateType.Jump, OnJump);
        }

        protected override CurvesMotionData GetDataFromPreset(IMotionDataHandler dataHandler)
        {
            return dataHandler.TryGetData<GeneralMotionData>(out var data)
                ? data.Jump
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
            if (Data == null || !_playJumpAnim)
                return;

            bool playPosJump = Data.PositionCurves.Duration > _currentJumpTime;
            if (playPosJump)
            {
                // Evaluate position jumping curves.
                Vector3 posJump = Data.PositionCurves.Evaluate(_currentJumpTime);
                posJump = MotionMixer.TargetTransform.InverseTransformVector(posJump);

                posJump = new Vector3(POSITION_FORCE_MOD * posJump.x * _randomFactor,
                    posJump.y * POSITION_FORCE_MOD,
                    posJump.z * POSITION_FORCE_MOD);

                SetTargetPosition(posJump);
            }

            bool playRotJump = Data.RotationCurves.Duration > _currentJumpTime;
            if (playRotJump)
            {
                // Evaluate rotation jumping curves.
                Vector3 rotJump = Data.RotationCurves.Evaluate(_currentJumpTime);

                rotJump = new Vector3(rotJump.x * ROTATION_FORCE_MOD,
                    rotJump.y * ROTATION_FORCE_MOD * _randomFactor,
                    rotJump.z * ROTATION_FORCE_MOD * _randomFactor);

                SetTargetRotation(rotJump);
            }
            _currentJumpTime += deltaTime;

            if (!playPosJump && !playRotJump)
            {
                _playJumpAnim = false;
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
            }
        }

        private void OnJump(MovementStateType state)
        {
            _currentJumpTime = 0f;
            _playJumpAnim = true;
            _randomFactor = _randomFactor > 0f ? -1f : 1f;

            UpdateMotion(Time.deltaTime);
        }
    }
}
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [AddComponentMenu("Polymind Games/Motion/Fall Motion")]
    [RequireCharacterComponent(typeof(IMotorCC))]
    public sealed class FallMotion : DataMotionBehaviour<SingleValueMotionData>
    {
        [BeginGroup, EndGroup]
        [SerializeField, Range(0f, 100f)]
        private float _fallSpeedLimit = 10f;

        private IMotorCC _motor;

        private const float POSITION_FORCE_MOD = 0.02f;
        private const float ROTATION_FORCE_MOD = 2f;


        protected override void OnBehaviourStart(ICharacter character)
        {
            _motor = character.GetCC<IMotorCC>();
        }

        protected override SingleValueMotionData GetDataFromPreset(IMotionDataHandler dataHandler)
        {
            return dataHandler.TryGetData<GeneralMotionData>(out var data)
                ? data.Fall
                : null;
        }

        protected override void OnDataChanged(SingleValueMotionData data)
        {
            if (data != null)
            {
                PositionSpring.Adjust(data.PositionSettings);
                RotationSpring.Adjust(data.RotationSettings);
            }
        }

        public override void UpdateMotion(float deltaTime)
        {
            if (Data == null || (_motor.IsGrounded && RotationSpring.IsIdle && PositionSpring.IsIdle))
                return;

            float factor = Mathf.Max(_motor.Velocity.y, -_fallSpeedLimit);

            Vector3 posFall = new Vector3(0f, factor * Data.PositionValue * POSITION_FORCE_MOD, 0f);
            Vector3 rotFall = new Vector3(factor * Data.RotationValue * ROTATION_FORCE_MOD, 0f, 0f);

            SetTargetPosition(posFall);
            SetTargetRotation(rotFall);
        }
    }
}
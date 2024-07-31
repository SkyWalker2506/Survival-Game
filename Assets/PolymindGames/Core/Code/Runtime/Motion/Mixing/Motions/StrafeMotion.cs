using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Polymind Games/Motion/Strafe Motion")]
    [RequireCharacterComponent(typeof(IMotorCC))]
    public sealed class StrafeMotion : DataMotionBehaviour<SwayMotionData>
    {
        private IMotorCC _motor;

        private const float POSITION_FORCE_MOD = 0.01f;


        protected override void OnBehaviourStart(ICharacter character)
        {
            _motor = character.GetCC<IMotorCC>();
        }

        protected override SwayMotionData GetDataFromPreset(IMotionDataHandler dataHandler)
        {
            return dataHandler.TryGetData<GeneralMotionData>(out var data)
                ? data.Strafe
                : null;
        }

        protected override void OnDataChanged(SwayMotionData data)
        {
            if (data != null)
            {
                PositionSpring.Adjust(data.PositionSettings);
                RotationSpring.Adjust(data.RotationSettings);
            }
        }

        public override void UpdateMotion(float deltaTime)
        {
            if (Data == null || PositionSpring.IsIdle && RotationSpring.IsIdle && _motor.Velocity == Vector3.zero)
                return;

            // Calculate the strafe input.
            Vector3 strafeInput = transform.InverseTransformVector(_motor.Velocity);
            strafeInput = Vector3.ClampMagnitude(strafeInput, Data.MaxSwayLength);

            // Calculate the strafe position sway.
            Vector3 posSway = new()
            {
                x = strafeInput.x * Data.PositionSway.x * POSITION_FORCE_MOD,
                y = -Mathf.Abs(strafeInput.x * Data.PositionSway.y) * POSITION_FORCE_MOD,
                z = -strafeInput.z * Data.PositionSway.z * POSITION_FORCE_MOD
            };

            // Calculate the strafe rotation sway.
            Vector3 rotSway = new()
            {
                x = strafeInput.z * Data.RotationSway.x,
                y = -strafeInput.x * Data.RotationSway.y,
                z = strafeInput.x * Data.RotationSway.z
            };

            SetTargetPosition(posSway);
            SetTargetRotation(rotSway);
        }
    }
}
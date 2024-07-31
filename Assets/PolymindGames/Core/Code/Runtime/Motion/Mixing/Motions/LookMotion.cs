using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Polymind Games/Motion/Look Motion")]
    [RequireCharacterComponent(typeof(ILookHandlerCC))]
    public sealed class LookMotion : DataMotionBehaviour<SwayMotionData>
    {
        private ILookHandlerCC _lookHandler;

        private const float POSITION_FORCE_MOD = 0.02f;


        protected override void OnBehaviourStart(ICharacter character)
        {
            _lookHandler = character.GetCC<ILookHandlerCC>();
        }

        protected override SwayMotionData GetDataFromPreset(IMotionDataHandler dataHandler)
        {
            return dataHandler.TryGetData<GeneralMotionData>(out var data)
                ? data.Look
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
            if (Data == null)
                return;

            // Calculate the look input.
            Vector2 lookInput = _lookHandler.LookInput;
            lookInput = Vector2.ClampMagnitude(lookInput, Data.MaxSwayLength);

            Vector3 posSway = new(
                lookInput.y * Data.PositionSway.x * POSITION_FORCE_MOD,
                lookInput.x * Data.PositionSway.y * -POSITION_FORCE_MOD);

            Vector3 rotSway = new(
                lookInput.x * Data.RotationSway.x,
                lookInput.y * -Data.RotationSway.y,
                lookInput.y * -Data.RotationSway.z);

            SetTargetPosition(posSway);
            SetTargetRotation(rotSway);
        }
    }
}
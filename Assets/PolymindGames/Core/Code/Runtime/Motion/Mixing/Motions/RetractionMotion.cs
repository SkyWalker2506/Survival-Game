using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Polymind Games/Motion/Retraction Motion")]
    [RequireCharacterComponent(typeof(IWieldableRetractionHandlerCC))]
    public sealed class RetractionMotion : DataMotionBehaviour<RetractionMotionData>
    {
        private IWieldableRetractionHandlerCC _retractionHandler;

        private const float POSITION_FORCE_MOD = 0.02f;
        
        
        public float RetractionFactor { get; private set; }
        public bool IsRetracted { get; private set; }

        protected override void OnBehaviourStart(ICharacter character)
        {
            _retractionHandler = character.GetCC<IWieldableRetractionHandlerCC>();
            IgnoreParentMultiplier = true;
        }

        protected override void OnDataChanged(RetractionMotionData data)
        {
            if (data != null)
            {
                PositionSpring.Adjust(data.PositionSettings);
                RotationSpring.Adjust(data.RotationSettings);
            }
        }

        public override void UpdateMotion(float deltaTime)
        {
            IsRetracted = Data != null && _retractionHandler.ClosestObjectDistance < Data.RetractionDistance;

            if (!IsRetracted && PositionSpring.IsIdle && RotationSpring.IsIdle)
                return;

            if (IsRetracted)
            {
                RetractionFactor = (Data.RetractionDistance - _retractionHandler.ClosestObjectDistance) / Data.RetractionDistance;

                Vector3 posRetraction = Data.PositionOffset * (RetractionFactor * POSITION_FORCE_MOD);
                Vector3 rotRetraction = Data.RotationOffset * RetractionFactor;

                SetTargetPosition(posRetraction);
                SetTargetRotation(rotRetraction);
            }
            else
            {
                RetractionFactor = 0f;
                SetTargetPosition(Vector3.zero);
                SetTargetRotation(Vector3.zero);
            }
        }
    }
}
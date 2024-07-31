using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames
{
    public interface IMotorCC : ICharacterComponent
    {
        bool IsGrounded { get; }
        float LastGroundedChangeTime { get; }
        float Gravity { get; }
        Vector3 Velocity { get; }
        Vector3 SimulatedVelocity { get; }
        Vector3 GroundNormal { get; }
        float TurnSpeed { get; }
        float GroundSurfaceAngle { get; }
        CollisionFlags CollisionFlags { get; }
        LayerMask CollisionMask { get; }
        float DefaultHeight { get; }
        float SlopeLimit { get; }
        float Height { get; set; }
        float Radius { get; }


        event UnityAction Teleported;
        event UnityAction<bool> GroundedChanged;
        event UnityAction<float> FallImpact;
        event UnityAction<float> HeightChanged;

        bool CanSetHeight(float height);

        void ResetMotor();
        float GetSlopeSpeedMultiplier();
        void Teleport(Vector3 position, Quaternion rotation, bool resetMotor = false);
        void AddForce(Vector3 force, ForceMode mode, bool snapToGround = false);

        /// <summary>
        /// A method that will be called when the character motor needs input. 
        /// </summary>
        void SetMotionInput(MotionInputCallback motionInput);
    }

    /// <summary>
    /// A delegate that will be called when the character motor needs input.
    /// </summary>
    public delegate Vector3 MotionInputCallback(Vector3 velocity, out bool useGravity, out bool snapToGround);

    public static class CharacterMotorExtensions
    {
        public static bool Has(this CollisionFlags thisFlags, CollisionFlags flag)
        {
            return (thisFlags & flag) == flag;
        }

        public static bool Raycast(this IMotorCC motor, Ray ray, float distance)
        {
            return PhysicsUtils.RaycastOptimized(ray, distance, out _, motor.CollisionMask);
        }

        public static bool Raycast(this IMotorCC motor, Ray ray, float distance, out RaycastHit raycastHit)
        {
            return PhysicsUtils.RaycastOptimized(ray, distance, out raycastHit, motor.CollisionMask);
        }

        public static bool SphereCast(this IMotorCC motor, Ray ray, float distance, float radius)
        {
            return PhysicsUtils.SphereCastOptimized(ray, radius, distance, out _, motor.CollisionMask);
        }
    }
}
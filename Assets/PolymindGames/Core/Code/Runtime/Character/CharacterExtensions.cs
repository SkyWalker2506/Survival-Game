using UnityEngine;

namespace PolymindGames
{
    public static class CharacterExtensions
    {
        private const float INTERPOLATION_DISABLE_DELAY = 1f;
        private const float ROTATION_RANDOMNESS = 0.1f;
        
        public static Vector3 GetVelocity(this ICharacter character) =>
            character.TryGetCC(out IMotorCC motor) ? motor.Velocity : Vector3.zero;

        public static void ThrowObject(this ICharacter character, Rigidbody rigidbody, Vector3 throwForce, float throwTorque)
        {
            Vector3 inheritedVelocity = character.GetVelocity();
            inheritedVelocity = new Vector3(inheritedVelocity.x, Mathf.Abs(inheritedVelocity.y), inheritedVelocity.z);

            Vector3 forceVector = throwForce + inheritedVelocity;
            Vector3 torqueVector = Random.rotation.eulerAngles.normalized * throwTorque;

            rigidbody.AddForce(forceVector, ForceMode.Impulse);
            rigidbody.AddTorque(torqueVector, ForceMode.Impulse);
        }

        public static void DropObject(this ICharacter character, MonoBehaviour component, Transform dropPoint, float dropForce)
        {
            bool isFacingObstacle = IsFacingObstacle(dropPoint);
            Vector3 dropPosition = CalculateDropPosition(dropPoint, isFacingObstacle);
            Quaternion dropRotation = CalculateDropRotation(dropPoint);

            if (component.TryGetComponent(out Rigidbody rigidbody))
            {
                SetupDroppedRigidbody(character, component, rigidbody, dropPoint, dropForce);
                rigidbody.position = dropPosition;
                rigidbody.rotation = dropRotation;
            }
            else
                component.transform.SetPositionAndRotation(dropPosition, dropRotation);

            static bool IsFacingObstacle(Transform dropPoint)
            {
                var ray = new Ray(dropPoint.position, dropPoint.forward);
                return PhysicsUtils.SphereCastOptimized(ray, 0.5f, 0.5f, LayerConstants.SIMPLE_SOLID_OBJECTS_MASK);
            }
            
            static Vector3 CalculateDropPosition(Transform dropPoint, bool isFacingObstacle)
            {
                Vector3 dropPosition = dropPoint.position;

                if (isFacingObstacle)
                {
                    var characterPosition = dropPoint.root.position;
                    dropPosition = new Vector3(characterPosition.x, dropPosition.y, characterPosition.z);
                }

                return dropPosition;
            }

            static Quaternion CalculateDropRotation(Transform dropPoint)
            {
                Quaternion r1 = Quaternion.LookRotation(dropPoint.forward);
                Quaternion r2 = Random.rotationUniform;
                return Quaternion.Lerp(r1, r2, ROTATION_RANDOMNESS);
            }
            
            static void SetupDroppedRigidbody(ICharacter character, MonoBehaviour component, Rigidbody rigidbody, Transform dropPoint, float dropForce)
            {
                Vector3 inheritedVelocity = character.GetVelocity();
                inheritedVelocity = new Vector3(inheritedVelocity.x, Mathf.Abs(inheritedVelocity.y), inheritedVelocity.z);
                float mass = rigidbody.mass;

                Vector3 forceVector = dropPoint.forward * (dropForce * mass * 0.75f) + inheritedVelocity;
                Vector3 torqueVector = Random.rotation.eulerAngles.normalized * (dropForce / mass);

                rigidbody.AddForce(forceVector, ForceMode.Impulse);
                rigidbody.angularVelocity = torqueVector;

                if (rigidbody.interpolation == RigidbodyInterpolation.None)
                {
                    rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                    CoroutineUtils.InvokeDelayed(component, DisableInterpolation, rigidbody, INTERPOLATION_DISABLE_DELAY);

                    static void DisableInterpolation(Rigidbody rigidB) => rigidB.interpolation = RigidbodyInterpolation.None;
                }
            }
        }
    }
}
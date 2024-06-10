using UnityEngine;

namespace Raymarcher.ExampleContent
{
    public sealed class RMSample_MoveForward : MonoBehaviour
    {
        [SerializeField] private float moveForwardSpeed = 0.4f;
        [SerializeField] private float addRotationSpeed = 2;

        private void Update()
        {
            transform.position += transform.forward * moveForwardSpeed * Time.deltaTime;
            transform.Rotate(0, 0, addRotationSpeed * Time.deltaTime, Space.Self);
        }
    }
}
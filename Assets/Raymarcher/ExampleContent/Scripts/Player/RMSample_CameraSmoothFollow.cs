using UnityEngine;

namespace Raymarcher.ExampleContent
{
    public sealed class RMSample_CameraSmoothFollow : MonoBehaviour
    {
        public Transform target;
        public float smoothing = 16.0f;

        private void LateUpdate()
            => transform.SetPositionAndRotation(Vector3.Lerp(transform.position, target.position, Time.deltaTime * smoothing), target.rotation);
    }
}
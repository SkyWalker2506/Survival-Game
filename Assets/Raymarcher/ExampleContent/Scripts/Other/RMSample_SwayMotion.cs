using UnityEngine;

namespace Raymarcher.ExampleContent
{
    public sealed class RMSample_SwayMotion : MonoBehaviour
    {
        [SerializeField] private Transform target = null;
        [SerializeField] private float swayFrequency = 0.5f;
        [SerializeField] private float swayAmount = 2;
        [SerializeField] private Vector3 swayDirection = Vector3.up;

#if UNITY_EDITOR
        private void Reset()
        {
            target = transform;
        }
#endif

        private void Update()
            => target.localPosition = swayDirection * (Mathf.Sin(Time.time * swayFrequency) * swayAmount);
    }
}
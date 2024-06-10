using UnityEngine;

using Raymarcher.Attributes;

namespace Raymarcher.ExampleContent
{
    using static RMAttributes;

    public sealed class RMSample_CameraController : MonoBehaviour
    {
        public enum CameraType { ThirdPerson, FirstPerson, Orbital};
        [Space]
        public CameraType cameraType = CameraType.FirstPerson;
        [Space]
        public float cameraMoveSpeed = 8;
        public float smoothMovementSpeed = 3;
        public float smoothRotationSpeed = 4;
        [Space]
        [ShowIf("cameraType", 0, false)]
        public ThirdPersonSettings thirdPersonSettings;

        [System.Serializable]
        public sealed class ThirdPersonSettings
        {
            public Transform targetSource;
            public bool attachMovement = true;
            public bool attachRotation = true;
            public Vector3 attachMovementOffset = new Vector3(0, 0, 5);
        }

        [ShowIf("cameraType", 1, false)]
        public FirstPersonSettings firstPersonSettings;

        [System.Serializable]
        public sealed class FirstPersonSettings
        {
            public enum RotationAxis { MouseXAndY = 0, MouseX = 1, MouseY = 2 }
            public RotationAxis rotationAxis = RotationAxis.MouseXAndY;
            public bool flyingCam = true;
            [Space]
            public float sensitivityX = 8F;
            public float sensitivityY = 8F;
            [Space]
            public float minimumX = -360F;
            public float maximumX = 360F;
            [Space]
            public float minimumY = -75F;
            public float maximumY = 75F;

            [HideInInspector] public float rotationY = 0F;
        }

        [ShowIf("cameraType", 2, false)]
        public OrbitalSettings orbitalSettings;

        [System.Serializable]
        public sealed class OrbitalSettings
        {
            public Transform orbitalTarget;

            public bool controlOrbitMovementByKey = false;
            public KeyCode keyControl = KeyCode.Mouse0;

            public float distance = 5.0f;
            public float xSpeed = 40f;
            public float ySpeed = 120.0f;

            public float yMinLimit = -20f;
            public float yMaxLimit = 80f;

            public float distanceMin = .5f;
            public float distanceMax = 15f;

            [HideInInspector] public float x = 0.0f;
            [HideInInspector] public float y = 0.0f;

            public bool ignoreObstacles = true;
        }

        private GameObject camHelper;

        private void Awake()
        {
            Vector3 angles = transform.eulerAngles;
            orbitalSettings.x = angles.y;
            orbitalSettings.y = angles.x;
            camHelper = new GameObject(name + "_CamHelper");
        }

        private void Update()
        {
            if (cameraType == CameraType.FirstPerson)
            {
                if (firstPersonSettings.flyingCam)
                {
                    Vector3 pos = new Vector3(Input.GetAxis("Horizontal"), Input.GetKey(KeyCode.Space) ? 1 : Input.GetKey(KeyCode.C) ? -1 : 0, Input.GetAxis("Vertical"));
                    pos = transform.TransformDirection(pos);
                    transform.position += pos * (Time.deltaTime * cameraMoveSpeed);
                }

                if (firstPersonSettings.rotationAxis == FirstPersonSettings.RotationAxis.MouseXAndY)
                {
                    float rotationX = camHelper.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * firstPersonSettings.sensitivityX;

                    firstPersonSettings.rotationY += Input.GetAxis("Mouse Y") * firstPersonSettings.sensitivityY;
                    firstPersonSettings.rotationY = Mathf.Clamp(firstPersonSettings.rotationY, firstPersonSettings.minimumY, firstPersonSettings.maximumY);

                    camHelper.transform.localEulerAngles = new Vector3(-firstPersonSettings.rotationY, rotationX, 0);

                    transform.localRotation = Quaternion.Lerp(transform.localRotation, camHelper.transform.localRotation, smoothRotationSpeed * Time.deltaTime);
                }
                else if (firstPersonSettings.rotationAxis == FirstPersonSettings.RotationAxis.MouseX)
                {
                    camHelper.transform.Rotate(0, Input.GetAxis("Mouse X") * firstPersonSettings.sensitivityX, 0);

                    transform.localRotation = Quaternion.Lerp(transform.localRotation, camHelper.transform.localRotation, smoothRotationSpeed * Time.deltaTime);
                }
                else
                {
                    firstPersonSettings.rotationY += Input.GetAxis("Mouse Y") * firstPersonSettings.sensitivityY;
                    firstPersonSettings.rotationY = Mathf.Clamp(firstPersonSettings.rotationY, firstPersonSettings.minimumY, firstPersonSettings.maximumY);

                    camHelper.transform.localEulerAngles = new Vector3(-firstPersonSettings.rotationY, camHelper.transform.localEulerAngles.y, 0);

                    transform.localRotation = Quaternion.Lerp(transform.localRotation, camHelper.transform.localRotation, smoothRotationSpeed * Time.deltaTime);
                }
            }

            if (cameraType == CameraType.ThirdPerson)
            {
                if (!thirdPersonSettings.targetSource)
                    return;

                if (thirdPersonSettings.attachMovement)
                    transform.position = Vector3.Slerp(transform.position, thirdPersonSettings.targetSource.position + thirdPersonSettings.attachMovementOffset, smoothMovementSpeed * Time.deltaTime);
                if (thirdPersonSettings.attachRotation)
                    transform.rotation = Quaternion.Lerp(transform.rotation, thirdPersonSettings.targetSource.rotation, smoothRotationSpeed * Time.deltaTime);
            }

            if (cameraType == CameraType.Orbital)
            {
                if (!orbitalSettings.orbitalTarget)
                    return;

                bool passed = true;
                if (orbitalSettings.controlOrbitMovementByKey)
                    passed = Input.GetKey(orbitalSettings.keyControl);
                if (passed)
                {
                    orbitalSettings.x += Input.GetAxis("Mouse X") * orbitalSettings.xSpeed * orbitalSettings.distance * 0.02f;
                    orbitalSettings.y -= Input.GetAxis("Mouse Y") * orbitalSettings.ySpeed * 0.02f;
                }

                orbitalSettings.y = ClampAngle(orbitalSettings.y, orbitalSettings.yMinLimit, orbitalSettings.yMaxLimit);

                Quaternion rotation = Quaternion.Euler(orbitalSettings.y, orbitalSettings.x, 0);

                orbitalSettings.distance = Mathf.Clamp(orbitalSettings.distance - Input.GetAxis("Mouse ScrollWheel") * 5, orbitalSettings.distanceMin, orbitalSettings.distanceMax);

                if (!orbitalSettings.ignoreObstacles)
                {
                    if (Physics.Linecast(orbitalSettings.orbitalTarget.position, camHelper.transform.position, out RaycastHit hit))
                        orbitalSettings.distance -= hit.distance;
                }
                Vector3 negDistance = new Vector3(0.0f, 0.0f, -orbitalSettings.distance);
                Vector3 position = rotation * negDistance + orbitalSettings.orbitalTarget.position;

                camHelper.transform.SetPositionAndRotation(position, rotation);

                transform.SetPositionAndRotation(
                    Vector3.Slerp(transform.position, camHelper.transform.position, smoothMovementSpeed * Time.deltaTime), 
                    Quaternion.Lerp(transform.rotation, camHelper.transform.rotation, smoothRotationSpeed * Time.deltaTime));
            }
        }

        private static float ClampAngle(float angle, float min, float max)
        {
            if (angle < -360F)
                angle += 360F;
            if (angle > 360F)
                angle -= 360F;
            return Mathf.Clamp(angle, min, max);
        }
    }
}

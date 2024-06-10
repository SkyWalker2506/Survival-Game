using UnityEngine;

using Raymarcher.Toolkit;

namespace Raymarcher.ExampleContent
{
    public sealed class RMSample_VolumeCharacterControllerInput : MonoBehaviour
    {
        [SerializeField] private RMVolumeCharacterController volCharacterController;

        private const string AXIS_VERTICAL = "Vertical";
        private const string AXIS_HORIZONTAL = "Horizontal";

#if UNITY_EDITOR
        private void Reset()
        {
            volCharacterController = GetComponent<RMVolumeCharacterController>();
        }
#endif

        private void Update()
        {
            if (volCharacterController == null)
                return;

            // We need to move the target volume character controller - similarly to regular character controller, just call MoveCharacter

            Vector3 moveDir = new Vector3(Input.GetAxis(AXIS_HORIZONTAL), 0, Input.GetAxis(AXIS_VERTICAL));
            volCharacterController.MoveCharacter(transform.TransformDirection(moveDir));

            if (Input.GetKeyDown(KeyCode.Space))
                volCharacterController.JumpCharacter();

            // You can also limit the position of your character by just calling 'volCharacterController.transform.position'...
        }
    }
}
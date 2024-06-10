using UnityEngine;

namespace Raymarcher.ExampleContent
{
    public sealed class RMSample_CursorHandler : MonoBehaviour
    {
        [SerializeField] private bool showCursor = false;

        private void Start()
        {
            ChangeCursorVisibility(showCursor);
        }

        public static void ChangeCursorVisibility(bool show)
        {
            Cursor.visible = show;
            Cursor.lockState = !show ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
}
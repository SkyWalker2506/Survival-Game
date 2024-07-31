using UnityEngine;

namespace PolymindGames.Demo
{
    public sealed class Billboard : MonoBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        private bool _syncXRotation;


        private void LateUpdate()
        {
            if (UnityUtils.CachedMainCamera == null)
                return;

            Quaternion rot = Quaternion.LookRotation(transform.position - UnityUtils.CachedMainCamera.transform.position);

            if (!_syncXRotation)
                rot = Quaternion.Euler(0f, rot.eulerAngles.y, 0f);

            transform.rotation = rot;
        }
    }
}
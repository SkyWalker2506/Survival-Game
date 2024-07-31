using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public sealed class CustomSurfaceImpactHandler : MonoBehaviour
    {
        [SerializeField, Range(0f, 2f), BeginGroup]
        private float _volumeMultiplier = 1f;

        [SerializeField, NotNull, EndGroup]
        private AudioDataSO _impactAudio;

        private bool _hasCollided;


        private void OnEnable() => _hasCollided = false;

        private void OnCollisionEnter(Collision collision)
        {
            if (_hasCollided)
                return;
            
            AudioManager.Instance.PlayClipAtPoint(_impactAudio.Clip, collision.GetContact(0).point, _impactAudio.Volume * _volumeMultiplier, _impactAudio.Pitch);
            _hasCollided = true;
        }
    }
}
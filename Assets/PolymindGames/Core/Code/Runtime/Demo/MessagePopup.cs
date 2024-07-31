using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames.Demo
{
    public sealed class MessagePopup : MonoBehaviour
    {
        [SerializeField, BeginGroup("Display")]
        private Color _messageSeenIconColor;

        [SerializeField, EndGroup]
        private SpriteRenderer _messageIcon;

        [SerializeField, BeginGroup("Animation")]
        private TweenSequence _showAnimation;

        [SerializeField, EndGroup]
        private TweenSequence _hideAnimation;

        [SerializeField, InLineEditor, BeginGroup("Audio")]
        private AudioDataSO _proximityEnterAudio;

        [SerializeField, InLineEditor, EndGroup]
        private AudioDataSO _proximityExitAudio;

        private bool _previouslyInProximity;


        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(TagConstants.PLAYER))
            {
                _previouslyInProximity = true;
                if (_proximityEnterAudio != null)
                    AudioManager.Instance.PlayClipAtPoint(_proximityEnterAudio.Clip, transform.position, _proximityEnterAudio.Volume);
                
                _hideAnimation.Cancel();
                _showAnimation.Play();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag(TagConstants.PLAYER))
            {
                if (_previouslyInProximity)
                    _messageIcon.color = new Color(_messageSeenIconColor.r, _messageSeenIconColor.g, _messageSeenIconColor.b, _messageIcon.color.a);

                if (_proximityExitAudio != null)
                    AudioManager.Instance.PlayClipAtPoint(_proximityExitAudio.Clip, transform.position, _proximityExitAudio.Volume);

                _showAnimation.Cancel();
                _hideAnimation.Play();
            }
        }

        private void Start()
        {
            _hideAnimation.SetTime(1f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _showAnimation?.Validate(gameObject);
            _hideAnimation?.Validate(gameObject);
        }
#endif
    }
}
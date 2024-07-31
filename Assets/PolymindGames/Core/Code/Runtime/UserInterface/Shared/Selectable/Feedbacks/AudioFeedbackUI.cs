using UnityEngine;

namespace PolymindGames.UserInterface
{
    [NestedObjectPath(MenuName = "Audio Feedback")]
    public sealed class AudioFeedbackUI : SelectableFeedbackUI
    {
        [SerializeField, InLineEditor]
        private AudioDataSO _highlightAudio;

        [SerializeField, InLineEditor]
        private AudioDataSO _selectedAudio;


        public override void OnNormal(bool instant) { }
        public override void OnPressed(bool instant) { }

        public override void OnHighlighted(bool instant)
        {
            if (!instant)
                PlayAudio(_highlightAudio);
        }

        public override void OnSelected(bool instant)
        {
            if (!instant)
                PlayAudio(_selectedAudio);
        }

        private static void PlayAudio(AudioDataSO audioData)
        {
            if (audioData != null)
                AudioManager.Instance.PlayUIClip(audioData.Clip, audioData.Volume, audioData.Pitch);
        }
    }
}
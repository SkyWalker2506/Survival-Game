using UnityEngine;

namespace PolymindGames.UserInterface
{
    [AddComponentMenu("Polymind Games/User Interface/Panels/Audio Panel")]
    public class AudioPanelUI : CanvasPanelUI
    {
        [SerializeField, InLineEditor, BeginGroup("Audio")]
        private AudioDataSO _showAudio;

        [SerializeField, InLineEditor, EndGroup]
        private AudioDataSO _hideAudio;


        protected override void OnShowPanel(bool show)
        {
            base.OnShowPanel(show);

            if (show)
            {
                if (_showAudio != null)
                    AudioManager.Instance.PlayUIClip(_showAudio.Clip, _showAudio.Volume);
            }
            else
            {
                if (_hideAudio != null)
                    AudioManager.Instance.PlayUIClip(_hideAudio.Clip, _hideAudio.Volume);
            }
        }
    }
}
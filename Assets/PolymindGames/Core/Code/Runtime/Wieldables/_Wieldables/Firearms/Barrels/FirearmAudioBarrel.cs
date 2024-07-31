using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Muzzle Effects/Audio Barrel")]
    public sealed class FirearmAudioBarrel : FirearmBarrelBehaviour
    {
        [SerializeField, BeginGroup("Audio")]
        private AdvancedAudioData _fireAudio = AdvancedAudioData.Default;

        [SerializeField, EndGroup]
        private AdvancedAudioData _fireTailAudio = AdvancedAudioData.Default;


        public override void DoFireEffect() => Wieldable.AudioPlayer.Play(_fireAudio);

        public override void DoFireStopEffect()
        {
            if (_fireTailAudio.IsPlayable)
                Wieldable.AudioPlayer.Play(_fireTailAudio);
        }
    }
}
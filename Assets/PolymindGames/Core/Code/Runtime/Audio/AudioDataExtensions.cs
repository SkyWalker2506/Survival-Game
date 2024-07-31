using System.Runtime.CompilerServices;

namespace PolymindGames
{
    public static partial class AudioDataExtensions
    {
        #region Audio Data SO
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlaySafe(this IAudioPlayer audioPlayer, AudioDataSO audioData, BodyPoint point = BodyPoint.Hands)
        {
            if (audioData != null && audioData.IsPlayable)
                audioPlayer.Play(audioData.Clip, point, audioData.Volume, audioData.Pitch);
        }
        #endregion

        #region Simple Audio Data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Play(this IAudioPlayer audioPlayer, in SimpleAudioData audioData, BodyPoint point = BodyPoint.Hands)
        {
            audioPlayer.Play(audioData.Clip, point, audioData.Volume);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Play(this IAudioPlayer audioPlayer, in SimpleAudioData audioData, float volume, BodyPoint point = BodyPoint.Hands)
        {
            audioPlayer.Play(audioData.Clip, point, audioData.Volume * volume);
        }
        #endregion

        #region Audio Data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Play(this IAudioPlayer audioPlayer, in AudioData audioData, BodyPoint point = BodyPoint.Hands)
        {
            audioPlayer.Play(audioData.Clip, point, audioData.Volume);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Play(this IAudioPlayer audioPlayer, in AudioData audioData, float volume, BodyPoint point = BodyPoint.Hands)
        {
            audioPlayer.Play(audioData.Clip, point, audioData.Volume * volume);
        }
        #endregion

        #region Advanced Audio Data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Play(this IAudioPlayer audioPlayer, in AdvancedAudioData audioData, BodyPoint point = BodyPoint.Hands)
        {
            audioPlayer.Play(audioData.Clip, point, audioData.Volume, audioData.Pitch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Play(this IAudioPlayer audioPlayer, in AdvancedAudioData audioData, float volume, BodyPoint point = BodyPoint.Hands)
        {
            audioPlayer.Play(audioData.Clip, point, audioData.Volume * volume, audioData.Pitch);
        }
        #endregion

        #region Delayed Simple Audio Data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlayDelayed(this IAudioPlayer audioPlayer, in DelayedSimpleAudioData audioData, BodyPoint point = BodyPoint.Hands)
        {
            audioPlayer.PlayDelayed(audioData.Clip, point, audioData.Delay, audioData.Volume);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlayDelayed(this IAudioPlayer audioPlayer, in DelayedSimpleAudioData audioData, float volume, BodyPoint point = BodyPoint.Hands)
        {
            audioPlayer.PlayDelayed(audioData.Clip, point, audioData.Delay, audioData.Volume * volume);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlayDelayed(this IAudioPlayer audioPlayer, DelayedSimpleAudioData[] audioData, float speed = 1f, BodyPoint point = BodyPoint.Hands)
        {
            for (int i = 0; i < audioData.Length; i++)
            {
                var audio = audioData[i];
                audioPlayer.PlayDelayed(audio.Clip, point, audio.Delay / speed, audio.Volume);
            }
        }
        #endregion

        #region Delayed Audio Data
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlayDelayed(this IAudioPlayer audioPlayer, in DelayedAudioData audioData, BodyPoint point = BodyPoint.Hands)
        {
            audioPlayer.PlayDelayed(audioData.Clip, point, audioData.Delay, audioData.Volume);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlayDelayed(this IAudioPlayer audioPlayer, in DelayedAudioData audioData, float volume, BodyPoint point = BodyPoint.Hands)
        {
            audioPlayer.PlayDelayed(audioData.Clip, point, audioData.Delay, audioData.Volume * volume);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void PlayDelayed(this IAudioPlayer audioPlayer, DelayedAudioData[] audioData, float speed = 1f, BodyPoint point = BodyPoint.Hands)
        {
            for (int i = 0; i < audioData.Length; i++)
            {
                var audio = audioData[i];
                audioPlayer.PlayDelayed(audio.Clip, point, audio.Delay / speed, audio.Volume);
            }
        }
        #endregion
    }
}
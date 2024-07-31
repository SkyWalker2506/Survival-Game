using UnityEngine;

namespace PolymindGames
{
    public interface IAudioPlayer
    {
        void Play(AudioClip clip, BodyPoint point, float volume = 1f, float pitch = 1f);
        void PlayDelayed(AudioClip clip, BodyPoint point, float delay, float volume = 1f, float pitch = 1f);
        int StartLoop(AudioClip clip, BodyPoint point, float volume = 1f, float pitch = 1f, float duration = Mathf.Infinity);
        bool IsLoopPlaying(int loopId);
        void StopLoop(ref int loopId);
    }
}
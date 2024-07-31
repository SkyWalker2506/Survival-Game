using UnityEngine;
using System;

namespace PolymindGames
{
    /// <summary>
    /// A simple audio data struct to store an AudioClip and a volume multiplier.
    /// </summary>
    [Serializable]
    public struct SimpleAudioData
    {
        [Range(0f, 1f)]
        [Tooltip("The volume of the audio clip in the range of 0 to 1.")]
        public float Volume;

        [SerializeField, InLineEditor]
        [Tooltip("The audio clip.")]
        public AudioClip Clip;


        /// <summary>
        /// Default instance of SimpleAudioData.
        /// </summary>
        public static readonly SimpleAudioData Default = new()
        {
            Volume = 1f
        };

        /// <summary>
        /// Returns true if the audio is worth playing (can be heard).
        /// </summary>
        public readonly bool IsPlayable => Volume > 0.01f;
    }

    /// <summary>
    /// A version of SimpleAudioData that adds a delay time.
    /// </summary>
    [Serializable]
    public struct DelayedSimpleAudioData
    {
        [Range(0f, 1f)]
        [Tooltip("The volume of the audio clip in the range of 0 to 1.")]
        public float Volume;

        [Range(0f, 20f)]
        [Tooltip("The delay time in seconds.")]
        public float Delay;

        [InLineEditor]
        [Tooltip("The audio clip.")]
        public AudioClip Clip;


        /// <summary>
        /// Default instance of DelayedSimpleAudioData.
        /// </summary>
        public static readonly DelayedSimpleAudioData Default = new()
        {
            Volume = 1f,
            Delay = 0f
        };

        /// <summary>
        /// Returns true if the audio is worth playing (can be heard).
        /// </summary>
        public readonly bool IsPlayable => Volume > 0.01f;
    }

    /// <summary>
    /// Struct that's used to store a list of audio clips and a volume multiplier.
    /// </summary>
    [Serializable]
    public struct AudioData
    {
        [SerializeField, Range(0f, 1f)]
        [Tooltip("The volume of the audio clip in the range of 0 to 1.")]
        private float _volume;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("The amount of volume jitter to apply to the audio clip.")]
        private float _volumeJitter;

        [SpaceArea(3)]
        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false)]
        [Tooltip("The array of audio clips to choose from.")]
        private AudioClip[] _clips;


        /// <summary>
        /// The default instance of AudioData.
        /// </summary>
        public static readonly AudioData Default = new(Array.Empty<AudioClip>(), 1f, 0.1f);

        /// <summary>
        /// Constructor for creating an instance of AudioData.
        /// </summary>
        /// <param name="clips">The array of audio clips to choose from.</param>
        /// <param name="volume">The volume of the audio clip in the range of 0 to 1.</param>
        /// <param name="volumeJitter">The amount of volume jitter to apply to the audio clip.</param>
        public AudioData(AudioClip[] clips, float volume, float volumeJitter)
        {
            _clips = clips;
            _volume = volume;
            _volumeJitter = volumeJitter;
        }

        /// <summary>
        /// The audio clip to play.
        /// </summary>
        public readonly AudioClip Clip => _clips.SelectRandom();

        /// <summary>
        /// The jittered volume multiplier to use for the clip.
        /// </summary>
        public readonly float Volume => _volume.Jitter(_volumeJitter);

        /// <summary>
        /// Returns true if the audio is worth playing (can be heard).
        /// </summary>
        public readonly bool IsPlayable => _volume > 0.01f && _clips.Length > 0;
    }

    /// <summary>
    /// A version of AudioData that adds a delay time.
    /// </summary>
    [Serializable]
    public struct DelayedAudioData
    {
        [SerializeField, Range(0f, 1f)]
        [Tooltip("The volume of the audio clip in the range of 0 to 1.")]
        private float _volume;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("The amount of volume jitter to apply to the audio clip.")]
        private float _volumeJitter;

        [SpaceArea(3)]
        [SerializeField, Range(0f, 20f)]
        [Tooltip("The delay time in seconds.")]
        private float _delay;

        [SpaceArea(3)]
        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false)]
        [Tooltip("The array of audio clips to choose from.")]
        private AudioClip[] _clips;


        /// <summary>
        /// The default instance of DelayedAudioData.
        /// </summary>
        public static readonly DelayedAudioData Default = new()
        {
            _volume = 1f,
            _volumeJitter = 0.1f,
            _delay = 0f
        };

        /// <summary>
        /// The audio clip to play.
        /// </summary>
        public readonly AudioClip Clip => _clips.SelectRandom();

        /// <summary>
        /// The volume of the audio clip with jitter applied.
        /// </summary>
        public readonly float Volume => _volume.Jitter(_volumeJitter);

        /// <summary>
        /// The delay time in seconds.
        /// </summary>
        public readonly float Delay => _delay;

        /// <summary>
        /// Returns true if the audio is worth playing (can be heard).
        /// </summary>
        public readonly bool IsPlayable => _volume > 0.01f && _clips.Length > 0;
    }

    [Serializable]
    public struct AdvancedAudioData
    {
        [SerializeField, Range(0f, 1f)]
        [Tooltip("The volume of the audio clip in the range of 0 to 1.")]
        private float _volume;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("The amount of volume jitter to apply to the audio clip.")]
        private float _volumeJitter;

        [SerializeField, Range(0f, 2f)]
        [Tooltip("The pitch of the audio clip in the range of 0 to 2.")]
        private float _pitch;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("The amount of pitch jitter to apply to the audio clip.")]
        private float _pitchJitter;

        [SpaceArea(3)]
        [SerializeField, ReorderableList(ListStyle.Lined, HasLabels = false)]
        [Tooltip("The array of audio clips to choose from.")]
        private AudioClip[] _clips;


        /// <summary>
        /// The default values for an instance of AdvancedAudioData.
        /// </summary>
        public static readonly AdvancedAudioData Default = new(Array.Empty<AudioClip>(), 1f, 0.1f, 1f, 0.05f);

        public AdvancedAudioData(AudioClip[] clips, float volume, float volumeJitter, float pitch, float pitchJitter)
        {
            _clips = clips;
            _volume = volume;
            _volumeJitter = volumeJitter;
            _pitch = pitch;
            _pitchJitter = pitchJitter;
        }

        /// <summary>
        /// The audio clip to play.
        /// </summary>
        public readonly AudioClip Clip => _clips.SelectRandom();

        /// <summary>
        /// The volume of the audio clip with jitter applied.
        /// </summary>
        public readonly float Volume => _volume.Jitter(_volumeJitter);

        /// <summary>
        /// The pitch of the audio clip with jitter applied.
        /// </summary>
        public readonly float Pitch => _pitch.Jitter(_pitchJitter);

        /// <summary>
        /// Returns true if the audio is worth playing (can be heard).
        /// </summary>
        public readonly bool IsPlayable => _volume > 0.01f && _clips.Length > 0;
    }
}
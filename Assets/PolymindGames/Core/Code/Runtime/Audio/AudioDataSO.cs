using UnityEngine;

namespace PolymindGames
{
    [CreateAssetMenu(menuName = "Polymind Games/Utilities/Audio Data", fileName = "Audio_")]
    public sealed class AudioDataSO : ScriptableObject
    {
        [SerializeField, Range(0f, 1f)]
        private float _volume = 1f;

        [SerializeField, Range(0f, 1f)]
        private float _volumeJitter = 0.1f;

        [SpaceArea(3f)]
        [SerializeField, Range(0f, 2f)]
        private float _pitch = 1f;

        [SerializeField, Range(0f, 1f)]
        private float _pitchJitter = 0.05f;

        [SpaceArea(3f)]
        [SerializeField, Range(0f, 5f)]
        private float _cooldown = 0.15f;

        [SerializeField, SpaceArea(3f)]
        [ReorderableList(ListStyle.Lined, HasLabels = false)]
        private AudioClip[] _clips;

        private int _lastPlayedClip;
        private float _playTimer = -1f;
        
        
        public AudioClip Clip => _clips.Select(ref _lastPlayedClip);

        public bool IsPlayable
        {
            get
            {
#if UNITY_EDITOR

                // This can happen when the "Reload Domain" play mode option is disabled.
                if (_playTimer < (float)UnityEditor.EditorApplication.timeSinceStartup)
                    return true;
#else
                if (_playTimer < Time.time)
                    return true;
#endif

                return false;
            }
        }

        public float Volume
        {
            get
            {
                if (IsPlayable)
                {
#if UNITY_EDITOR
                    _playTimer = (float)UnityEditor.EditorApplication.timeSinceStartup + _cooldown;
#else
                    _playTimer = Time.time + _cooldown;
#endif

                    return _volume.Jitter(_volumeJitter);
                }

                return 0f;
            }
        }

        public float Pitch => _pitch.Jitter(_pitchJitter);
    }
}
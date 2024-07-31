using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace PolymindGames
{
    [HelpURL(
        "https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/player/modules-and-behaviours/audio#audio-player-module")]
    public sealed class CharacterAudioPlayer : CharacterBehaviour, IAudioPlayer
    {
        [HideInPlayMode]
        [SerializeField, ReorderableList(ListStyle.Boxed), LabelFromChild(nameof(AudioSourcePoint.Point))]
        private AudioSourcePoint[] _audioSourcePoints = Array.Empty<AudioSourcePoint>();

        private AudioPointData[] _audioPointsData;
        

        public void Play(AudioClip clip, BodyPoint point, float volume, float pitch)
        {
            if (!IsVolumeInRange(volume))
                return;

            AudioPointData audioPointData = GetAudioPointData(point);
            AudioSource source = audioPointData.GetNextSource();

            source.volume = 1f;
            source.pitch = pitch;
            source.PlayOneShot(clip, volume);
        }

        public void PlayDelayed(AudioClip clip, BodyPoint point, float delay, float volume, float pitch)
        {
            if (!IsVolumeInRange(volume))
                return;

            AudioPointData audioPointData = GetAudioPointData(point);
            AudioSource source = audioPointData.GetNextSource();

            source.Stop();
            source.clip = clip;
            source.volume = volume;
            source.pitch = pitch;
            source.PlayDelayed(delay);
        }

        public int StartLoop(AudioClip clip, BodyPoint point, float volume, float pitch, float duration)
        {
            if (!IsVolumeInRange(volume))
                return AudioManager.NULL_LOOP_ID;

            AudioPointData audioPointData = GetAudioPointData(point);
            if (!audioPointData.TryGetNextFreeLoopSource(out var source))
            {
#if DEBUG
                Debug.LogWarning("All audio sources used for looping are in use. Consider increasing the amount of sources.");
#endif

                return AudioManager.NULL_LOOP_ID;
            }

            source.playOnAwake = true;
            source.clip = clip;
            source.pitch = pitch;

            StartCoroutine(AudioManager.Instance.C_PlayLoop(source, volume, duration));
            return CreateLoopId(point, audioPointData.GetCurrentLoopIndex());
        }

        private static int CreateLoopId(BodyPoint point, int loopIndex) =>
            (int)point * 100 + loopIndex;

        private static int GetAudioPointDataIndex(int loopId) => 
            ((BodyPoint)(loopId / 100)).GetIndex();
        
        private static int GetAudioSourceLoopIndex(int loopId) =>
            ((BodyPoint)(loopId % 100)).GetIndex();

        public bool IsLoopPlaying(int loopId)
        {
            if (loopId < 0)
                return false;

            var sources = _audioPointsData[GetAudioPointDataIndex(loopId)].GetAllLoopSources();
            AudioSource source = sources[GetAudioSourceLoopIndex(loopId)];
            return source.isPlaying;
        }

        public void StopLoop(ref int loopId)
        {
#if DEBUG
            if (loopId < 0)
            {
                Debug.LogError($"{loopId} is not a valid id.");
                return;
            }
#endif

            var sources = _audioPointsData[GetAudioPointDataIndex(loopId)].GetAllLoopSources();
            AudioSource source = sources[GetAudioSourceLoopIndex(loopId)];
            source.playOnAwake = false;

            loopId = AudioManager.NULL_LOOP_ID;
        }

        protected override void OnBehaviourStart(ICharacter character)
        {
            // Create the audio points data.
            int length = GetLastBodyPointFromSources();
            _audioPointsData = length > 0
                ? new AudioPointData[length]
                : Array.Empty<AudioPointData>();

            // Assign the audio points data.
            for (int i = 0; i < _audioPointsData.Length; ++i)
            {
                var audioSourcePoint = GetSourceWithBodyPoint(i);
                var duplicatePointsData = _audioPointsData[audioSourcePoint.Point.GetIndex()];
                var pointTrs = character.GetTransformOfBodyPoint(audioSourcePoint.Point);
                _audioPointsData[i] = duplicatePointsData ?? CreateAudioPointData(ref audioSourcePoint, pointTrs);
            }

            _audioSourcePoints = null;
            return;

            int GetLastBodyPointFromSources()
            {
                int count = 0;
                for (int i = 0; i < _audioSourcePoints.Length; i++)
                {
                    int pointVal = _audioSourcePoints[i].Point.GetIndex();
                    if (count < pointVal)
                        count = pointVal;
                }

                return count + 1;
            }

            AudioSourcePoint GetSourceWithBodyPoint(int bodyPoint)
            {
                for (int i = 0; i < _audioSourcePoints.Length; i++)
                {
                    if (_audioSourcePoints[i].Point.GetIndex() == bodyPoint)
                        return _audioSourcePoints[i];
                }

                return _audioSourcePoints[0];
            }
        }

        private static AudioPointData CreateAudioPointData(ref AudioSourcePoint sourcePoint, Transform pointTrs)
        {
            if (pointTrs == null)
            {
                Debug.LogWarning($"No point transform found for: ''{sourcePoint.Point}'");
                return null;
            }

            var pointObj = pointTrs.gameObject;
            return new AudioPointData(
                sources: CreateAudioSourceArray(pointObj, sourcePoint.SourcesCount, false),
                loopSources: CreateAudioSourceArray(pointObj, sourcePoint.LoopSourcesCount, true));

            static AudioSource[] CreateAudioSourceArray(GameObject obj, int length, bool loop)
            {
                var sources = length == 0
                    ? Array.Empty<AudioSource>()
                    : new AudioSource[length];

                for (int i = 0; i < length; i++)
                {
                    var source = obj.AddComponent<AudioSource>();
                    source.outputAudioMixerGroup = AudioManager.Instance.EffectsGroup;
                    source.playOnAwake = false;
                    source.loop = loop;
                    source.spatialize = true;
                    source.spatialBlend = 1f;

                    sources[i] = source;
                }

                return sources;
            }
        }

        private AudioPointData GetAudioPointData(BodyPoint point)
        {
            int index = Mathf.Clamp(point.GetIndex(), 0, _audioPointsData.Length - 1);
            AudioPointData audioPointData = _audioPointsData[index];

            if (audioPointData == null)
            {
                audioPointData = _audioPointsData[0];
#if DEBUG
                Debug.LogError($"No body point of type ''{point}'' found on this character.", gameObject);
#endif
            }

            return audioPointData;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsVolumeInRange(float volume)
        {
            if (volume > 0.01f)
                return true;

#if DEBUG
            Debug.LogWarning($"Volume is too low.", gameObject);
#endif

            return false;
        }

        #region Editor
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_audioSourcePoints.Length < 1)
            {
                _audioSourcePoints = new[]
                {
                    new AudioSourcePoint
                    {
                        Point = BodyPoint.Head,
                        SourcesCount = 4,
                        LoopSourcesCount = 1
                    }
                };
            }

            _audioSourcePoints[0].Point = BodyPoint.Head;
        }
#endif
        #endregion

        #region Internal
        private sealed class AudioPointData
        {
            private readonly AudioSource[] _loopSources;
            private readonly AudioSource[] _sources;
            private int _currentLoopIndex;
            private int _currentIndex;


            public AudioPointData(AudioSource[] sources, AudioSource[] loopSources)
            {
                _sources = sources;
                _currentIndex = -1;

                _loopSources = loopSources;
                _currentLoopIndex = -1;
            }

            public AudioSource[] GetAllSources() => _sources;

            public AudioSource GetNextSource()
            {
                if (_sources.Length == 1)
                {
                    _currentIndex = 0;
                    return _sources[0];
                }

                _currentIndex = (int)Mathf.Repeat(_currentIndex + 1, _sources.Length);
                return _sources[_currentIndex];
            }

            public int GetCurrentLoopIndex() => _currentLoopIndex;
            public AudioSource[] GetAllLoopSources() => _loopSources;

            public bool TryGetNextFreeLoopSource(out AudioSource source)
            {
                if (_loopSources.Length == 0)
                {
                    source = null;
                    return false;
                }

                if (_loopSources.Length == 1)
                {
                    _currentLoopIndex = 0;
                    source = _loopSources[0];
                }

                _currentLoopIndex = (int)Mathf.Repeat(_currentLoopIndex + 1, _loopSources.Length);
                source = _loopSources[_currentLoopIndex];

                return !source.isPlaying;
            }
        }

        [Serializable]
        private struct AudioSourcePoint
        {
            public BodyPoint Point;

            [Range(0, 10)]
            public int SourcesCount;

            [Range(0, 10)]
            public int LoopSourcesCount;
        }
        #endregion
    }
}
using PolymindGames.PoolingSystem;
using UnityEngine;

namespace PolymindGames
{
    /// <summary>
    /// ScriptableObject representing an effect pair consisting of a particle system and an audio clip.
    /// </summary>
    [CreateAssetMenu(menuName = "Polymind Games/Utilities/Effect Pair", fileName = "EffectPair_")]
    public sealed class EffectPairSO : ScriptableObject
    {
        [SerializeField, BeginGroup("Effects")]
        [Tooltip("The particle system to be played.")]
        private ParticleSystem _particles;

        [SerializeField, Range(0, 100), HideIf(nameof(_particles), false)]
        [Tooltip("The number of instances to keep in the pool.")]
        private int _poolCount = 4;
        
        [SerializeField, EndGroup]
        [Tooltip("The audio data to be played.")]
        private AudioDataSO _audio;

        
        /// <summary>
        /// Plays the particle system and audio at a given position with a specified rotation.
        /// </summary>
        /// <param name="position">The position at which to play the effects.</param>
        /// <param name="rotation">The rotation at which to play the effects.</param>
        public void PlayAtPosition(Vector3 position, Quaternion rotation)
        {
            // Get an instance of the particle system from the pool
            if (_particles != null)
            {
                var instance = GetInstanceFromPool();

                // Set the position and rotation of the instance and play it
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.Play(true);
            }

            // If an audio clip is provided, play it at the same position
            if (_audio != null)
                AudioManager.Instance.PlayClipAtPoint(_audio.Clip, position, _audio.Volume, _audio.Pitch);
        }

        /// <summary>
        /// Retrieves an instance of the particle system from the pool.
        /// </summary>
        /// <returns>An instance of the particle system.</returns>
        private ParticleSystem GetInstanceFromPool()
        {
            // If the particle system is not already pooled, create a new pool for it
            if (!ScenePools.ContainsPool(_particles))
            {
                var pool = ScenePools.CreatePool(_particles, 2, _poolCount, name);
                return pool.GetInstanceComponent();
            }

            // Otherwise, retrieve an instance of the particle system from the existing pool
            return ScenePools.GetObject(_particles);
        }
    }
}
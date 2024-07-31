using PolymindGames.PoolingSystem;
using UnityEngine;

namespace PolymindGames.SurfaceSystem
{
    /// <summary>
    /// This script defines a surface effect that manages visual effects.
    /// </summary>
    [RequireComponent(typeof(PoolableObject))]
    public sealed class SurfaceEffect : MonoBehaviour
    {
        [SerializeField, ReorderableList(ListStyle.Boxed, HasLabels = false)]
        private ParticleSystem[] _particles;

        
        public void Play()
        {
            for (int i = 0; i < _particles.Length; i++)
                _particles[i].Play(false);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            gameObject.layer = LayerConstants.EFFECT;
        }

        private void OnValidate()
        {
            _particles = GetComponentsInChildren<ParticleSystem>();
        }
#endif
    }
}
using PolymindGames.PoolingSystem;
using System.Collections.Generic;
using UnityEngine;

namespace PolymindGames.SurfaceSystem
{
    /// <summary>
    /// Global surface effects system
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConstants.SCRIPTABLE_SINGLETON)]
    [CreateAssetMenu(menuName = MANAGERS_MENU_PATH + "Surface Manager", fileName = nameof(SurfaceManager))]
    public sealed class SurfaceManager : Manager<SurfaceManager>
    {
        [BeginGroup, EndGroup]
        [SerializeField, InLineEditor, NotNull]
        [Tooltip("Default surface definition.")]
        private SurfaceDefinition _defaultSurface;

        [SerializeField, Range(2, 128), BeginGroup("Effects")]
        [Tooltip("Size of the effect pool.")]
        private int _effectPoolSize = 4;

        [SerializeField, Range(2, 128), EndGroup]
        [Tooltip("Capacity of the effect pool.")]
        private int _effectPoolCapacity = 8;

        [SerializeField, Range(2, 128), BeginGroup("Decals")]
        [Tooltip("Size of the decal pool.")]
        private int _decalPoolSize = 4;

        [SerializeField, Range(2, 128), EndGroup]
        [Tooltip("Capacity of the decal pool.")]
        private int _decalPoolCapacity = 16;

        private readonly Dictionary<PhysicMaterial, SurfaceDefinition> _materialSurfacePairs = new(12);
        private readonly Dictionary<int, CachedSurfaceEffect> _surfaceEffects = new(32);

        
        #region Initialization
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init() => LoadOrCreateInstance();

        static SurfaceManager()
        {
            ScenePools.ScenePoolHandlerDestroyed += () =>
            {
                if (Instance != null)
                    Instance._surfaceEffects.Clear();
            };
        }

        protected override void OnInitialized()
        {
#if UNITY_EDITOR
            _materialSurfacePairs.Clear();
            _surfaceEffects.Clear();
#endif
            CacheSurfaceDefinitions();
        }

        private void CacheSurfaceDefinitions()
        {
            var surfaces = SurfaceDefinition.Definitions;
            foreach (var surface in surfaces)
            {
                foreach (var material in surface.Materials)
                {
                    if (material == null)
                    {
                        Debug.LogError($"One of the physic materials ''{surface}'' is null.", surface);
                        return;
                    }

                    if (!_materialSurfacePairs.TryAdd(material, surface))
                    {
                        Debug.LogError($"The physic material ''{material.name}'' on {surface.name} is already referenced by a different surface definition ''{_materialSurfacePairs[material]}''", material);
                        return;
                    }
                }
            }
        }
		#endregion
        
        public SurfaceDefinition GetSurfaceFromHit(ref RaycastHit hit)
        {
            var material = hit.collider.sharedMaterial;
            if (material != null && _materialSurfacePairs.TryGetValue(material, out var surface))
                return surface;

            return hit.collider.TryGetComponent(out SurfaceIdentity identity)
                ? identity.GetSurfaceFromHit(ref hit)
                : _defaultSurface;
        }

        public SurfaceDefinition GetSurfaceFromCollision(Collision collision)
        {
            var material = collision.collider.sharedMaterial;
            if (material != null && _materialSurfacePairs.TryGetValue(material, out var surface))
                return surface;

            return collision.collider.TryGetComponent(out SurfaceIdentity identity)
                ? identity.GetSurfaceFromCollision(collision)
                : _defaultSurface;
        }

        public SurfaceDefinition GetSurfaceFromCollider(Collider collider)
        {
            return GetSurfaceFromMaterial(collider.sharedMaterial);
        }

        public SurfaceDefinition GetSurfaceFromMaterial(PhysicMaterial material)
        {
            if (material != null && _materialSurfacePairs.TryGetValue(material, out var surface))
                return surface;

            return _defaultSurface;
        }

        public SurfaceDefinition SpawnEffectFromHit(ref RaycastHit hit, SurfaceEffectType effectType, float audioVolume = 1f, bool parentDecal = false)
        {
            var surface = GetSurfaceFromHit(ref hit);
            if (TryGetEffect(surface, effectType, out var surfaceEffect))
            {
                if (parentDecal)
                    surfaceEffect.Play(hit.point, Quaternion.LookRotation(hit.normal), audioVolume, hit.transform);
                else
                    surfaceEffect.Play(hit.point, Quaternion.LookRotation(hit.normal), audioVolume);
            }

            return surface;
        }

        public SurfaceDefinition SpawnEffectFromCollision(Collision collision, SurfaceEffectType effectType, float audioVolume = 1f, bool parentDecal = false)
        {
            var surface = GetSurfaceFromCollision(collision);
            var contact = collision.GetContact(0);
            if (TryGetEffect(surface, effectType, out var surfaceEffect))
            {
                if (parentDecal)
                    surfaceEffect.Play(contact.point, Quaternion.LookRotation(contact.normal), audioVolume, collision.collider.transform);
                else
                    surfaceEffect.Play(contact.point, Quaternion.LookRotation(contact.normal), audioVolume);
            }

            return surface;
        }
        
        public void SpawnEffectFromSurface(SurfaceDefinition surface, Vector3 position, Quaternion rotation, SurfaceEffectType effectType, float audioVolume = 1f)
        {
            if (TryGetEffect(surface, effectType, out var surfaceEffect))
                surfaceEffect.Play(position, rotation, audioVolume);
        }

        private bool TryGetEffect(SurfaceDefinition surface, SurfaceEffectType effectType, out CachedSurfaceEffect surfaceEffect)
        {
            int poolId = surface.Id + (int)effectType;
            if (_surfaceEffects.TryGetValue(poolId, out surfaceEffect))
                return surfaceEffect != null;

            var effectPair = surface.GetEffectPairOfType(effectType);
            surfaceEffect = CreateCachedEffect(effectPair, poolId);
            return surfaceEffect != null;
        }

        private CachedSurfaceEffect CreateCachedEffect(SurfaceDefinition.EffectPair effectPair, int poolId)
        {
            var fxPool = effectPair.VisualEffect != null
                ? ScenePools.CreatePool(effectPair.VisualEffect, _effectPoolSize, _effectPoolCapacity, "SurfaceEffects")
                : null;

            var decalPool = effectPair.DecalEffect != null
                ? ScenePools.CreatePool(effectPair.DecalEffect, _decalPoolSize, _decalPoolCapacity, "SurfaceDecals")
                : null;

            var surfaceEffect = new CachedSurfaceEffect(effectPair, fxPool, decalPool);
            _surfaceEffects.Add(poolId, surfaceEffect);
            return surfaceEffect;
        }

		#region Internal
        private sealed class CachedSurfaceEffect
        {
            private readonly ObjectPool<SurfaceEffect> _decalPool;
            private readonly SurfaceDefinition.EffectPair _effectPair;
            private readonly ObjectPool<SurfaceEffect> _fxPool;
            private float _audioPlayTimer;
            
            private const float AUDIO_PLAY_COOLDOWN = 0.3f;

            
            public CachedSurfaceEffect(SurfaceDefinition.EffectPair effectPair, ObjectPool<SurfaceEffect> fxPool, ObjectPool<SurfaceEffect> decalPool)
            {
                _effectPair = effectPair;
                _fxPool = fxPool;
                _decalPool = decalPool;
            }

            public void Play(Vector3 position, Quaternion rotation, float volume)
            {
                // Play the audio.
                if (Time.time > _audioPlayTimer && _effectPair.AudioData.IsPlayable)
                {
                    AudioManager.Instance.PlayClipAtPoint(_effectPair.AudioData.Clip, position, _effectPair.AudioData.Volume * volume, _effectPair.AudioData.Pitch);
                    _audioPlayTimer = Time.time + AUDIO_PLAY_COOLDOWN;
                }

                // Play the visual effect. 
                if (_fxPool != null)
                {
                    var instance = _fxPool.GetInstance().GetCachedComponent<SurfaceEffect>();
                    instance.transform.SetPositionAndRotation(position, rotation);
                    instance.Play();
                }

                // Play the decal. 
                if (_decalPool != null)
                {
                    var instance = _decalPool.GetInstance().GetCachedComponent<SurfaceEffect>();
                    instance.transform.SetPositionAndRotation(position, rotation);
                    instance.Play();
                }
            }

            public void Play(Vector3 position, Quaternion rotation, float volume, Transform parent)
            {
                // Play the audio.
                if (_effectPair.AudioData.IsPlayable)
                    AudioManager.Instance.PlayClipAtPoint(_effectPair.AudioData.Clip, position, _effectPair.AudioData.Volume * volume, _effectPair.AudioData.Pitch);

                // Play the visual effect. 
                if (_fxPool != null)
                {
                    var instance = _fxPool.GetInstance().GetCachedComponent<SurfaceEffect>();
                    instance.transform.SetPositionAndRotation(position, rotation);
                    instance.Play();
                }

                // Play the decal. 
                if (_decalPool != null)
                {
                    var instance = _decalPool.GetInstance().GetCachedComponent<SurfaceEffect>();
                    var instanceTransform = instance.transform;
                    instanceTransform.SetPositionAndRotation(position, rotation);
                    instanceTransform.SetParent(parent);
                    instance.Play();
                }
            }
        }
		#endregion
    }
}
using System.Collections;
using PolymindGames.PoolingSystem;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Casing Ejectors/Casing Ejector")]
    public class FirearmCasingEjector : FirearmCasingEjectorBehaviour
    {
        [BeginGroup("References")]
        [SerializeField, NotNull, PrefabObjectOnly]
        private Rigidbody _casingPrefab;

        [SerializeField, NotNull, EndGroup]
        private Transform _ejectionPoint;

        [BeginGroup("Ejection")]
        [SerializeField, Range(0f, 10f)]
        private float _spawnDelay;

        [SerializeField, Range(0, 100f)]
        private float _spawnSpeed = 2f;

        [SerializeField, Range(0, 100f)]
        private float _spawnSpin = 10f;

        [SerializeField, Range(0.01f, 10f)]
        private float _spawnScale = 1f;

        [SerializeField]
        private Vector3 _spawnRotation;

        [SerializeField, SpaceArea, EndGroup]
        [ReorderableList(ListStyle.Lined, elementLabel: "Sound")]
        private DelayedSimpleAudioData[] _ejectAudio;

        private const float INHERITED_SPEED = 0.85f;
        private const float SPEED_RANDOMNESS = 0.5f;
        private const float RESET_SCALE_DURATION = 0.3f;
        private const float RESET_SCALE_DELAY = 0.2f;


        public override void Eject()
        {
            base.Eject();

            Wieldable.AudioPlayer.PlayDelayed(_ejectAudio);
            StartCoroutine(C_EjectCasing());
        }

        private IEnumerator C_EjectCasing()
        {
            float delay = _spawnDelay;
            while (delay > 0f)
            {
                delay -= Time.deltaTime;
                yield return null;
            }
            
            Quaternion rotation = Quaternion.Euler(Quaternion.LookRotation(_ejectionPoint.forward) * _spawnRotation);
            Quaternion randomRotation = Random.rotation;

            var casing = ScenePools.GetObject(_casingPrefab, _ejectionPoint.position, Quaternion.Lerp(rotation, randomRotation, 0.1f));

            Vector3 velocityJitter = new(Random.Range(-SPEED_RANDOMNESS, SPEED_RANDOMNESS),
                Random.Range(-SPEED_RANDOMNESS, SPEED_RANDOMNESS),
                Random.Range(-SPEED_RANDOMNESS, SPEED_RANDOMNESS));

            var inheritedVelocity = Wieldable.Character.GetCC<IMotorCC>().Velocity * INHERITED_SPEED;
            var randomizedVelocity = _ejectionPoint.TransformVector(Vector3.forward * _spawnSpeed + velocityJitter);

            float spinDirection = Random.Range(0, 2) == 0 ? 1f : -1f;
            float spinAmount = Random.Range(0.5f, 1f) * _spawnSpin;

            casing.linearVelocity = inheritedVelocity + randomizedVelocity;
            casing.angularVelocity = spinAmount * spinDirection * Vector3.one;

            Vector3 startScale = !Wieldable.IsGeometryVisible ? Vector3.zero : Vector3.one * _spawnScale;
            
            if (Mathf.Approximately(startScale.x, 1f))
                yield break;

            var casingTrs = casing.transform;
            casingTrs.localScale = startScale;

            delay = RESET_SCALE_DELAY;
            while (delay > 0f)
            {
                delay -= Time.deltaTime;
                yield return null;
            }

            float t = 0f;
            while (t < 1f)
            {
                casingTrs.localScale = Vector3.Lerp(startScale, Vector3.one, t);
                t += Time.deltaTime * (1 / RESET_SCALE_DURATION);

                yield return null;
            }

            casingTrs.localScale = Vector3.one;
        }

        protected override void Awake()
        {
            base.Awake();

            if (_casingPrefab == null)
            {
                Debug.LogError($"Prefab on {gameObject.name} can't be null.");
                return;
            }

            _casingPrefab.maxAngularVelocity = 10000f;
            ScenePools.CreatePool(_casingPrefab, 2, 8, "Casings", 10f);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _spawnDelay = Mathf.Clamp(_spawnDelay, 0f, EjectDuration);
        }
#endif
    }
}
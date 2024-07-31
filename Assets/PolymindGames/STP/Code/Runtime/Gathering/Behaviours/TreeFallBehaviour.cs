using System.Collections;
using PolymindGames.ProceduralMotion;
using PolymindGames.SaveSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PolymindGames.ResourceGathering
{
    public sealed class TreeFallBehaviour : MonoBehaviour, ISaveableComponent
    {
        [SerializeField, NotNull, BeginGroup("References")]
        private Rigidbody _fallingTree;

        [SerializeField, NotNull, EndGroup]
        private ColliderTriggerHandler _impactTrigger;

        [BeginGroup("Logs")]
        [SerializeField, NotNull, PrefabObjectOnly]
        private Rigidbody _logPrefab;

        [SerializeField, Range(1, 100)]
        private int _logsCount = 6;

        [SerializeField, Range(0f, 100f), EndGroup]
        private float _logsOffset = 2f;

        [SerializeField, BeginGroup("Effects")]
        private ParticleSystem _treeImpactFX;

        [SerializeField, NotNull]
        private AudioDataSO _treeFallAudio;

        [SerializeField, NotNull]
        private AudioDataSO _treeImpactAudio;

        [SerializeField, EndGroup]
        private ShakeData _shake;

        private Vector3 _fallingTreeDefaultPosition;
        private IGatherable _gatherable;
        private bool _isFalling;

        private const float MIN_IMPACT_TRIGGER_TIME = 1f;
        private const float SHAKE_RADIUS = 10f;
        private const float MAX_FALL_TIME = 8f;
        private const float LOGS_FORCE = 50f;


        private void Awake()
        {
            _fallingTreeDefaultPosition = _fallingTree.transform.localPosition;
        }

        private void OnEnable()
        {
            // Get reference to gatherable component and subscribe to damage received event
            _gatherable = gameObject.GetComponentInParent<IGatherable>();
            if (_gatherable == null)
                return;
            
            _gatherable.DamageReceived += OnDamage;

            if (_gatherable.IsAlive)
                DeactivateFallingTree();
            else
                ActivateFallingTree();
        }

        private void OnDisable()
        {
            // Unsubscribe from damage received event and enable collision
            if (_gatherable != null)
            {
                _gatherable.DamageReceived -= OnDamage;
                _gatherable.EnableCollision(true);
            }
        }

        private void DeactivateFallingTree()
        {
            _fallingTree.GetComponent<Collider>().enabled = false;
            _fallingTree.isKinematic = true;
            _fallingTree.transform.SetLocalPositionAndRotation(_fallingTreeDefaultPosition, Quaternion.identity);
            _fallingTree.gameObject.SetActive(true);
        }

        private void ActivateFallingTree()
        {
            _gatherable.EnableCollision(false);
            _fallingTree.GetComponent<Collider>().enabled = true;
            _fallingTree.isKinematic = false;
        }

        // Event handler for damage received
        private void OnDamage(float damage, in DamageArgs args)
        {
            // Check if tree is alive
            if (!_gatherable.IsAlive)
            {
                ActivateFallingTree();
                _fallingTree.AddForce(new Vector3(args.HitForce.x, 0, args.HitForce.z), ForceMode.Impulse);

                StartCoroutine(C_HandleFalling());
            }
        }

        private IEnumerator C_HandleFalling()
        {
            // Play tree falling audio
            AudioManager.Instance.PlayClipAtPoint(_treeFallAudio.Clip, transform.position, _treeFallAudio.Volume);
            
            _impactTrigger.TriggerEnter += OnTreeImpact;

            _isFalling = true;
            float timeSinceFallStart = 0f;
            while (_isFalling)
            {
                // Check if tree has fallen for too long or has almost stopped rotating
                if (timeSinceFallStart > MIN_IMPACT_TRIGGER_TIME)
                {
                    if (timeSinceFallStart > MAX_FALL_TIME || _fallingTree.angularVelocity.sqrMagnitude < 0.001f)
                        _isFalling = false;
                }

                timeSinceFallStart += Time.deltaTime;
                yield return null;
            }

            _impactTrigger.TriggerEnter -= OnTreeImpact;
            
            // Wait for a short duration before processing impact
            float stopTime = Time.time + 0.1f;
            while (Time.time < stopTime)
                yield return null;
            
            HandleImpact();
            
            void OnTreeImpact(Collider other) => _isFalling = false;
        }

        private void HandleImpact()
        {
            // Get transform and collider of falling tree
            Transform tree = _fallingTree.transform;
            Collider treeCol = _fallingTree.GetComponent<Collider>();
            treeCol.enabled = false;

            // Check if impact effect exists
            bool hasImpactEffect = _treeImpactFX != null;

            // Spawn logs and apply force
            for (int i = 0; i < _logsCount; i++)
            {
                var logRigidB = Instantiate(_logPrefab, tree);
                var logTrs = logRigidB.transform;
                logTrs.SetParent(tree, false);
                logTrs.localPosition = i * _logsOffset * Vector3.up;
                logRigidB.AddForce(LOGS_FORCE * Mathf.Sign(Random.Range(-100f, 100f)) * logTrs.right, ForceMode.Impulse);
                logRigidB.transform.SetParent(null, true);

                // Spawn impact effect if available
                if (hasImpactEffect)
                    Instantiate(_treeImpactFX, logTrs.position, logTrs.rotation);
            }
            
            _fallingTree.gameObject.SetActive(false);
            
            // Apply camera shake and play impact audio
            var position = transform.position;
            ShakeEvents.DoShake(position, _shake, SHAKE_RADIUS);
            AudioManager.Instance.PlayClipAtPoint(_treeImpactAudio.Clip, position, _treeImpactAudio.Volume);
        }
        
        #region Save & Load
        void ISaveableComponent.LoadMembers(object data)
        {
            if (_gatherable.IsAlive)
                return;
            
            if (data is SerializedRigidbodyData saveData)
            {
                SerializedRigidbodyData.ApplyToRigidbody(_fallingTree, saveData);

                // Disable collision and start falling coroutine
                StartCoroutine(C_HandleFalling());
            }
            else
                _fallingTree.gameObject.SetActive(false);
        }

        object ISaveableComponent.SaveMembers()
        {
            if (_gatherable.IsAlive || !_isFalling)
                return null;

            return new SerializedRigidbodyData(_fallingTree);
        }
        #endregion
    }
}
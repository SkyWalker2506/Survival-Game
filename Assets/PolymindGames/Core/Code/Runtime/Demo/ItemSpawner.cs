using System.Collections;
using System.Collections.Generic;
using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.Demo
{
    [RequireComponent(typeof(BoxCollider))]
    public sealed class ItemSpawner : MonoBehaviour
    {
        [SerializeField, BeginGroup("Settings")]
        private Vector2Int _itemSpawnCount = new(1, 2);

        [SerializeField, Range(0f, 10f)]
        private float _spawnDelay = 0.5f;

        [SerializeField, Range(0f, 10f)]
        private float _consecutiveSpawnDelay = 0.1f;

        [SerializeField, Range(0f, 100f)]
        private float _itemDestroyDelay = 0f;

        [SerializeField, SpaceArea]
        private Vector3 _randomRotation = new(45f, 45f, 45);

        [SerializeField, Range(0f, 100f)]
        private float _positionForce = 2f;

        [SerializeField, Range(0f, 100f), EndGroup]
        private float _angularForce = 25f;

        [SerializeField, BeginGroup("Effects")]
        private ParticleSystem _spawnParticles;

        [SerializeField, EndGroup]
        private AudioDataSO _spawnAudio;

        [SerializeField, ReorderableList(ListStyle.Boxed), IgnoreParent]
        private ItemGenerator[] _itemsToSpawn;

        private BoxCollider _collider;


        public void SpawnItems()
        {
            int spawnCount = _itemSpawnCount.GetRandomFromRange();
            var itemsToSpawn = new List<GameObject>();

            for (int i = 0; i < spawnCount; i++)
            {
                ItemGenerator itemToSpawn = _itemsToSpawn.SelectRandom();
                ItemDefinition itemDef = itemToSpawn.GetItemDefinition();

                if (itemDef != null && itemDef.Pickup != null)
                    itemsToSpawn.Add(itemDef.Pickup.gameObject);
                else
                    spawnCount--;
            }

            StartCoroutine(C_SpawnItems(itemsToSpawn));
        }

        private void Awake() => _collider = GetComponent<BoxCollider>();

        private IEnumerator C_SpawnItems(List<GameObject> itemsToSpawn)
        {
            for (float timer = Time.time + _spawnDelay; timer > Time.time;)
                yield return null;

            if (_spawnAudio != null)
                AudioManager.Instance.PlayClipAtPoint(_spawnAudio.Clip, transform.position, _spawnAudio.Volume, _spawnAudio.Pitch);

            for (int i = 0; i < itemsToSpawn.Count; i++)
            {
                Quaternion spawnRotation = Quaternion.Euler(
                    Random.Range(-Mathf.Abs(_randomRotation.x), Mathf.Abs(_randomRotation.x)),
                    Random.Range(-Mathf.Abs(_randomRotation.y), Mathf.Abs(_randomRotation.y)),
                    Random.Range(-Mathf.Abs(_randomRotation.z), Mathf.Abs(_randomRotation.z))
                );

                GameObject pickup = Instantiate(itemsToSpawn[i], _collider.bounds.GetRandomPoint(), spawnRotation);

                if (_spawnParticles != null)
                    Instantiate(_spawnParticles, pickup.transform.position, spawnRotation);

                if (pickup.TryGetComponent(out Rigidbody rigidB))
                {
                    rigidB.linearVelocity = Random.insideUnitSphere.normalized * _positionForce;
                    rigidB.angularVelocity = spawnRotation.eulerAngles * _angularForce;
                }

                if (pickup != null && _itemDestroyDelay > 0.01f)
                    Destroy(pickup, _itemDestroyDelay);

                for (float timer = Time.time + _consecutiveSpawnDelay; timer > Time.time;)
                    yield return null;
            }
        }
    }
}
using System.Collections.Generic;
using PolymindGames.PoolingSystem;
using PolymindGames.WorldManagement;
using UnityEditor;
using UnityEngine;
using System;

namespace PolymindGames.ResourceGathering
{
    [RequireComponent(typeof(Collider))]
    public sealed class Gatherable : MonoBehaviour, IGatherable, ISaveableComponent
    {
        [SerializeField, InLineEditor, BeginGroup]
        private GatherableDefinition _definition;

        [SerializeField, SceneObjectOnly]
        private GameObject _dormant;

        [SerializeField, Range(0f, MAX_HEALTH), Disable, EndGroup]
        private float _health = MAX_HEALTH;

        private GameObject _engagedInstance;
        private int _respawnDaysLeft;

        public const float MAX_HEALTH = 100f;


        public float Health => _health;
        public bool IsAlive => _health > 0.01f;
        public GatherableDefinition Definition => _definition;
        
        public event DamageReceivedDelegate DamageReceived;

        public void ReceiveDamage(float damage, in DamageArgs args)
        {
            if (_engagedInstance == null)
                EnableEngaged();

            _health = Mathf.Max(_health - Mathf.Abs(damage), 0f);
            DamageReceived?.Invoke(damage, in args);

            if (!IsAlive && _definition.RespawnDays > 0)
            {
                _respawnDaysLeft = _definition.RespawnDays;
                World.Instance.Time.DayChanged += OnDayChanged;
            }
        }

        public void EnableCollision(bool enable)
        {
            if (TryGetComponent<Collider>(out var col))
                col.enabled = enable;
        }

        public Vector3 GetGatherPosition() => transform.TransformPoint(_definition.GatherOffset);

        private void OnDestroy()
        {
            if (_respawnDaysLeft > 0)
                World.Instance.Time.DayChanged -= OnDayChanged;
        }

        private void Respawn()
        {
            if (IsAlive)
            {
                Debug.LogError("Gatherable is not dead.");
                return;
            }
            
            GatherablePools.ReturnEngagedInstance(_definition, _engagedInstance);
            _engagedInstance = null;

            _health = MAX_HEALTH;
            _dormant.SetActive(true);
        }

        private void EnableEngaged()
        {
            if (_engagedInstance != null)
            {
                Debug.LogError("Gatherable is already engaged.");
                return;
            }

            _dormant.SetActive(false);
            _engagedInstance = GatherablePools.GetEngagedInstance(_definition, transform);
        }

        private void OnDayChanged(int day, int passed)
        {
            _respawnDaysLeft -= passed;
            
            if (_respawnDaysLeft <= 0)
            {
                _respawnDaysLeft = 0;
                World.Instance.Time.DayChanged -= OnDayChanged;
                Respawn();
            }
        }

        #region Save & Load
        [Serializable]
        private class SaveData
        {
            public int RespawnDaysLeft;
            public float Health;
        }
        
        void ISaveableComponent.LoadMembers(object data)
        {
            var saveData = (SaveData)data;
            _health = saveData.Health;

            if (Mathf.Abs(_health - MAX_HEALTH) < 0.01f)
                return;

            EnableEngaged();

            if (!IsAlive)
            {
                _respawnDaysLeft = saveData.RespawnDaysLeft;
                World.Instance.Time.DayChanged += OnDayChanged;
            }
        }

        object ISaveableComponent.SaveMembers() => new SaveData()
        {
            RespawnDaysLeft = _respawnDaysLeft,
            Health = _health
        };
        #endregion

        #region Internal
        private static class GatherablePools
        {
            static GatherablePools()
            {
                ScenePools.ScenePoolHandlerDestroyed += ClearPools;
            }
            
            private static void ClearPools()
            {
                foreach (var pool in s_EngagedPools.Values)
                    pool.Clear();
                
                s_EngagedPools.Clear();
            }

            private static readonly Dictionary<GatherableDefinition, UnityEngine.Pool.ObjectPool<GameObject>> s_EngagedPools = new();

            
            private static Transform s_SpawnRoot;
            private static Transform SpawnRoot
            {
                get
                {
                    if (s_SpawnRoot == null)
                    {
                        var rootObj = new GameObject("Gatherables");
                        rootObj.transform.SetParent(ScenePools.ScenePoolHandlerRoot);
                        s_SpawnRoot = rootObj.transform;
                    }

                    return s_SpawnRoot;
                }
            }

            public static GameObject GetEngagedInstance(GatherableDefinition definition, Transform parent)
            {
                GameObject engaged = GetEngaged(definition);
                var trs = engaged.transform;
                trs.SetParent(parent);
                trs.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                engaged.SetActive(true);
                return engaged;
            }

            private static GameObject GetEngaged(GatherableDefinition definition)
            {
                GameObject engaged;
                if (s_EngagedPools.TryGetValue(definition, out var pool))
                    engaged = pool.Get();
                else
                {
                    var instance = Instantiate(definition.EngagedPrefab, SpawnRoot);
                    instance.SetActive(false);

                    pool = new UnityEngine.Pool.ObjectPool<GameObject>(() => Instantiate(instance),
                        null, null, null, true, 8, 32);

                    s_EngagedPools.Add(definition, pool);
                    engaged = pool.Get();
                    
                    ScenePools.ScenePoolHandlerDestroyed += pool.Clear;
                }
                
                return engaged;
            }

            public static void ReturnEngagedInstance(GatherableDefinition definition, GameObject instance)
            {
                if (s_EngagedPools.TryGetValue(definition, out var pool))
                {
                    instance.transform.SetParent(SpawnRoot);
                    instance.SetActive(false);
                    pool.Release(instance);
                }
            }
        }
        #endregion

        #region Editor
#if UNITY_EDITOR
        private void Reset()
        {
            gameObject.SetLayersInChildren(LayerConstants.DYNAMIC_OBJECTS);
            _health = MAX_HEALTH;
        }

        private void OnDrawGizmosSelected()
        {
            if (Event.current.type == EventType.Repaint && _definition != null)
            {
                Vector3 gatherPoint = GetGatherPosition();

                Handles.CircleHandleCap(0, gatherPoint, Quaternion.LookRotation(Vector3.up), _definition.GatherRadius, EventType.Repaint);

                Handles.color = new Color(1f, 0f, 0f, 0.5f);
                Handles.SphereHandleCap(0, gatherPoint, Quaternion.identity, 0.1f, EventType.Repaint);
                Handles.color = Color.white;

                Handles.Label(gatherPoint, "Gather Position");
            }
        }
#endif
        #endregion
    }
}
using PolymindGames.UserInterface;
using System.Collections;
using UnityEngine;
using System;

namespace PolymindGames
{
    using Random = UnityEngine.Random;
    
    [DefaultExecutionOrder(ExecutionOrderConstants.SCENE_SINGLETON)]
    public abstract class GameMode : MonoBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        private GameObject _sceneCamera;

        [SerializeField, PrefabObjectOnly, BeginGroup]
        private Player _playerPrefab;

        [SerializeField, PrefabObjectOnly, EndGroup]
        private PlayerUI _playerUIPrefab;

        [SerializeField, Range(0f, 1f), BeginGroup]
        private float _spawnRotationRandomness = 0.15f;

        [SerializeField, EndGroup]
        [ReorderableList(ListStyle.Lined, HasLabels = false, Foldable = true)]
        private Transform[] _spawnPoints = Array.Empty<Transform>();

        private Player _player;
        private PlayerUI _playerUI;


        protected Player Player => _player;
        protected PlayerUI PlayerUI => _playerUI;
        
        private IEnumerator Start()
        {
            Player.LocalPlayerChanged += DestroySceneCamera;
            
            yield return null;
            
            // Player set up.
            if (TryGetPlayer(out _player))
            {
                _player.SetName(PlayerNameUI.GetPlayerName());
                
                TryGetPlayerUI(out _playerUI);

                yield return null;
                Player.LocalPlayerChanged -= DestroySceneCamera;

                _playerUI.AttachToCharacter(_player);
                OnPlayerInitialized(_player, _playerUI);
            } 
            
            void DestroySceneCamera()
            {
                if (_sceneCamera != null)
                    Destroy(_sceneCamera.gameObject);
            }
        }
        
        protected virtual void OnPlayerInitialized(Player player, PlayerUI playerUI) { }

        private bool TryGetPlayer(out Player player)
        {
            player = Player.LocalPlayer;
            
            if (player != null)
            {
                _playerPrefab = null;
                return true;
            }

            if (_playerPrefab != null)
            {
                player = Instantiate(_playerPrefab);
                _playerPrefab = null;
                return true;
            }

            Debug.LogError("The Player prefab is null, you need to assign one in the inspector.", gameObject);
            return false;
        }

        private bool TryGetPlayerUI(out PlayerUI playerUi)
        {
            playerUi = PlayerUI.LocalUI;

            if (playerUi != null)
            {
                _playerUIPrefab = null;
                return true;
            }

            if (_playerUIPrefab != null)
            {
                playerUi = Instantiate(_playerUIPrefab);
                _playerUIPrefab = null;
                return true;
            }

            Debug.LogWarning("The Player UI prefab is null, you may want to assign one in the inspector.", gameObject);
            return false;
        }

        protected void SetPlayerPosition(Vector3 position, Quaternion rotation)
        {
            // Sets the player's position and rotation.
            if (_player.TryGetCC(out IMotorCC motor))
                motor.Teleport(position, rotation, true);
        }

        public (Vector3 position, Quaternion rotation) GetRandomSpawnPoint()
        {
            // Search for random spawn point.
            Transform spawnPoint = _spawnPoints.Length > 0 ? _spawnPoints.SelectRandom() : transform;

            Vector3 position = spawnPoint.position;

            float yAngle = Random.Range(0f, 360f);
            Quaternion randomRotation = Quaternion.Euler(0f, yAngle, 0f);
            Quaternion rotation = Quaternion.Lerp(spawnPoint.rotation, randomRotation, _spawnRotationRandomness);

            return (position, rotation);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_sceneCamera == null)
            {
                var cam = GetComponentInChildren<Camera>(true);
                if (cam != null)
                    _sceneCamera = cam.gameObject;
            }
        }

        private void OnDrawGizmos()
        {
            if (_spawnPoints.Length == 0)
            {
                SnapSpawnPointToGround(transform);
                DrawSpawnPointGizmo(transform);
            }
            else
            {
                foreach (var spawnPoint in _spawnPoints)
                {
                    SnapSpawnPointToGround(spawnPoint);
                    DrawSpawnPointGizmo(spawnPoint);
                }
            }
        }

        private static void SnapSpawnPointToGround(Transform spawnPoint)
        {
            // Snaps the spawn point position to the ground.
            if (Physics.Raycast(spawnPoint.position + Vector3.up * 0.25f, Vector3.down, out RaycastHit hitInfo, 10f)
                || Physics.Raycast(spawnPoint.position + Vector3.up, Vector3.down, out hitInfo, 10f))
            {
                spawnPoint.position = hitInfo.point;
            }
        }

        private static void DrawSpawnPointGizmo(Transform spawnPoint)
        {
            Color prevColor = Gizmos.color;
            Gizmos.color = new Color(0.1f, 0.9f, 0.1f, 0.35f);

            const float GIZMO_WIDTH = 0.5f;
            const float GIZMO_HEIGHT = 1.8f;

            Vector3 position = spawnPoint.position;
            Gizmos.DrawCube(new Vector3(position.x, position.y + GIZMO_HEIGHT / 2, position.z), new Vector3(GIZMO_WIDTH, GIZMO_HEIGHT, GIZMO_WIDTH));

            Gizmos.color = prevColor;
        }
#endif
    }
}
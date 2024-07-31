using UnityEngine.SceneManagement;
using System.Collections.Generic;
using PolymindGames.InputSystem;
using PolymindGames.SaveSystem;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine.Events;
using UnityEngine;
using System;

#if SURVIVAL_TEMPLATE_PRO
using PolymindGames.WorldManagement;
#endif

namespace PolymindGames
{
    [DefaultExecutionOrder(ExecutionOrderConstants.SCRIPTABLE_SINGLETON)]
    [CreateAssetMenu(menuName = MANAGERS_MENU_PATH + "Level Manager", fileName = nameof(LevelManager))]
    public sealed class LevelManager : Manager<LevelManager>
    {
        [SerializeField, Range(1f, 10000f), BeginGroup("Autosave")]
        [Tooltip("Delay in seconds between autosaves.")]
        private float _autosaveDelay = 120f;

        [SerializeField, PrefabObjectOnly, NotNull]
        [Tooltip("Prefab of the loading screen.")]
        private LoadScreen _loadScreen;

        [SerializeField, EndGroup]
        [Tooltip("Input context for loading.")]
        private InputContext _loadContext;

        private readonly List<SaveableObject> _sceneSaveables = new(256);
        private LoadScreen _runtimeLoadScreen;
        private Coroutine _autoSaveRoutine;
        private float _autosaveTimer;
        private int _saveFileIndex;
        private bool _isLoading;
        private bool _isSaving;
        
        private const int DEFAULT_SAVE_FILE_INDEX = -1;
        
        
        #region Initialization
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
#else
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void Init() => LoadOrCreateInstance();

        protected override void OnInitialized()
        {
#if UNITY_EDITOR
            _isLoading = false;
            _isSaving = false;
            _sceneSaveables.Clear();
#endif
            
            _saveFileIndex = DEFAULT_SAVE_FILE_INDEX;
            _autosaveTimer = Time.time + _autosaveDelay;
            
            _runtimeLoadScreen = Instantiate(_loadScreen, GetManagersRoot().transform);
            _runtimeLoadScreen.name = "LevelRuntimeComponent";

            InitAutoSave();
        }
        #endregion

        public bool IsSaving => _isSaving;
        public bool IsLoading => _isLoading;
        public string MainMenuScene => "MainMenu";

        public event UnityAction LevelLoadStart;
        public event UnityAction LevelLoaded;
        public event UnityAction GameSaveStart;
        public event UnityAction GameSaved;

        public void RegisterSaveable(SaveableObject saveable)
        {
            _sceneSaveables.Add(saveable);
        }

        public void UnregisterSaveable(SaveableObject saveable)
        {
            if (!_sceneSaveables.Remove(saveable))
            {
#if DEBUG
                Debug.LogWarning($"Saveable ({saveable.name}) is not registered, no need for un-registering!" +
                                 $" If you did not get this warning while editing a prefab in playmode something might have gone wrong.");
#endif
            }
        }
        
        public bool TryLoadScene(string sceneName)
        {
            if (!IsBusy())
            {
                _runtimeLoadScreen.StartCoroutine(C_LoadScene(sceneName));
                return true;
            }

            return false;
        }

        public bool TryLoadGame(int saveFileIndex)
        {
            if (!IsBusy() && SaveLoadManager.Instance.SaveFileExists(saveFileIndex))
            {
                _runtimeLoadScreen.StartCoroutine(C_LoadGame(saveFileIndex));
                return true;
            }
            
            return false;
        }
        
        public bool TrySaveCurrentGame()
        {
            return _saveFileIndex != DEFAULT_SAVE_FILE_INDEX
                   && TrySaveCurrentGameToIndex(DEFAULT_SAVE_FILE_INDEX);
        }
        
        public bool TrySaveCurrentGameToIndex(int saveFileIndex)
        {
            if (!IsBusy())
            {
                _runtimeLoadScreen.StartCoroutine(C_SaveCurrentGame(saveFileIndex));
                return true;
            }

            return false;
        }

        private IEnumerator C_LoadScene(string sceneName)
        {
            _isLoading = true;
            LevelLoadStart?.Invoke();
            InputManager.Instance.PushContext(_loadContext);
            
            yield return _runtimeLoadScreen.FadeIn();

            // Load the scene
            AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            while (!sceneLoadOperation.isDone)
            {
                // float loadingProgress = sceneLoadOperation.progress;
                yield return null;
            }

            InputManager.Instance.PopContext(_loadContext);
            _runtimeLoadScreen.FadeOut();
            
            _isLoading = false;
            LevelLoaded?.Invoke();

            ResetAutoSaveTimer();
        }

        private void ResetAutoSaveTimer() => _autosaveTimer = Time.unscaledTime + _autosaveDelay;

        private IEnumerator C_LoadGame(int saveId)
        {
            var saveData = SaveLoadManager.Instance.LoadSaveDataFromFile(saveId);

            if (saveData == null)
            {
                Debug.LogError($"Game with save id ''{saveId}'' is invalid.");
                yield break;
            }

            var sceneData = saveData.Data[0];

            yield return C_LoadScene(sceneData.Scene);
            
            // Spawn the player, UI, and others
            LoadSaveables(sceneData);
        }

        private IEnumerator C_SaveCurrentGame(int saveId)
        {
            _isSaving = true;
            GameSaveStart?.Invoke();
            _runtimeLoadScreen.ShowLoadIcon();

            yield return new WaitForEndOfFrame();

            var data = new ObjectSaveData[_sceneSaveables.Count];
            for (int i = 0; i < _sceneSaveables.Count; i++)
                data[i] = _sceneSaveables[i].GetSaveData();
            
            string sceneName = SceneManager.GetActiveScene().name;
            var sceneData = new SceneSaveData(sceneName, data);
            var saveInfo = CreateGameSaveInfo(saveId, sceneName);
            
            yield return null;

            var saveData = new GameSaveData(saveInfo,new [] { sceneData });
            var task = Task.Run(() => SaveLoadManager.Instance.SaveDataToFile(saveData));
            yield return new WaitUntil(() => task.IsCompleted);

            _runtimeLoadScreen.HideSaveIcon();
            _isSaving = false;
            GameSaved?.Invoke();
                
            ResetAutoSaveTimer();
        }

        private void LoadSaveables(SceneSaveData sceneSaveData)
        {
            var saveableObjectData = sceneSaveData.Data;
            for (int i = _sceneSaveables.Count - 1; i >= 0; i--)
            {
                var sceneSaveable = _sceneSaveables[i];
                var sceneSaveableGuid = sceneSaveable.InstanceGuid;
                var saveableIndex = -1;

                for (int j = 0; j < saveableObjectData.Length; j++)
                {
                    if (saveableObjectData[j].InstanceGuid == sceneSaveableGuid)
                    {
                        saveableIndex = j;
                        break;
                    }
                }

                if (saveableIndex != -1)
                    sceneSaveable.LoadData(saveableObjectData[saveableIndex]);
                else
                {
                    Destroy(sceneSaveable.gameObject);
                }
            }

            foreach (var objData in saveableObjectData)
            {
                if (!_sceneSaveables.Exists(sceneSaveable => sceneSaveable.InstanceGuid == objData.InstanceGuid))
                {
                    SaveableObject prefab = SaveLoadManager.Instance.GetPrefabWithGuid(ref objData.PrefabGuid);
                    SaveableObject instance = Instantiate(prefab);
                    instance.LoadData(objData);
                }
            }
        }
        
        private void InitAutoSave()
        {
            _runtimeLoadScreen.Started += () =>
            {
                var autosaveEnabled = GameplayOptions.Instance.AutosaveEnabled;
                autosaveEnabled.Changed += OnAutosaveSettingChanged;
                OnAutosaveSettingChanged(autosaveEnabled.Value);
            };
            
            _runtimeLoadScreen.Destroyed += () => GameplayOptions.Instance.AutosaveEnabled.Changed -= OnAutosaveSettingChanged;

            return;
            
            void OnAutosaveSettingChanged(bool enabled)
            {
                if (enabled)
                {
                    if (_autoSaveRoutine != null)
                    {
                        _runtimeLoadScreen.StopCoroutine(_autoSaveRoutine);
                        _autoSaveRoutine = null;
                    }
                    else
                        _autoSaveRoutine = _runtimeLoadScreen.StartCoroutine(AutoSave());
                }
                else
                {
                    if (_autoSaveRoutine != null)
                    {
                        _runtimeLoadScreen.StopCoroutine(_autoSaveRoutine);
                        _autoSaveRoutine = null;
                    }
                }
            }
            
            IEnumerator AutoSave()
            {
                while (true)
                {
                    if (Time.time < _autosaveTimer || _saveFileIndex == DEFAULT_SAVE_FILE_INDEX)
                        yield return null;
                    else
                    {
                        _autosaveTimer = Time.time + _autosaveDelay;

                        if (!_isSaving && !_isLoading)
                            yield return C_SaveCurrentGame(_saveFileIndex);
                    }
                }
            }
        }
        
        private bool IsBusy() => _isLoading | _isSaving;

        private static GameSaveInfo CreateGameSaveInfo(int saveId, string scene)
        {
            var screenshot = TakeScreenshot();
            
#if SURVIVAL_TEMPLATE_PRO
            var gameTime = World.Instance.Time.GetGameTime();
            var saveInfo = new GameSaveInfo(saveId, scene, DateTime.Now, GameDifficulty.Standard, gameTime, screenshot);
#else
            var saveInfo = new GameSaveInfo(saveId, scene, DateTime.Now, GameDifficulty.Standard, screenshot);
#endif
            return saveInfo;
        }
        
        private static Texture2D TakeScreenshot()
        {
            int width = Screen.width;
            int height = Screen.height;

            Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            Rect rect = new Rect(0, 0, width, height);
            texture.ReadPixels(rect, 0, 0);
            texture.Apply();

            return texture;
        }
    }
}
using PolymindGames.OdinSerializer;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;

namespace PolymindGames.SaveSystem
{
    using SerializationUtility = OdinSerializer.SerializationUtility;
    using Debug = UnityEngine.Debug;
    
    [DefaultExecutionOrder(ExecutionOrderConstants.SCRIPTABLE_SINGLETON)]
    [CreateAssetMenu(menuName = MANAGERS_MENU_PATH + "Save & Load Manager", fileName = ASSET_NAME)]
    public sealed class SaveLoadManager : Manager<SaveLoadManager>
    {
        [EditorButton(nameof(LoadAllSaveablePrefabs))]
        [SerializeField, ScrollableItems(0, 10), Disable]
        [Help("Every prefab with a ''Saveable Object'' script attached (Auto filled).")]
        private SaveableObject[] _saveablePrefabs;
        
        private Dictionary<SerializedGuid, SaveableObject> _prefabsDict;
        private bool _isSavingOrLoading;
        
        private static string s_SaveDirectoryPath;
        
        private const int MAX_SAVE_SLOTS = 10;
        private const string ASSET_NAME = "SaveLoadManager";
        private const string SAVE_FILE_NAME = "Save";
        private const string SAVE_DATA_FILE_EXTENSION = ".sav";
        private const string SAVE_INFO_FILE_EXTENSION = ".meta";
        
        
        #region Initialization
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Init() => LoadOrCreateInstance();

        protected override void OnInitialized()
        {
#if UNITY_EDITOR
            if (UnityUtils.IsFirstPlayMode)
                LoadAllSaveablePrefabs();
#endif

            s_SaveDirectoryPath = GetSaveDirectory();

            if (_prefabsDict == null)
            {
                _prefabsDict = new Dictionary<SerializedGuid, SaveableObject>(_saveablePrefabs.Length);
                foreach (var saveable in _saveablePrefabs)
                    _prefabsDict.Add(saveable.PrefabGuid, saveable);
            }

#if !DEBUG
			_saveablePrefabs = null;
#endif
        }
		#endregion
        
        public SaveableObject GetPrefabWithGuid(ref SerializedGuid guid)
        {
#if DEBUG
            if (_prefabsDict.TryGetValue(guid, out var sav))
                return sav;

            Debug.LogError($"No saveable object with the prefab GUID: {guid} has been found.");
            return null;
#else
			return _prefabsDict[guid];
#endif
        }

        public bool SaveFileExists(int saveFileIndex) => File.Exists(GetSaveDataPath(saveFileIndex));

        public void SaveDataToFile(GameSaveData gameSaveData)
        {
            if (_isSavingOrLoading)
            {
                Debug.LogError("Cannot save to file while saving or loading.");
                return;
            }

            _isSavingOrLoading = true;
            
            int saveId = gameSaveData.Info.SaveId;
            
            string saveInfoPath = GetSaveInfoPath(saveId); 
            SaveToFile(gameSaveData.Info, saveInfoPath);
            
            string sceneDataPath = GetSaveDataPath(saveId);
            SaveToFile(gameSaveData.Data, sceneDataPath);

            _isSavingOrLoading = false;
        }

        public GameSaveData LoadSaveDataFromFile(int saveId)
        {
            if (_isSavingOrLoading)
            {
                Debug.LogError("Cannot load from file while saving or loading.");
                return null;
            }
            
            _isSavingOrLoading = true;
            
            var saveInfo = LoadSaveInfo(saveId);
            if (saveInfo == null)
                Debug.LogError($"No save info with id {saveId} found.");
            
            var sceneSaveData = LoadSceneSaveData(saveId);
            if (sceneSaveData == null)
                Debug.LogError($"No scene save data with id {saveId} found.");

            _isSavingOrLoading = false;
            
            return new GameSaveData(saveInfo, sceneSaveData);
        }
        
        public SceneSaveData[] LoadSceneSaveData(int saveId)
        {
            string filePath = GetSaveDataPath(saveId);
            return LoadFromFile<SceneSaveData[]>(filePath);
        }

        public GameSaveInfo LoadSaveInfo(int saveId)
        {
            string filePath = GetSaveInfoPath(saveId);
            return LoadFromFile<GameSaveInfo>(filePath);
        }
        
        public List<GameSaveInfo> LoadSavesInfo(int count)
        {
            if (count < 0 || count > MAX_SAVE_SLOTS)
            {
                Debug.LogError($"The max save files count is {MAX_SAVE_SLOTS}. You're trying to load {count} of them.");
                return null;
            }

            var saves = new List<GameSaveInfo>();
            for (int i = 0; i < count; i++)
            {
                var saveInfo = LoadSaveInfo(i);
                if (saveInfo != null)
                    saves.Add(saveInfo);
            }

            return saves;
        }

        public void DeleteSaveFile(int saveId)
        {
            string filePath;
            
            filePath = GetSaveDataPath(saveId);
            if (File.Exists(filePath))
                File.Delete(filePath);
            
            filePath = GetSaveInfoPath(saveId);
            if (File.Exists(filePath))
                File.Delete(filePath);
        }

        private static T LoadFromFile<T>(string filePath) where T : class
        {
            if (File.Exists(filePath))
            {
                byte[] bytes = File.ReadAllBytes(filePath);
                T data = SerializationUtility.DeserializeValue<T>(bytes, DataFormat.Binary);
                return data;
            }

            return null;
        }
        
        private static void SaveToFile<T>(T data, string filePath) where T : class
        {
            using Stream stream = File.Open(filePath, FileMode.Create);
            var context = new SerializationContext();
            var writer = new BinaryDataWriter(stream, context);
            SerializationUtility.SerializeValue(data, writer);
        }

        private static string GetSavePathForId(int saveId, string extension)
            => Path.Combine(s_SaveDirectoryPath, $"{SAVE_FILE_NAME}{saveId}{extension}");

        private static string GetSaveDataPath(int saveId)
            => GetSavePathForId(saveId, SAVE_DATA_FILE_EXTENSION);
        
        private static string GetSaveInfoPath(int saveId)
            => GetSavePathForId(saveId, SAVE_INFO_FILE_EXTENSION);
        
        private static string GetSaveDirectory()
        {
            s_SaveDirectoryPath ??= Path.Combine(Application.persistentDataPath, "Saves");
            
            if (!Directory.Exists(s_SaveDirectoryPath))
                Directory.CreateDirectory(s_SaveDirectoryPath);

            return s_SaveDirectoryPath;
        }

        #region Editor
        [Conditional("UNITY_EDITOR")]
        private void LoadAllSaveablePrefabs()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;
            
            var saveablePrefabs = new List<SaveableObject>();
            var allPrefabs = AssetDatabase.FindAssets("t:prefab");

            foreach (var prefabGuid in allPrefabs)
            {
                var gameObject = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(prefabGuid));

                if (gameObject.TryGetComponent<SaveableObject>(out var saveable))
                {
                    Guid guid = new Guid(prefabGuid);
                    if (saveable.PrefabGuid != guid)
                    {
                        EditorUtility.SetDirty(saveable);
                        saveable.PrefabGuid = guid;
                    }

                    saveablePrefabs.Add(saveable);
                }
            }

            _saveablePrefabs = saveablePrefabs.ToArray();
            EditorUtility.SetDirty(this);
#endif
        }
        #endregion
    }
}
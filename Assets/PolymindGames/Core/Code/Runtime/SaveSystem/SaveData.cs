using PolymindGames.WorldManagement;
using UnityEngine;
using System;

namespace PolymindGames.SaveSystem
{
    public enum GameDifficulty
    {
        Easy,
        Standard,
        Difficult,
        Expert
    }

    [Serializable]
    public sealed class GameSaveData
    {
        [SerializeField]
        private GameSaveInfo _gameSaveInfo;

        [SerializeField]
        private SceneSaveData[] _saveData;

        public GameSaveData(GameSaveInfo gameSaveInfo, SceneSaveData[] saveData)
        {
            _gameSaveInfo = gameSaveInfo;
            _saveData = saveData;
        }

        public SceneSaveData[] Data => _saveData;
        public GameSaveInfo Info => _gameSaveInfo;
    }
    
    [Serializable]
    public sealed class SceneSaveData
    {
        [SerializeField]
        private string _scene;
        
        [SerializeField]
        private ObjectSaveData[] _objectData;

        public SceneSaveData(string scene, ObjectSaveData[] objectData)
        {
            _scene = scene;
            _objectData = objectData;
        }

        public string Scene => _scene;
        public ObjectSaveData[] Data => _objectData;
    }
    
    [Serializable]
    public sealed class ObjectSaveData
    {
        public SerializedGuid PrefabGuid;
        public SerializedGuid InstanceGuid;
        public SerializedTransformData Transform;
        public SerializedTransformData[] Transforms;
        public SerializedComponentData[] Components;
        public SerializedRigidbodyData[] Rigidbodies;
    }
    
    [Serializable]
    public sealed class GameSaveInfo
    {
        [SerializeField]
        private int _saveId;

        [SerializeField]
        private string _scene;

        [SerializeField]
        private SerializedImage _screenshot;
        
        [SerializeField]
        private SerializedDateTime _dateTime;

        [SerializeField]
        private GameDifficulty _difficulty;
        
#if SURVIVAL_TEMPLATE_PRO
        [SerializeField]
        private GameTime _gameTime;
#endif
        

        public GameSaveInfo(int saveId, string scene, DateTime time, GameDifficulty difficulty, Texture2D screenshot = null)
        {
            _saveId = saveId;
            _scene = scene;
            _dateTime = time;
            _difficulty = difficulty;
            _screenshot = new SerializedImage(screenshot);
        }
        
#if SURVIVAL_TEMPLATE_PRO
        public GameSaveInfo(int saveId, string scene, DateTime time, GameDifficulty difficulty, GameTime gameTime, Texture2D screenshot = null)
            : this(saveId, scene, time, difficulty, screenshot)
        {
            _gameTime = gameTime;
        }
#endif
        
        public int SaveId => _saveId;
        public string Scene => _scene;
        public DateTime DateTime => _dateTime;
        public GameDifficulty Difficulty => _difficulty;
        public Texture2D Screenshot => _screenshot;
        
#if SURVIVAL_TEMPLATE_PRO
        public GameTime GameTime => _gameTime;
#endif
    }
}
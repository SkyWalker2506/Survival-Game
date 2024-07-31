using UnityEngine;

namespace PolymindGames.SaveSystem
{
    [ExecuteAlways, DisallowMultipleComponent]
    public class SaveableObject : GuidComponent
    {
        [SerializeField, EndGroup] 
        [SerializedGuidDetails(false, false)]
        private SerializedGuid _prefabGuid;

        [SerializeField, BeginGroup, EndGroup]
        private SerializedTransformData.SaveFlags _rootSaveFlags =
            SerializedTransformData.SaveFlags.Position | SerializedTransformData.SaveFlags.Rotation;

        [SerializeField, NewLabel("Transforms To Save"), ReorderableList(ListStyle.Boxed, HasLabels = false)]
        [Tooltip("Here you can specify which transforms to save. The corresponding transform of this component will always be implicitly added to the list.")]
        private Transform[] _childrenToSave;

        [SerializeField, ReorderableList(ListStyle.Boxed, HasLabels = false)]
        private Rigidbody[] _rigidbodiesToSave;

        private bool _isRegistered = false;
        private bool _isSaveable = true;


        public bool IsSaveable
        {
            get => _isSaveable;
            set
            {
                if (_isSaveable == value)
                    return;

                _isSaveable = value;
                
                if (value) RegisterSaveable();
                else UnregisterSaveable();
            }
        }

        public SerializedGuid PrefabGuid
        {
            get => _prefabGuid;
            set => _prefabGuid = value;
        }

        public ObjectSaveData GetSaveData()
        {
            var cachedTransform = transform;
            return new ObjectSaveData
            {
                PrefabGuid = _prefabGuid,
                InstanceGuid = _instanceGuid,
                Transform = new SerializedTransformData(cachedTransform),
                Components = SerializedComponentData.ExtractFromObject(cachedTransform),
                Transforms = SerializedTransformData.ExtractFromTransforms(_childrenToSave),
                Rigidbodies = SerializedRigidbodyData.ExtractFromRigidbodies(_rigidbodiesToSave)
            };
        }

        public void LoadData(ObjectSaveData saveData)
        {
            _instanceGuid = saveData.InstanceGuid;
            var cachedTransform = transform;

            SerializedTransformData.ApplyToTransform(cachedTransform, saveData.Transform, _rootSaveFlags);
            SerializedTransformData.ApplyToTransforms(_childrenToSave, saveData.Transforms);
            SerializedComponentData.ApplyToObject(cachedTransform, saveData.Components);
            SerializedRigidbodyData.ApplyToRigidbodies(_rigidbodiesToSave, saveData.Rigidbodies);
        }

        private void Start() => RegisterSaveable();

        protected override void OnDestroy()
        {
            base.OnDestroy();
            UnregisterSaveable();
        }

        protected void RegisterSaveable()
        {
            if (_isRegistered || !Application.isPlaying || !_isSaveable)
                return;
            
            _isRegistered = true;
            LevelManager.Instance.RegisterSaveable(this);
        }

        protected void UnregisterSaveable()
        {
            if (!_isRegistered)
                return;

            _isRegistered = false;
            LevelManager.Instance.UnregisterSaveable(this);
        }
    }
}
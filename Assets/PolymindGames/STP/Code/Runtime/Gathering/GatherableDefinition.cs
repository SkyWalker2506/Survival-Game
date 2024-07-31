using UnityEngine;

namespace PolymindGames.ResourceGathering
{
    [CreateAssetMenu(menuName = "Polymind Games/Gathering/Gatherable Definition", fileName = "GatherableDef_")]
    public sealed class GatherableDefinition : ScriptableObject
    {
        [SerializeField, NewLabel("Name"), BeginGroup]
        private string _gatherableName;

        [SerializeField, SpritePreview, EndGroup]
        private Sprite _icon;

        [SerializeField, PrefabObjectOnly, BeginGroup, EndGroup]
        private GameObject _engagedPrefab;

        [SerializeField, Range(0, 1000), BeginGroup]
        private int _respawnDays = 1;

        [SerializeField]
        private Vector3 _gatherOffset;

        [SerializeField, Range(0.1f, 1f), EndGroup]
        private float _gatherRadius = 0.35f;

        
        public string Name => _gatherableName;
        public Sprite Icon => _icon;
        public Vector3 GatherOffset => _gatherOffset;
        public GameObject EngagedPrefab => _engagedPrefab;
        public float GatherRadius => _gatherRadius;
        public int RespawnDays => _respawnDays;
    }
}
using UnityEngine;

namespace PolymindGames.BuildingSystem
{
    [DefaultExecutionOrder(ExecutionOrderConstants.SCRIPTABLE_SINGLETON)]
    [CreateAssetMenu(menuName = MANAGERS_MENU_PATH + "Building Manager", fileName = nameof(BuildingManager))]
    public sealed class BuildingManager : Manager<BuildingManager>
    {
        [SerializeField, PrefabObjectOnly, BeginGroup, EndGroup]
        private BuildingPieceGroup _defaultGroupPrefab;
        
        [SerializeField, BeginGroup("Masks")]
        private LayerMask _freePlacementMask;
        
        [SerializeField, EndGroup]
        private LayerMask _overlapCheckMask;

        [SerializeField, BeginGroup("Materials")]
        private MaterialEffectSO _placementAllowedMaterial;

        [SerializeField, EndGroup]
        private MaterialEffectSO _placementDeniedMaterialEffect;

        [SerializeField, BeginGroup("Audio"), EndGroup]
        private EffectPairSO _fullBuildEffect;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
        private static void Init() => LoadOrCreateInstance();
        
        public LayerMask FreePlacementMask => _freePlacementMask;
        public LayerMask OverlapCheckMask => _overlapCheckMask;
        public EffectPairSO FullBuildEffect => _fullBuildEffect;
        public BuildingPieceGroup DefaultGroupPrefab => _defaultGroupPrefab;
        public MaterialEffectSO PlacementAllowedMaterialEffect => _placementAllowedMaterial;
        public MaterialEffectSO PlacementDeniedMaterialEffect => _placementDeniedMaterialEffect;
    }
}
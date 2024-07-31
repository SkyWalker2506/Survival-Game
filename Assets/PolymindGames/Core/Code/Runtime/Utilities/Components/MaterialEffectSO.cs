using UnityEngine;

namespace PolymindGames
{
    [CreateAssetMenu(menuName = "Polymind Games/Utilities/Material Effect Info", fileName = "MaterialEffect_")]
    public sealed class MaterialEffectSO : ScriptableObject
    {
        public enum EffectType : byte
        {
            StackWithBaseMaterials,
            ReplaceBaseMaterials
        }

        [SerializeField, BeginGroup]
        private EffectType _effectMode;

        [SerializeField]
        private Material _material;

        [SerializeField, EndGroup]
        private bool _castShadows = true;


        public Material Material => _material;
        public bool CastShadows => _castShadows;
        public EffectType EffectMode => _effectMode;
    }
}
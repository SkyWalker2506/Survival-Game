using UnityEngine.Rendering;
using System.Linq;
using UnityEngine;
using System;

namespace PolymindGames
{
    /// <summary>
    /// Manages the application of material effects to renderers.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class MaterialEffect : MonoBehaviour
    {
        [Tooltip("The default material effect to apply.")]
        [SerializeField, BeginGroup, EndGroup]
        private MaterialEffectSO _defaultEffect;

        [Tooltip("The renderers to apply material effects to.")]
        [EditorButton(nameof(GetAllRenderers))]
        [SerializeField, ReorderableList(ListStyle.Boxed, HasLabels = false)]
        private Renderer[] _renderers;

        private MaterialEffectSO _activeEffect;
        private Material[][] _baseMaterials;
        

        /// <summary>
        /// Enables the specified material effect.
        /// </summary>
        /// <param name="effect">The material effect to enable.</param>
        public void EnableEffect(MaterialEffectSO effect = null)
        {
            if (effect == null)
            {
                if (_defaultEffect == null)
                {
                    DisableEffect();
                    return;
                }

                effect = _defaultEffect;
            }
            
            SetEffect(effect);
        }

        /// <summary>
        /// Disables the currently active material effect.
        /// </summary>
        public void DisableEffect()
        {
            if (_activeEffect == null)
                return;

            for (int i = 0; i < _renderers.Length; i++)
            {
                _renderers[i].sharedMaterials = _baseMaterials[i];
                _renderers[i].shadowCastingMode = ShadowCastingMode.On;
            }

            _activeEffect = null;
        }

        /// <summary>
        /// Sets the specified material effect.
        /// </summary>
        /// <param name="effect">The material effect to set.</param>
        private void SetEffect(MaterialEffectSO effect)
        {
            if (_activeEffect == effect)
                return;

            _baseMaterials ??= GetBaseMaterials();
            _activeEffect = effect;

            switch (effect.EffectMode)
            {
                case MaterialEffectSO.EffectType.StackWithBaseMaterials:
                    ApplyStackWithBaseMaterials(effect.Material, effect.CastShadows);
                    return;
                
                case MaterialEffectSO.EffectType.ReplaceBaseMaterials:
                    ApplyReplaceBaseMaterials(effect.Material, effect.CastShadows);
                    return;
                
                default: throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Applies the stack-with-base-materials effect mode.
        /// </summary>
        /// <param name="material">The material apply.</param>
        /// <param name="castShadows"></param>
        private void ApplyStackWithBaseMaterials(Material material, bool castShadows)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var baseMaterials = _baseMaterials[i];
                var newMaterials = new Material[baseMaterials.Length + 1];

                newMaterials[baseMaterials.Length] = material;
                for (int j = 0; j < baseMaterials.Length; j++)
                    newMaterials[j] = baseMaterials[j];

                _renderers[i].sharedMaterials = newMaterials;
                _renderers[i].shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
        }

        /// <summary>
        /// Applies the replace-base-materials effect mode.
        /// </summary>
        /// <param name="material">The material to apply.</param>
        /// <param name="castShadows"></param>
        private void ApplyReplaceBaseMaterials(Material material, bool castShadows)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var effectMaterials = new Material[_renderers[i].sharedMaterials.Length];

                for (int j = 0; j < effectMaterials.Length; j++)
                    effectMaterials[j] = material;

                _renderers[i].sharedMaterials = effectMaterials;
                _renderers[i].shadowCastingMode = castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off;
            }
        }

        /// <summary>
        /// Retrieves the base materials of the renderers.
        /// </summary>
        /// <returns>The base materials of the renderers.</returns>
        private Material[][] GetBaseMaterials()
        {
            var allMaterials = new Material[_renderers.Length][];

            for (int i = 0; i < allMaterials.Length; i++)
            {
                var sharedMaterials = _renderers[i].sharedMaterials;
                var materials = new Material[sharedMaterials.Length];

                for (int j = 0; j < sharedMaterials.Length; j++)
                    materials[j] = sharedMaterials[j];

                allMaterials[i] = materials;
            }

            return allMaterials;
        }

        #region Editor
        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void GetAllRenderers()
        {
#if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "GetAllRenderers");
            _renderers = GetRenderers();
#endif
        }
        
#if UNITY_EDITOR
        private void Reset() => _renderers = GetRenderers();

        private Renderer[] GetRenderers()
        {
            var skinnedRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var meshRenderers = TryGetComponent<LODGroup>(out var lodGroup)
                ? lodGroup.GetLODs()[0].renderers.Cast<MeshRenderer>().ToArray()
                : GetComponentsInChildren<MeshRenderer>(true);

            var renderers = new Renderer[meshRenderers.Length + skinnedRenderers.Length];

            meshRenderers.CopyTo(renderers, 0);
            skinnedRenderers.CopyTo(renderers, meshRenderers.Length);

            return renderers;
        }
#endif
        #endregion
    }
}
using System;
using UnityEngine;

namespace PolymindGames.SurfaceSystem
{
    /// <summary>
    /// Definition of a surface type for use in a physics-based character controller.
    /// </summary>
    [CreateAssetMenu(menuName = "Polymind Games/Surfaces/Surface Definition", fileName = "Surface_")]
    public sealed class SurfaceDefinition : DataDefinition<SurfaceDefinition>
    {
        /// <summary>
        /// A pair of audio and visual effects to play when an action occurs on the surface.
        /// </summary>
        [Serializable]
        public sealed class EffectPair
        {
            [Tooltip("Audio Effect")]
            public AdvancedAudioData AudioData;

            [SpaceArea]
            [Tooltip("Visual Effect")]
            public SurfaceEffect VisualEffect;

            [Tooltip("Decal Effect")]
            public SurfaceEffect DecalEffect;
        }
        
        [ReorderableList(ListStyle.Boxed, HasLabels = false)]
        [Tooltip("The physical materials assigned to this surface.")]
        public PhysicsMaterial[] Materials = Array.Empty<PhysicsMaterial>();

        [Range(0.01f, 2f), BeginGroup("Settings")]
        [Tooltip("Multiplier applied to character velocity when stepping on this surface.")]
        public float VelocityModifier = 1f;

        [Range(0.01f, 1f)]
        [Tooltip("Determines how rough the surface is, ranging from slippery to rough.")]
        public float SurfaceFriction = 1f;

        [Range(0f, 1f), EndGroup]
        [Tooltip("Determines how penetrable this surface is, ranging from easily penetrable to not penetrable.")]
        public float PenetrationResistance = 0.3f;

        [Title("Effects"), BeginGroup, EndGroup]
        [Tooltip("The impact effect for the surface. Used by collisions.")]
        public EffectPair ImpactEffect;

        [BeginGroup, EndGroup]
        [Tooltip("The soft footstep effect for the surface. Used by characters when moving slow, such as walking.")]
        public EffectPair SoftFootstepEffect;

        [BeginGroup, EndGroup]
        [Tooltip("The hard footstep effect for the surface. Used by characters when moving fast, such as running.")]
        public EffectPair HardFootstepEffect;

        [BeginGroup, EndGroup]
        [Tooltip("The fall impact effect for the surface. Used by characters.")]
        public EffectPair FallImpactEffect;

        [BeginGroup, EndGroup]
        [Tooltip("The bullet hit effect for the surface. Used by bullets.")]
        public EffectPair BulletEffect;

        [BeginGroup, EndGroup]
        [Tooltip("The hit effect for the surface. Used by bludgeoning weapons, such as bats and maces.")]
        public EffectPair HitEffect;

        [BeginGroup, EndGroup]
        [Tooltip("The cut effect for the surface. Used by slashing weapons, such as knives and axes.")]
        public EffectPair CutEffect;

        [BeginGroup, EndGroup]
        [Tooltip("The stab effect for the surface. Used by thrusting weapons, such as spears and rapiers.")]
        public EffectPair StabEffect;

        
        /// <summary>
        /// The name of the surface definition, derived from the scriptable object name.
        /// </summary>
        public override string Name
        {
            get => this != null ? name.Replace("Surface_", "") : string.Empty;
            protected set { }
        }

        public EffectPair GetEffectPairOfType(SurfaceEffectType effectType)
        {
            return effectType switch
            {
                SurfaceEffectType.Impact => ImpactEffect,
                SurfaceEffectType.SoftFootstep => SoftFootstepEffect,
                SurfaceEffectType.HardFootstep => HardFootstepEffect,
                SurfaceEffectType.FallImpact => FallImpactEffect,
                SurfaceEffectType.Bullet => BulletEffect,
                SurfaceEffectType.Hit => HitEffect,
                SurfaceEffectType.Cut => CutEffect,
                SurfaceEffectType.Stab => StabEffect,
                _ => throw new ArgumentOutOfRangeException(nameof(effectType), effectType, null)
            };
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            CollectionExtensions.DistinctPreserveNull(ref Materials);
        }
#endif
    }
}
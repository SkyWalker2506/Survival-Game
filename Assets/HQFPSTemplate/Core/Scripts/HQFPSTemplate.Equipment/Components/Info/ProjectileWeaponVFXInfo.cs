//-=-=-=-=-=-=- Copyright (c) Polymind Games, All rights reserved. -=-=-=-=-=-=-//
using System;
using UnityEngine;

namespace HQFPSTemplate.Equipment
{
    [CreateAssetMenu(fileName = "Weapon VFX Info", menuName = "HQ FPS Template/Equipment Component/Weapon VFX")]
    public class ProjectileWeaponVFXInfo : ScriptableObject
    {
		#region Internal
		#pragma warning disable 0649
		[Serializable]
		public class ParticleEffectsInfo : ICloneable
		{
			[BHeader("Particles")]

			public GameObject MuzzleFlashPrefab;

			[Space]

			public Vector3 MuzzleFlashOffset;
			public Vector2 MuzzleFlashRandomScale;
			public Vector3 MuzzleFlashRandomRot;

			[BHeader("Tracer")]

			public GameObject TracerPrefab;
			public Vector3 TracerOffset;

			public object Clone() => MemberwiseClone();
		}

		[Serializable]
		public class CasingEjectionInfo : ICloneable
		{
			public GameObject CasingPrefab;

			[Space]

			public float SpawnDelay;
			public float CasingScale = 1f;
			public float Spin;

			public Vector3 SpawnOffset;
			public Vector3 AimSpawnOffset;
			public Vector3 SpawnVelocity;

			public object Clone() => MemberwiseClone();
		}

		[Serializable]
		public class MagazineEjectionInfo : ICloneable
		{
			public GameObject MagazinePrefab;

			[Space]

			public float SpawnDelay;

			public Vector3 MagazineVelocity;
			public Vector3 MagazineAngularVelocity;

			public object Clone() => MemberwiseClone();
		}
		#endregion

		[Group] public ParticleEffectsInfo ParticleEffects = new ParticleEffectsInfo();
		[Group] public CasingEjectionInfo CasingEjection = null;
		[Group] public MagazineEjectionInfo MagazineEjection = null;
	}
}

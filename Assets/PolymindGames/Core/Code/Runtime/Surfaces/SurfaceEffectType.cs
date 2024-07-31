namespace PolymindGames.SurfaceSystem
{
	public enum SurfaceEffectType
	{
		Impact = 0,
		SoftFootstep = 1,
		HardFootstep = 2,
		FallImpact = 3,
		Bullet = 4,
		Hit = 5,
		Cut = 6,
		Stab = 7
	}

	public static class DamageTypeExtensions
	{
		public static SurfaceEffectType GetSurfaceEffectType(this DamageType damageType)
		{
			return damageType switch
			{
				DamageType.Generic => SurfaceEffectType.Impact,
				DamageType.Cut => SurfaceEffectType.Cut,
				DamageType.Hit => SurfaceEffectType.Hit,
				DamageType.Stab => SurfaceEffectType.Stab,
				DamageType.Bullet => SurfaceEffectType.Bullet,
				_ => SurfaceEffectType.Impact
			};
		}
	}
}
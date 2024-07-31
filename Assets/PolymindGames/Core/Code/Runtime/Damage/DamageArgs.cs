using UnityEngine;

namespace PolymindGames
{
    public readonly struct DamageArgs
    {
        public readonly IDamageSource Source;
        public readonly Vector3 HitPoint;
        public readonly Vector3 HitForce;
        public readonly DamageType DamageType;
        
        public static readonly DamageArgs Default = new();
        

        public DamageArgs(IDamageSource source)
        {
            DamageType = DamageType.Generic;
            HitPoint = HitForce = Vector3.zero;
            Source = source;
        }

        public DamageArgs(DamageType damageType)
        {
            DamageType = damageType;
            HitPoint = HitForce = Vector3.zero;
            Source = null;
        }

        public DamageArgs(DamageType damageType, IDamageSource source)
        {
            DamageType = damageType;
            HitPoint = HitForce = Vector3.zero;
            Source = source;
        }

        public DamageArgs(Vector3 hitPoint, Vector3 hitForce, IDamageSource source = null)
        {
            DamageType = DamageType.Generic;
            HitPoint = hitPoint;
            HitForce = hitForce;
            Source = source;
        }

        public DamageArgs(DamageType damageType, Vector3 hitPoint, Vector3 hitForce, IDamageSource source = null)
        {
            DamageType = damageType;
            HitPoint = hitPoint;
            HitForce = hitForce;
            Source = source;
        }
    }

    public enum DamageType : byte
    {
        Generic = 0,
        Cut = 1,
        Hit = 2,
        Stab = 3,
        Bullet = 4,
        Explosion = 5,
        Fire = 6,
        Gravity = 7
    }
}
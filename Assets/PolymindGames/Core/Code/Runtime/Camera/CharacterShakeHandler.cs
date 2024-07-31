using PolymindGames.ProceduralMotion;
using UnityEngine;
using System;

namespace PolymindGames
{
    public sealed class CharacterShakeHandler : CharacterBehaviour, IShakeHandler
    {
        [SerializeField, NotNull, BeginGroup("Motions")]
        private AdditiveShakeMotion _headShakeMotion;
        
        [SerializeField, NotNull, EndGroup]
        private AdditiveShakeMotion _handsShakeMotion;

        [SerializeField, Range(0f, 1000f), BeginGroup("Damage")]
        private float _minDamageThreshold = 10f;

        [SerializeField, SpaceArea(3f), EndGroup]
        [ReorderableList(ListStyle.Lined), LabelFromChild(nameof(DamageShake.Type))]
        private DamageShake[] _damageShakes;


        public void AddShake(ShakeMotionData shake, float multiplier = 1f, BodyPoint point = BodyPoint.Head)
        {
            if (point == BodyPoint.Head)
            {
                _headShakeMotion.AddPositionShake(shake.PositionShake, multiplier);
                _headShakeMotion.AddRotationShake(shake.RotationShake, multiplier);
            }
            else if (point == BodyPoint.Hands)
            {
                _handsShakeMotion.AddPositionShake(shake.PositionShake, multiplier);
                _handsShakeMotion.AddRotationShake(shake.RotationShake, multiplier);
            }
        }

        public void AddShake(ShakeData shake, BodyPoint point = BodyPoint.Head)
        {
            if (point == BodyPoint.Head)
                _headShakeMotion.AddShake(shake);
            else if (point == BodyPoint.Hands)
                _handsShakeMotion.AddShake(shake);
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            ShakeEvents.AddReceiver(this);
            character.HealthManager.DamageReceived += DamageReceived;
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            ShakeEvents.RemoveReceiver(this);
            character.HealthManager.DamageReceived -= DamageReceived;
        }

        private void DamageReceived(float damage, in DamageArgs args)
        {
            if (damage > _minDamageThreshold)
            {
                var shake = GetShakeForType(args.DamageType);
                float multiplier = (Mathf.Min(damage, shake.MaxDamage) - _minDamageThreshold) / (shake.MaxDamage - _minDamageThreshold);
                AddShake(shake.Data.Shake, shake.Data.Multiplier * multiplier);
            }
        }

        private DamageShake GetShakeForType(DamageType damageType)
        {
            for (int i = 0; i < _damageShakes.Length; i++)
            {
                if (_damageShakes[i].Type == damageType)
                    return _damageShakes[i];
            }

            return _damageShakes[0];
        }

        #region Internal
        [Serializable]
        private class DamageShake
        {
            [Range(0f, 100f)]
            public float MaxDamage = 50f;
            public DamageType Type;
            public ShakeData Data;
        }
        #endregion
    }
}
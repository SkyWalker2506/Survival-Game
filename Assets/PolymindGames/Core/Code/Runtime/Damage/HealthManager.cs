﻿using UnityEngine.Events;
using UnityEngine;
using System;

namespace PolymindGames
{
    /// <summary>
    /// Manages the parent character's health and death
    /// </summary>
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/player/modules-and-behaviours/health#health-manager-module")]
    [AddComponentMenu("Polymind Games/Damage/Health Manager")]
    public sealed class HealthManager : MonoBehaviour, IHealthManager, ISaveableComponent
    {
        [SerializeField, BeginGroup]
        [Tooltip("The starting health of this character (can't be higher than the max health).")]
#if UNITY_EDITOR
        [DynamicRange(nameof(Editor_GetMinHealth), nameof(Editor_GetMaxHealth))]
#endif
        private float _health = 100f;

        [SerializeField, Range(1, 10000f), EndGroup]
        [Tooltip("The starting max health of this character (can be modified at runtime).")]
#if UNITY_EDITOR
        [OnValueChanged(nameof(Editor_OnMaxHealthChanged))]
#endif
        private float _maxHealth = 100f;
        
        private const float THRESHOLD = 0.001f;
        
        
        public float Health
        {
            get => _health;
            private set
            {
                PrevHealth = _health;
                _health = value;
            }
        }

        public float PrevHealth { get; private set; }

        public float MaxHealth
        {
            get => _maxHealth;
            set
            {
                float clampedValue = Mathf.Max(value, 0f);

                if (Math.Abs(clampedValue - _maxHealth) > THRESHOLD)
                {
                    _maxHealth = clampedValue;
                    Health = Mathf.Clamp(Health, 0f, _maxHealth);
                }
            }
        }
        
        private bool IsAlive => _health >= HealthExtensions.THRESHOLD;

        public event DamageReceivedDelegate DamageReceived;
        public event HealthRestoredDelegate HealthRestored;
        public event DeathDelegate Death;
        public event UnityAction Respawn;
        
        public float RestoreHealth(float value)
        {
            bool wasAlive = IsAlive;
            value = Mathf.Abs(value);
            if (TryChangeHealth(ref value))
            {
                HealthRestored?.Invoke(value);

                if (!wasAlive && IsAlive)
                    Respawn?.Invoke();

                return value;
            }

            return 0f;
        }

        public float ReceiveDamage(float damage)
        {
            damage = -Mathf.Abs(damage);
            if (IsAlive && TryChangeHealth(ref damage))
            {
                DamageReceived?.Invoke(damage, in DamageArgs.Default);

                if (!IsAlive)
                    Death?.Invoke(in DamageArgs.Default);

                return damage * -1;
            }

            return 0f;
        }

        public float ReceiveDamage(float damage, in DamageArgs args)
        {
            damage = -Mathf.Abs(damage);
            if (IsAlive && TryChangeHealth(ref damage))
            {
                DamageReceived?.Invoke(damage, in args);

                if (!IsAlive)
                    Death?.Invoke(args);

                return damage * -1;
            }

            return 0f;
        }

        private bool TryChangeHealth(ref float delta)
        {
            if (Mathf.Abs(delta) < THRESHOLD)
                return false;

            Health = Mathf.Clamp(_health + delta, 0f, _maxHealth);
            delta = Mathf.Abs(PrevHealth - _health);
            return true;
        }

        #region Save & Load
        [Serializable]
        private sealed class SaveData
        {
            public float Health;
            public float MaxHealth;
        }

        void ISaveableComponent.LoadMembers(object data)
        {
            var saveData = (SaveData)data;
            _health = saveData.Health;
            _maxHealth = saveData.MaxHealth;
        }

        object ISaveableComponent.SaveMembers() => new SaveData
        {
            Health = _health,
            MaxHealth = _maxHealth
        };
        #endregion

        #region Editor
#if UNITY_EDITOR
#pragma warning disable CS0628
        protected void Editor_OnMaxHealthChanged() => Health = _health;
        protected float Editor_GetMinHealth() => 0f;
        protected float Editor_GetMaxHealth() => _maxHealth;
#pragma warning restore CS0628
#endif
        #endregion
    }
}
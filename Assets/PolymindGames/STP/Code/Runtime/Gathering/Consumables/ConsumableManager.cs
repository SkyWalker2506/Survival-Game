using System;
using System.Collections.Generic;
using PolymindGames.WorldManagement;
using UnityEngine;

namespace PolymindGames
{
    public sealed class ConsumableManager : MonoBehaviour, ISaveableComponent
    {
        [SerializeField, BeginGroup]
        private string _consumableName = "Blueberry";
        
        [SerializeField, MinMaxSlider(0f, 1000f), EndGroup]
        private Vector2Int _respawnHours = new(16, 24);

        [BeginGroup("Stat Changes")]
        [SerializeField, MinMaxSlider(-100, 100)]
        private Vector2Int _hungerRestore = new(15, 20);

        [SerializeField, MinMaxSlider(-100, 100), EndGroup]
        private Vector2Int _thirstRestore = new(5, 10);

        [SerializeField, BeginGroup("Audio"), EndGroup]
        [Tooltip("Audio that will be played after a character consumes this.")]
        private AudioDataSO _consumeAudio;

        private IHoverableInteractable[] _consumables;
        private List<RespawnData> _respawnData;


        private void Consume(IInteractable consumable, ICharacter character)
        {
            if (TryConsume(character))
            {
                consumable.gameObject.SetActive(false);
                int respawnHour = World.Instance.Time.TotalHours + _respawnHours.GetRandomFromRange();

                _respawnData ??= new List<RespawnData>();
                _respawnData.Add(new RespawnData(consumable, respawnHour));

                if (_respawnData.Count == 1)
                    SubscribeToTimeChanges();
            }
        }

        private bool TryConsume(ICharacter character)
        {
            bool consumed = false;

            if (character.TryGetCC(out IHungerManagerCC hungerManager) && hungerManager.MaxHunger - hungerManager.Hunger > 1f)
            {
                hungerManager.Hunger += _hungerRestore.GetRandomFromRange();
                consumed = true;
            }

            if (character.TryGetCC(out IThirstManagerCC thirstManager) && thirstManager.MaxThirst - thirstManager.Thirst > 1f)
            {
                thirstManager.Thirst += _thirstRestore.GetRandomFromRange();
                consumed = true;
            }

            if (consumed && _consumeAudio != null)
                AudioManager.Instance.PlayClip2D(_consumeAudio.Clip, _consumeAudio.Volume, _consumeAudio.Pitch);

            return consumed;
        }
        
        private void SubscribeToTimeChanges() => World.Instance.Time.HourChanged += OnHourChanged;
        private void UnsubscribeFromTimeChanges() => World.Instance.Time.HourChanged -= OnHourChanged;

        private void OnHourChanged(int totalHours, int passedHours)
        {
            if (passedHours < 0)
                return;
            
            for (int i = _respawnData.Count - 1; i >= 0; i--)
            {
                var respawnData = _respawnData[i];
                if (totalHours >= respawnData.RespawnHour)
                {
                    respawnData.Consumable.gameObject.SetActive(true);
                    _respawnData.RemoveAt(i);

                    if (_respawnData.Count == 0)
                        UnsubscribeFromTimeChanges();
                }
            }
        }

        private void Awake() => InitializeConsumables();

        private void OnDestroy()
        {
            if (_respawnData != null && _respawnData.Count > 0)
                UnsubscribeFromTimeChanges();
        }

        private void InitializeConsumables()
        {
            _consumables = GetComponentsInChildren<IHoverableInteractable>();
            
            string description = $"Hunger: +{_hungerRestore.x}-{_hungerRestore.y}\nThirst: +{_thirstRestore.x}-{_thirstRestore.y}";
            foreach (var consumable in _consumables)
            {
                consumable.Interacted += Consume;
                consumable.Title = _consumableName;
                consumable.Description = description;
            }
        }

        #region Internal
        private readonly struct RespawnData
        {
            public readonly IInteractable Consumable;
            public readonly int RespawnHour;

            public RespawnData(IInteractable consumable, int respawnHour)
            {
                Consumable = consumable;
                RespawnHour = respawnHour;
            }
        }
        #endregion

        #region Save & Load
        [Serializable]
        private sealed class RespawnSaveData
        {
            public int ConsumableIndex;
            public int RemainingHours;
        }

        void ISaveableComponent.LoadMembers(object data)
        {
            if (data == null)
                return;

            var saveData = (RespawnSaveData[])data;

            if (saveData.Length > 0)
            {
                _respawnData = new List<RespawnData>(saveData.Length);
                SubscribeToTimeChanges();
            }

            for (int i = 0; i < saveData.Length; i++)
            {
                var consumable = _consumables[saveData[i].ConsumableIndex];
                consumable.gameObject.SetActive(false);
                int targetHour = World.Instance.Time.TotalHours + saveData[i].RemainingHours;
                _respawnData.Add(new RespawnData(consumable, targetHour));
            }
            
            enabled = saveData.Length > 0;
        }

        object ISaveableComponent.SaveMembers()
        {
            if (_respawnData == null || _respawnData.Count == 0)
                return null;

            var respawnData = new RespawnSaveData[_respawnData.Count];
            for (int i = 0; i < _respawnData.Count; i++)
            {
                respawnData[i] = new RespawnSaveData
                {
                    ConsumableIndex = Array.IndexOf(_consumables, _respawnData[i].Consumable),
                    RemainingHours = _respawnData[i].RespawnHour - World.Instance.Time.TotalHours
                };
            }

            return respawnData;
        }
        #endregion
    }
}
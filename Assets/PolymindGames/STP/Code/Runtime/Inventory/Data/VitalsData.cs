using UnityEngine;

namespace PolymindGames.InventorySystem
{
    public sealed class VitalsData : ItemData
    {
        [SerializeField, MinMaxSlider(-100f, 100f)]
        private Vector2 _healthChange; 
        
        [SerializeField, MinMaxSlider(-100f, 100f)]
        private Vector2 _hungerChange; 

        [SerializeField, MinMaxSlider(-100f, 100f)]
        private Vector2 _thirstChange;


        public float HealthChange => _healthChange.GetRandomFromRange();
        public float HungerChange => _hungerChange.GetRandomFromRange();
        public float ThirstChange => _thirstChange.GetRandomFromRange();
    }
}
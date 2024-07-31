using UnityEngine;

namespace PolymindGames.InventorySystem
{
    public sealed class FuelData : ItemData
    {
        [SerializeField, MinMaxSlider(1, 1000)]
        private Vector2Int _fuel;


        public int FuelEfficiency => Mathf.Clamp(_fuel.GetRandomFromRange(), 1, 1000);
    }
}
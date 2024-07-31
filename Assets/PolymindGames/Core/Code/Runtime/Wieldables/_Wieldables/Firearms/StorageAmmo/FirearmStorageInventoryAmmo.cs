using PolymindGames.InventorySystem;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Firearms/Ammo/Inventory Storage Ammo")]
    public class FirearmStorageInventoryAmmo : FirearmStorageAmmoBehaviour
    {
        [SerializeField, DataReferenceDetails(HasNullElement = false), BeginGroup, EndGroup]
        private DataIdReference<ItemDefinition> _ammoItem;

        private IItemContainer _ammoContainer;


        public override event UnityAction<int> AmmoCountChanged;
        
        protected override void OnEnable()
        {
            var inventory = Wieldable.Character.Inventory;
            _ammoContainer = inventory.GetContainerWithoutTags();
            _ammoContainer.ContainerChanged += OnContainerChanged;
            base.OnEnable();
        }

        private void OnDisable() => _ammoContainer.ContainerChanged -= OnContainerChanged;
        private void OnContainerChanged() => AmmoCountChanged?.Invoke(GetAmmoCount());

        public override int RemoveAmmo(int amount) => _ammoContainer.RemoveItem(_ammoItem, amount);
        public override int AddAmmo(int amount) => _ammoContainer.AddItem(_ammoItem, amount);
        public override int GetAmmoCount() => _ammoContainer.GetItemCount(_ammoItem);
        public override bool HasAmmo() => _ammoContainer.ContainsItemWithId(_ammoItem);
    }
}
using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [RequireComponent(typeof(ItemContainerUI))]
    public sealed class PlayerItemContainerUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup]
        private string _containerName;

        [SerializeField, EndGroup]
        private AttachMode _attachMode = AttachMode.AttachOnStart;

        private ItemContainerUI _containerUI;
        private IItemContainer _container;


        public void AttachToCharacterContainer() => _containerUI.AttachToContainer(_container);
        public void DetachFromCharacterContainer() => _containerUI.DetachFromContainer();

        protected override void Awake()
        {
            base.Awake();
            _containerUI = GetComponent<ItemContainerUI>();
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            _container = character.Inventory.GetContainerWithName(_containerName);
            if (_attachMode != AttachMode.AttachManually)
                _containerUI.AttachToContainer(_container);
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            base.OnCharacterDetached(character);
            _containerUI.DetachFromContainer();
        }

		#region Internal
        private enum AttachMode
        {
            AttachOnStart,
            AttachManually
        }
		#endregion
    }
}
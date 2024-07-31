using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class CharacterPreviewUI : CharacterUIBehaviour
    {
        [SerializeField, NotNull, BeginGroup("References")]
        private Camera _camera;

        [SerializeField, NotNull]
        private BodyClothing _bodyClothing;
        
        [SerializeField, NotNull]
        private GameObject _characterVisuals;

        [SerializeField, EndGroup]
        private AudioDataSO _clothChangedAudio;

        [SerializeField, BeginGroup("Equipment Containers")]
        private DataIdReference<ItemTagDefinition> _headEquipmentTag;

        [SerializeField]
        private DataIdReference<ItemTagDefinition> _torsoEquipmentTag;

        [SerializeField]
        private DataIdReference<ItemTagDefinition> _legsEquipmentTag;

        [SerializeField, EndGroup]
        private DataIdReference<ItemTagDefinition> _feetEquipmentTag;
        
        private IItemContainer _feetContainer;
        private IItemContainer _headContainer;
        private IItemContainer _legsContainer;
        private IItemContainer _torsoContainer;


        protected override void Awake()
        {
            base.Awake();

            _camera.forceIntoRenderTexture = true;
            _characterVisuals.SetActive(false);
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            var inventory = character.Inventory;

            _headContainer = inventory.GetContainerWithTag(_headEquipmentTag);
            _torsoContainer = inventory.GetContainerWithTag(_torsoEquipmentTag);
            _legsContainer = inventory.GetContainerWithTag(_legsEquipmentTag);
            _feetContainer = inventory.GetContainerWithTag(_feetEquipmentTag);

            _headContainer.SlotChanged += OnSlotChanged;
            _torsoContainer.SlotChanged += OnSlotChanged;
            _legsContainer.SlotChanged += OnSlotChanged;
            _feetContainer.SlotChanged += OnSlotChanged;

            OnClothingChanged(BodyPoint.Head, _headContainer.Slots[0]);
            OnClothingChanged(BodyPoint.Torso, _torsoContainer.Slots[0]);
            OnClothingChanged(BodyPoint.Legs, _legsContainer.Slots[0]);
            OnClothingChanged(BodyPoint.Feet, _feetContainer.Slots[0]);

            if (character.TryGetCC(out IInventoryInspectManagerCC inspection))
            {
                inspection.InspectionStarted += OnInspectionStarted;
                inspection.InspectionEnded += OnInspectionEnded;
            }
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            _headContainer.SlotChanged -= OnSlotChanged;
            _torsoContainer.SlotChanged -= OnSlotChanged;
            _legsContainer.SlotChanged -= OnSlotChanged;
            _feetContainer.SlotChanged -= OnSlotChanged;

            if (character.TryGetCC(out IInventoryInspectManagerCC inspection))
            {
                inspection.InspectionStarted -= OnInspectionStarted;
                inspection.InspectionEnded -= OnInspectionEnded;
            }
        }

        private void OnSlotChanged(ItemSlot.CallbackArgs args)
        {
            var container = args.Slot.Container;
            BodyPoint bodyPoint = GetBodyPointFromContainer(container);
            OnClothingChanged(bodyPoint, args.Slot);
        }

        private void OnClothingChanged(BodyPoint bodyPoint, ItemSlot slot)
        {
            var item = slot.Item?.Definition;
            _bodyClothing.SetClothing(bodyPoint, item);
            
            if (Time.timeSinceLevelLoad > 1f)
                Character.AudioPlayer.PlaySafe(_clothChangedAudio, bodyPoint);
        }

        private BodyPoint GetBodyPointFromContainer(IItemContainer container)
        {
            if (container == _headContainer) 
                return BodyPoint.Head;
            
            if (container == _torsoContainer)
                return BodyPoint.Torso;
            
            if (container == _legsContainer)
                return BodyPoint.Legs;

            return BodyPoint.Feet;
        }

        private void OnInspectionStarted() => _characterVisuals.SetActive(true);
        private void OnInspectionEnded() => _characterVisuals.SetActive(false);
    }
}
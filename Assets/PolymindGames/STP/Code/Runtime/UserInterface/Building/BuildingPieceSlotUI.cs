using PolymindGames.BuildingSystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [AddComponentMenu("Polymind Games/User Interface/Slots/Building Piece Slot")]
    public sealed class BuildingPieceSlotUI : SlotUI
    {
        [SerializeField, IgnoreParent, BeginGroup]
        private DataDefinitionInfo<BuildingPieceDefinition> _nameInfo;

        [SerializeField, IgnoreParent, EndGroup]
        private BuildingPieceRequirementInfo _requirementInfo;

        private BuildingPieceDefinition _definition;
        
        
        public BuildingPieceDefinition Definition => _definition;

        public void SetBuildingPiece(BuildingPieceDefinition definition)
        {
            _nameInfo.UpdateInfo(definition);
            _requirementInfo.UpdateInfo(definition);

            _definition = definition;
        }
    }
}
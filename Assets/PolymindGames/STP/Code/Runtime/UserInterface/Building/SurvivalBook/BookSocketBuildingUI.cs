using PolymindGames.BuildingSystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class BookSocketBuildingUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        private DataIdReference<BuildingPieceDefinition> _startingPiece;


        public void StartSocketBasedBuilding()
        {
            Character.GetCC<IWieldableControllerCC>().TryEquipWieldable(null);
            var buildingController = Character.GetCC<IBuildControllerCC>();
            var instance = Instantiate(_startingPiece.Def.Prefab);
            buildingController.SetBuildingPiece(instance);
        }
    }
}

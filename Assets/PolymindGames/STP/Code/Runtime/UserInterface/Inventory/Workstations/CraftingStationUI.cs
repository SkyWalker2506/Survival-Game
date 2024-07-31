using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class CraftingStationUI : WorkstationInspectorBaseUI<CraftStation>
    {
        [SerializeField, BeginGroup("Crafting"), EndGroup]
        private CraftingUI _craftingUI;


        protected override void OnInspectionStarted(CraftStation workstation)
        {
            _craftingUI.SetCustomLevel(workstation.CraftableLevel, workstation.Name);
        }

        protected override void OnInspectionEnded(CraftStation workstation)
        {
            _craftingUI.DisableCustomLevel();
        }
    }
}
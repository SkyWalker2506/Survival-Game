using UnityEngine;

namespace PolymindGames.UserInterface
{
    public interface IWheelUI
    {
        void StartInspection();
        void EndInspectionAndSelectHighlighted();
        void EndInspection();
        void UpdateSelection(Vector2 input);
    }
}
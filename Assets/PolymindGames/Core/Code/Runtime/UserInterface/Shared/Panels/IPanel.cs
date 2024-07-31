using UnityEngine.Events;

namespace PolymindGames.UserInterface
{
    public interface IPanel : IMonoBehaviour
    {
        bool IsVisible { get; }
        int PanelLayer { get; }
        bool CanEscape { get; }
        
        event UnityAction<bool> PanelToggled;
        
        void Show();
        void Hide();
        void ChangeVisibility(bool show);
    }
}
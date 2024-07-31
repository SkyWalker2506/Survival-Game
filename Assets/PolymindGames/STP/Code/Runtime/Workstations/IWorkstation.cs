using PolymindGames.InventorySystem;

namespace PolymindGames
{
    public interface IWorkstation : IMonoBehaviour
    {
        string Name { get; }
        IItemContainer[] GetContainers();
        
        void BeginInspection();
        void EndInspection();
    }
}
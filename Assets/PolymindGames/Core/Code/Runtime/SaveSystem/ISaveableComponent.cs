namespace PolymindGames
{
    public interface ISaveableComponent : IMonoBehaviour
    {
        void LoadMembers(object data);
        object SaveMembers();
    }
}
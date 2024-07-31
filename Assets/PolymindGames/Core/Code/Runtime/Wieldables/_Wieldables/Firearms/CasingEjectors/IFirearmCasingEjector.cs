namespace PolymindGames.WieldableSystem
{
    public interface IFirearmCasingEjector
    {
        float EjectDuration { get; }
        bool IsEjecting { get; }

        void Eject();

        void Attach();
        void Detach();
    }

    public sealed class DefaultFirearmCasingEjector : IFirearmCasingEjector
    {
        public static readonly DefaultFirearmCasingEjector Instance = new();

        private DefaultFirearmCasingEjector() { }
        
        public float EjectDuration => 0f;
        public bool IsEjecting { get; }

        public void Eject() { }
        public void Attach() { }
        public void Detach() { }
    }
}
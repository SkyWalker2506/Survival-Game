namespace PolymindGames
{
    public interface IDamageReceiver : IMonoBehaviour
    {
        ICharacter Character { get; }

        DamageResult ReceiveDamage(float damage);
        DamageResult ReceiveDamage(float damage, in DamageArgs args);
    }

    public enum DamageResult : byte
    {
        Normal = 0,
        Critical = 1,
        Fatal = 2,
        Ignored = 3
    }
}
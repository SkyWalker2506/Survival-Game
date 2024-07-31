namespace PolymindGames.WieldableSystem
{
    public interface IMeleeAttackHandler
    {
        bool QuickAttack(float accuracy = 1f, bool altAttack = false);
        bool CanAttack();
    }
}
using Unity.Entities;
namespace ProjectDawn.Enemy.ComponentData
{
    public struct AttackToPlayerState : IComponentData, IEnableableComponent
    {
        public float StopToAttackDistance; 
        public float AttackInterval;
        public float LastAttackTime;

    }
    
}
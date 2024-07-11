using Unity.Entities;
namespace ProjectDawn.Enemy.ComponentData
{
    public struct FollowPlayerState : IComponentData, IEnableableComponent
    {
        public float StartToAttackDistance; 
        public float StopToFollowDistance; 
    }
    
}
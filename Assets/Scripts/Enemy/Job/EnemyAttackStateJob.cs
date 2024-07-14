using ProjectDawn.Enemy.ComponentData;
using ProjectDawn.Navigation;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectDawn.Enemy.System
{
    public partial struct EnemyAttackStateJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float3 PlayerPosition;

        public void Execute(Entity entity, AttackToPlayerState attackToPlayerState, AgentBody agentBody, LocalTransform localTransform,[ChunkIndexInQuery]int chunkIndex)
        {
            var distance = math.distance(localTransform.Position, PlayerPosition);

            if (distance > attackToPlayerState.StopToAttackDistance)
            {
                ECB.SetComponentEnabled<AttackToPlayerState>(chunkIndex,entity, false);
                ECB.SetComponentEnabled<SearchPlayerState>(chunkIndex,entity, true);
            }

        }
    }



}

using ProjectDawn.Enemy.ComponentData;
using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectDawn.Enemy.System
{
    [BurstCompile]
    public partial struct EnemyFollowTargetStateJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float3 PlayerPosition;

        public void Execute(Entity entity, FollowPlayerState followPlayerState, AgentBody agentBody, LocalTransform localTransform,[ChunkIndexInQuery]int chunkIndex)
        {
            if (math.distance(localTransform.Position, PlayerPosition) > followPlayerState.MaxDistanceToFollow)
            {
                agentBody.SetDestination(PlayerPosition);
            }
            else
            {
                agentBody.Stop();
                ECB.SetComponentEnabled<FollowPlayerState>(chunkIndex,entity, false);
                ECB.SetComponentEnabled<AttackToPlayerState>(chunkIndex,entity, true);
            }
        }


   
    }
}

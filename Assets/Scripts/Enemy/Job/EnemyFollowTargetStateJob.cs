using ProjectDawn.Enemy.ComponentData;
using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Entities;
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
            var distance = math.distance(localTransform.Position, PlayerPosition);
            if (distance > followPlayerState.StopToFollowDistance)
            {
                agentBody.Stop();
                ECB.SetComponentEnabled<FollowPlayerState>(chunkIndex,entity, false);
                ECB.SetComponentEnabled<SearchPlayerState>(chunkIndex,entity, true);
            }
            else if (distance < followPlayerState.StartToAttackDistance)  
            {
                agentBody.Stop();
                ECB.SetComponentEnabled<FollowPlayerState>(chunkIndex,entity, false);
                ECB.SetComponentEnabled<AttackToPlayerState>(chunkIndex,entity, true);
            }
            agentBody.Destination = PlayerPosition;
        }
   
    }


}

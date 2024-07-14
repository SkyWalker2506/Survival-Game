using ProjectDawn.Enemy.ComponentData;
using ProjectDawn.Navigation;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace ProjectDawn.Enemy.System
{
    public partial struct SearchPlayerStateJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ECB;
        public float3 PlayerPosition;
        public Random Random;

        public void Execute(Entity entity, SearchPlayerState searchPlayerState, AgentBody agentBody, LocalTransform localTransform,[ChunkIndexInQuery]int chunkIndex)
        {
            var distance = math.distance(localTransform.Position, PlayerPosition);
            if (distance < searchPlayerState.StartToFollowDistance)
            {
                ECB.SetComponentEnabled<SearchPlayerState>(chunkIndex,entity, false);
                ECB.SetComponentEnabled<FollowPlayerState>(chunkIndex,entity, true);
            }
            else if(searchPlayerState.IsTargetPositionSet)
            {
                if(math.distance(localTransform.Position, searchPlayerState.TargetPosition) < 1)
                {
                    searchPlayerState.IsTargetPositionSet = false;
                }
            }
            else
            {
                searchPlayerState.TargetPosition = Random.NextFloat3();
                agentBody.Destination = searchPlayerState.TargetPosition;
                searchPlayerState.IsTargetPositionSet = true;
            }
        }
    }



}

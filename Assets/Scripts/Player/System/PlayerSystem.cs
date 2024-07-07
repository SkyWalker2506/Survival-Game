using ProjectDawn.Player.ComponentData;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;


namespace ProjectDawn.Player.System
{
    [BurstCompile]
    public partial struct PlayerSystem : ISystem
    {
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (localTransform, playerData) in SystemAPI.Query<RefRO<LocalTransform>, RefRW<PlayerData>>())
            {
                playerData.ValueRW.Position = localTransform.ValueRO.Position;
            }
        }
    }
}


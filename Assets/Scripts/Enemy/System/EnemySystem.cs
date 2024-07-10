using ECM2.Examples.FirstPerson;
using ProjectDawn.Enemy.ComponentData;
using ProjectDawn.Navigation;
using ProjectDawn.Player.System;
using Unity.Burst;
using Unity.Entities;

namespace ProjectDawn.Enemy.System
{
    [BurstCompile]
    public partial class EnemySystem : SystemBase
    {

        [BurstCompile]
        protected override void OnUpdate()
        {
            var playerSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerSystem>();
            if(playerSystem.IsPositionSet==false)
            {
                return;
            }
            var playerPosition = playerSystem.PlayerPosition;
            foreach (var (agentBody, enemy) in SystemAPI.Query<RefRW<AgentBody>, RefRO<EnemyTag>>())
            {
                agentBody.ValueRW.SetDestination(playerPosition);
            }          
        }
    }
}

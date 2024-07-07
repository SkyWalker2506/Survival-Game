using ECM2.Examples.FirstPerson;
using ProjectDawn.Enemy.ComponentData;
using ProjectDawn.Navigation;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace ProjectDawn.Enemy.System
{
    [BurstCompile]
    public partial class EnemySystem : SystemBase
    {
        float3 playerPosition;
        Transform playerTransform;

        [BurstCompile]
        protected override void OnUpdate()
        {
            if(playerTransform == null)
                playerTransform = Object.FindAnyObjectByType<FirstPersonCharacter>().transform;
            if(playerTransform == null)
                return;
            playerPosition = playerTransform.position;

            foreach (var (agentBody, enemy) in SystemAPI.Query<RefRW<AgentBody>, RefRO<EnemyTag>>())
            {
                agentBody.ValueRW.SetDestination(playerPosition);
            }          
        }
    }
}

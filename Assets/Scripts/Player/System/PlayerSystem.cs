using ECM2.Examples.FirstPerson;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace ProjectDawn.Player.System
{
    [BurstCompile]
    public partial class PlayerSystem : SystemBase
    {
        public bool IsPositionSet;
        public float3 PlayerPosition;
        Transform playerTransform;

        [BurstCompile]
        protected override void OnUpdate()
        {
            if(playerTransform == null)
            playerTransform = Object.FindAnyObjectByType<FirstPersonCharacter>().transform;
            if(playerTransform == null)
                return;
            PlayerPosition = playerTransform.position;
            IsPositionSet = true;
        }
    }
}


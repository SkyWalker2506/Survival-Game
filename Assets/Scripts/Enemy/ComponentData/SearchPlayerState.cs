using Unity.Entities;
using Unity.Mathematics;

namespace ProjectDawn.Enemy.ComponentData
{
    public struct SearchPlayerState : IComponentData, IEnableableComponent
    {
        public float StartToFollowDistance; 
        public float3 TargetPosition;
        public bool IsTargetPositionSet;
    }
    
}
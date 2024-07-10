using Unity.Entities;
namespace ProjectDawn.Enemy.ComponentData
{
    public struct SearchPlayerState : IComponentData, IEnableableComponent
    {
        public float MinDistanceToFindPlayer; 

    }
    
}
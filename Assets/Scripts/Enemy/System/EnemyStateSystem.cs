using ProjectDawn.Player.System;
using Unity.Entities;

namespace ProjectDawn.Enemy.System
{
    public partial struct EnemyStateSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var playerSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<PlayerSystem>();

        }
    }
}

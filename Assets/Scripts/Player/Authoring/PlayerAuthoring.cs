using ProjectDawn.Enemy.ComponentData;
using ProjectDawn.Player.ComponentData;
using Unity.Entities;
using UnityEngine;


namespace ProjectDawn.Player.Authoring
{
    public class PlayerAuthoring : MonoBehaviour
    {
        class PlayerBaker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerData());
            }
        }
    }
    
}
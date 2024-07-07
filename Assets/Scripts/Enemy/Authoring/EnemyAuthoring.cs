using ProjectDawn.Enemy.ComponentData;
using Unity.Entities;
using UnityEngine;

namespace ProjectDawn.Enemy.Authoring
{
    public class EnemyAuthoring : MonoBehaviour
    {
        class EnemyBaker : Baker<EnemyAuthoring>
        {
            public override void Bake(EnemyAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EnemyTag());
            }
        }
    }
    
}
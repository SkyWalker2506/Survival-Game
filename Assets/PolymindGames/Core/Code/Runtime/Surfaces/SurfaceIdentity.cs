using UnityEngine;

namespace PolymindGames.SurfaceSystem
{
    public abstract class SurfaceIdentity<T> : SurfaceIdentity where T : Collider
    {
        public sealed override SurfaceDefinition GetSurfaceFromHit(ref RaycastHit hit)
        {
            if (hit.collider is T col)
                return GetSurfaceFromHit(col, ref hit);
                
            return null;
        }

        public sealed override SurfaceDefinition GetSurfaceFromCollision(Collision collision)
        {
            if (collision.collider is T col)
                return GetSurfaceFromCollision(col, collision);
                
            return null;
        }

        protected abstract SurfaceDefinition GetSurfaceFromHit(T col, ref RaycastHit hit);
        protected abstract SurfaceDefinition GetSurfaceFromCollision(T col, Collision collision);

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!gameObject.HasComponent<T>())
                Debug.LogError($"No collider of type {typeof(T)} found on this game object.", gameObject);
        }
#endif
    }
    
    public abstract class SurfaceIdentity : MonoBehaviour
    {
        public abstract SurfaceDefinition GetSurfaceFromHit(ref RaycastHit hit);
        public abstract SurfaceDefinition GetSurfaceFromCollision(Collision collision);
    }
}
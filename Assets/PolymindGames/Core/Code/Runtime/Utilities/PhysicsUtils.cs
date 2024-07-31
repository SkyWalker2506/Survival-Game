using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace PolymindGames
{
    public static class PhysicsUtils
    {
        public static readonly LayerMask AllLayers = ~int.MinValue; 
        
        private static readonly Collider[] s_OverlappedColliders = new Collider[64];
        private static readonly RaycastHit[] s_RaycastHits = new RaycastHit[32];


        public static Ray GenerateRay(Transform transform, float randomSpread, Vector3 localOffset = default(Vector3))
        {
            Vector3 raySpreadVector = transform.TransformVector(new Vector3(Random.Range(-randomSpread, randomSpread), Random.Range(-randomSpread, randomSpread), 0f));
            Vector3 rayDirection = Quaternion.Euler(raySpreadVector) * transform.forward;

            return new Ray(transform.position + transform.TransformVector(localOffset), rayDirection);
        }

        public static float RaycastOptimizedClosestDistance(Ray ray, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers, Transform ignoredRoot = null, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int hitCount = Physics.RaycastNonAlloc(ray, s_RaycastHits, maxDistance, layerMask, triggerInteraction);
            if (hitCount > 0)
            {
                float closestDistance = Mathf.Infinity;
                bool hasIgnoredRoot = ignoredRoot != null;

                for (int i = 0; i < hitCount; i++)
                {
                    if (hasIgnoredRoot)
                    {
                        // Check if the transform is part of the ignored root.
                        if (s_RaycastHits[i].transform.IsChildOfTransform(ignoredRoot))
                            continue;
                    }

                    if (s_RaycastHits[i].distance < closestDistance)
                        closestDistance = s_RaycastHits[i].distance;
                }

                return closestDistance;
            }

            return Mathf.Infinity;
        }

        public static bool RaycastOptimized(Ray ray, float distance, out RaycastHit hitInfo,
            int layerMask = Physics.DefaultRaycastLayers, Transform ignoredRoot = null, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            var hitCount = Physics.RaycastNonAlloc(ray, s_RaycastHits, distance, layerMask, triggerInteraction);
            if (hitCount > 0)
            {
                int closestHit = -1;
                float closestDistance = Mathf.Infinity;
                bool hasIgnoredRoot = ignoredRoot != null;

                for (int i = 0; i < hitCount; i++)
                {
                    if (hasIgnoredRoot)
                    {
                        // Check if the transform is part of the ignored root.
                        if (s_RaycastHits[i].transform.IsChildOfTransform(ignoredRoot))
                            continue;
                    }

                    if (s_RaycastHits[i].distance < closestDistance)
                    {
                        closestDistance = s_RaycastHits[i].distance;
                        closestHit = i;
                    }
                }

                if (closestHit != -1)
                {
                    hitInfo = s_RaycastHits[closestHit];
                    return true;
                }
            }

            hitInfo = default(RaycastHit);
            return false;
        }

        public static bool SphereCastOptimized(Ray ray, float radius, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers, Transform ignoredRoot = null, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int hitCount = Physics.SphereCastNonAlloc(ray, radius, s_RaycastHits, maxDistance, layerMask, triggerInteraction);
            if (hitCount > 0)
            {
                if (ignoredRoot == null)
                    return true;

                for (int i = 0; i < hitCount; i++)
                {
                    // Check if the transform is part of the ignored root.
                    if (s_RaycastHits[i].transform.IsChildOfTransform(ignoredRoot))
                        continue;

                    return true;
                }
            }

            return false;
        }

        public static float SphereCastOptimizedClosestDistance(Ray ray, float radius, float maxDistance = Mathf.Infinity,
            int layerMask = Physics.DefaultRaycastLayers, Transform ignoredRoot = null, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int hitCount = Physics.SphereCastNonAlloc(ray, radius, s_RaycastHits, maxDistance, layerMask, triggerInteraction);
            if (hitCount > 0)
            {
                float closestDistance = Mathf.Infinity;
                bool hasIgnoredRoot = ignoredRoot != null;

                for (int i = 0; i < hitCount; i++)
                {
                    if (hasIgnoredRoot)
                    {
                        // Check if the transform is part of the ignored root.
                        if (s_RaycastHits[i].transform.IsChildOfTransform(ignoredRoot))
                            continue;
                    }

                    if (s_RaycastHits[i].distance < closestDistance)
                        closestDistance = s_RaycastHits[i].distance;
                }

                return closestDistance;
            }

            return Mathf.Infinity;
        }

        public static bool SphereCastOptimized(Ray ray, float radius, float distance, out RaycastHit hitInfo,
            int layerMask = Physics.DefaultRaycastLayers, Transform ignoredRoot = null, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            var hitCount = Physics.SphereCastNonAlloc(ray, radius, s_RaycastHits, distance, layerMask, triggerInteraction);
            if (hitCount > 0)
            {
                int closestHit = -1;
                float closestDistance = Mathf.Infinity;
                bool hasIgnoredRoot = ignoredRoot != null;

                for (int i = 0; i < hitCount; i++)
                {
                    if (hasIgnoredRoot)
                    {
                        // Check if the transform is part of the ignored root.
                        if (s_RaycastHits[i].transform.IsChildOfTransform(ignoredRoot))
                            continue;
                    }

                    if (s_RaycastHits[i].distance < closestDistance)
                    {
                        closestDistance = s_RaycastHits[i].distance;
                        closestHit = i;
                    }
                }

                if (closestHit != -1)
                {
                    hitInfo = s_RaycastHits[closestHit];
                    return true;
                }
            }

            hitInfo = default(RaycastHit);
            return false;
        }
        
        public static int OverlapBoxOptimized(in Bounds bounds, Quaternion orientation, out Collider[] colliders, int layerMask, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int size = Physics.OverlapBoxNonAlloc(bounds.center, bounds.extents, s_OverlappedColliders, orientation, layerMask, triggerInteraction);
            colliders = s_OverlappedColliders;
            return size;
        }

        public static int OverlapBoxOptimized(Vector3 center, Vector3 extents, Quaternion orientation, out Collider[] colliders,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int size = Physics.OverlapBoxNonAlloc(center, extents, s_OverlappedColliders, orientation, layerMask, triggerInteraction);
            colliders = s_OverlappedColliders;
            return size;
        }

        public static int OverlapSphereOptimized(Vector3 position, float radius, out Collider[] colliders,
            int layerMask = Physics.DefaultRaycastLayers, QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore)
        {
            int size = Physics.OverlapSphereNonAlloc(position, radius, s_OverlappedColliders, layerMask, triggerInteraction);
            colliders = s_OverlappedColliders;

            return size;
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    public static class ShakeEvents
    {
        private static readonly List<IShakeHandler> s_ShakeHandlers = new();


        public static void AddReceiver(IShakeHandler handler)
        {
#if DEBUG
            if (handler == null)
            {
                Debug.LogWarning("Shake Handler is null.");
                return;
            }

            if (s_ShakeHandlers.Contains(handler))
            {
                Debug.LogWarning($"Shake Handler is already added", handler.transform);
                return;
            }
#endif

            s_ShakeHandlers.Add(handler);
        }

        public static void RemoveReceiver(IShakeHandler handler)
        {
#if DEBUG
            if (handler == null)
            {
                Debug.LogWarning("Shake Handler is null.");
                return;
            }

            if (!s_ShakeHandlers.Remove(handler))
            {
                Debug.LogWarning($"Shake Handler is not added", handler.transform);
                return;
            }
#else
            s_ShakeHandlers.Remove(handler);
#endif
        }

        public static void DoShake(Vector3 position, ShakeData data, float radius)
        {
            if (!data.IsPlayable)
                return;

            radius = Math.Abs(radius);
            float multiplier = Math.Abs(data.Multiplier);
            foreach (var handler in s_ShakeHandlers)
            {
                float distToImpact = (handler.transform.position - position).magnitude;
                if (radius - distToImpact > 0f)
                {
                    float distanceFactor = 1f - Mathf.Clamp01(distToImpact / radius);
                    handler.AddShake(data.Shake, distanceFactor * multiplier);
                }
            }
        }
        
        public static void DoShake(Vector3 position, ShakeMotionData shake, float radius, float multiplier = 1f)
        {
#if DEBUG
            if (shake == null)
            {
                Debug.LogWarning("Shake is null.");
                return;
            }
#endif

            radius = Math.Abs(radius);
            multiplier = Math.Abs(multiplier);
            foreach (var handler in s_ShakeHandlers)
            {
                float distToImpact = (handler.transform.position - position).magnitude;
                if (radius - distToImpact > 0f)
                {
                    float distanceFactor = 1f - Mathf.Clamp01(distToImpact / radius);
                    handler.AddShake(shake, distanceFactor * multiplier);
                }
            }
        }

        public static void DoGlobalShake(ShakeMotionData shake, float multiplier = 1f)
        {
#if DEBUG
            if (shake == null)
            {
                Debug.LogWarning("Shake is null.");
                return;
            }
#endif

            multiplier = Math.Abs(multiplier);
            foreach (var handler in s_ShakeHandlers)
                 handler.AddShake(shake, multiplier);
        }
    }
}

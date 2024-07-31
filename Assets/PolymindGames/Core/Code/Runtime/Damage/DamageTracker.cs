using System.Collections.Generic;
using UnityEngine;

namespace PolymindGames
{
    public delegate void DamageDealtDelegate(IDamageReceiver receiver, DamageResult result, float damage, in DamageArgs args);

    public static class DamageTracker
    {
        private static readonly Dictionary<IDamageSource, DamageDealtDelegate> s_DamageDealtHandlers = new();


        public static void RegisterSource(IDamageSource source)
        {
#if DEBUG
            if (source == null)
            {
                Debug.LogError("Cannot register null source.");
                return;
            }
#endif

            if (!s_DamageDealtHandlers.TryAdd(source, null))
                Debug.LogError($"Source is already registered, {source}.");
        }

        public static void UnregisterSource(IDamageSource source)
        {
#if DEBUG
            if (source == null)
            {
                Debug.LogError("Cannot register null source.");
                return;
            }
#endif

            if (!s_DamageDealtHandlers.Remove(source))
                Debug.LogError($"Source is not registered, {source}.");
        }

        public static void RegisterDamage(IDamageReceiver receiver, DamageResult result, float damage, in DamageArgs args)
        {
            var source = args.Source;

#if DEBUG
            if (source == null)
            {
                Debug.LogError("Cannot register damage with a null source.");
                return;
            }
#endif

            if (s_DamageDealtHandlers.TryGetValue(source, out var damageDealtHandler))
                damageDealtHandler?.Invoke(receiver, result, damage, in args);
            else
                Debug.LogWarning($"The damage source is not registered, {source}.");
        }

        public static void AddListener(IDamageSource source, DamageDealtDelegate listener)
        {
#if DEBUG
            if (source == null)
            {
                Debug.LogError("Cannot listen to a null source.");
                return;
            }
#endif

            if (s_DamageDealtHandlers.TryGetValue(source, out var handler))
            {
                handler += listener;
                s_DamageDealtHandlers[source] = handler;
            }
            else
                Debug.LogWarning($"The damage source is not registered, {source}.");

        }

        public static void RemoveListener(IDamageSource source, DamageDealtDelegate listener)
        {
#if DEBUG
            if (source == null)
            {
                Debug.LogError("Cannot listen to a null source.");
                return;
            }
#endif

            if (s_DamageDealtHandlers.TryGetValue(source, out var handler))
            {
                handler -= listener;
                s_DamageDealtHandlers[source] = handler;
            }
            else
                Debug.LogWarning($"The damage source is not registered {source}.");
        }
    }
}
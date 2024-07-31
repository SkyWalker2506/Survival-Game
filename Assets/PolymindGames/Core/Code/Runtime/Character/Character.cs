using PolymindGames.InventorySystem;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;
using System.Linq;
using System;

namespace PolymindGames
{
    /// <summary>
    /// Main character class used by every entity in the game.
    /// It mainly acts as a hub for accessing components.
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_2)]
    public abstract class Character : MonoBehaviour, ICharacter
    {
        private Dictionary<Type, ICharacterComponent> _components;
        
        private static readonly List<ICharacterComponent> s_CachedComponents;
        private static readonly Dictionary<Type, Type> s_CharacterComponentToInterfacePairs;
        

        static Character()
        {
            Type baseType = typeof(ICharacterComponent);
            var ccImplementations =
                baseType.Assembly.GetTypes().Where(type => !type.IsInterface && baseType.IsAssignableFrom(type) && type != baseType).ToArray();

            int capacity = ccImplementations.Length;
            s_CharacterComponentToInterfacePairs = new Dictionary<Type, Type>(capacity);
            s_CachedComponents = new List<ICharacterComponent>(capacity);

            foreach (var ccImplementation in ccImplementations)
            {
                var ccInterfaces = ccImplementation.GetInterfaces();
                foreach (var ccInterface in ccInterfaces)
                {
                    if (ccInterface != baseType && baseType.IsAssignableFrom(ccInterface))
                    {
                        s_CharacterComponentToInterfacePairs.Add(ccImplementation, ccInterface);
                        break;
                    }
                }
            }
        }
        
        public virtual string Name => gameObject.name;
        public IAudioPlayer AudioPlayer { get; private set; }
        public IHealthManager HealthManager { get; private set; }
        public IInventory Inventory { get; private set; }
        string IDamageSource.SourceName => Name;

        public event UnityAction<ICharacter> Destroyed;

        public abstract Transform GetTransformOfBodyPoint(BodyPoint point);

        /// <summary>
        /// <para> Returns child component of specified type from this character. </para>
        /// Use this if you are NOT sure this character has a component of the given type.
        /// </summary>
        public bool TryGetCC<T>(out T component) where T : class, ICharacterComponent
        {
            if (_components.TryGetValue(typeof(T), out var cc))
            {
                component = (T)cc;
                return true;
            }

            component = null;
            return false;
        }

        /// <summary>
        /// <para> Returns child component of specified type from this character. </para>
        /// Use this if you ARE sure this character has a component of the given type.
        /// </summary>
        public T GetCC<T>() where T : class, ICharacterComponent
        {
            if (_components.TryGetValue(typeof(T), out ICharacterComponent component))
                return (T)component;

            return null;
        }

        protected virtual void Awake()
        {
            _components = GetCharacterComponentsInChildren(gameObject);

            AudioPlayer = GetComponentInChildren<IAudioPlayer>()
                          ?? new DefaultAudioPlayer(this);

            HealthManager = GetComponentInChildren<IHealthManager>();
            Inventory = GetComponentInChildren<IInventory>();

            DamageTracker.RegisterSource(this);
        }

        protected virtual void OnDestroy()
        {
            Destroyed?.Invoke(this);
            DamageTracker.UnregisterSource(this);
        }

        private static Dictionary<Type, ICharacterComponent> GetCharacterComponentsInChildren(GameObject root)
        {
            // Find & Setup all of the components
            root.GetComponentsInChildren(s_CachedComponents);

            var components = new Dictionary<Type, ICharacterComponent>(s_CachedComponents.Count);
            foreach (var component in s_CachedComponents)
            {
                var interfaceType = s_CharacterComponentToInterfacePairs[component.GetType()];

#if DEBUG
                if (!components.TryAdd(interfaceType, component))
                    Debug.LogError($"2 character components of the same type ({component.GetType()}) found under {root.name}.", root);
#else
                components.Add(interfaceType, component);
#endif
            }

            return components;
        }
        
        #region Internal
        private sealed class DefaultAudioPlayer : IAudioPlayer
        {
            private readonly Character _character;

            public DefaultAudioPlayer(Character character) => _character = character;
            
            #region IAudioPlayer Members
            public void Play(AudioClip clip, BodyPoint point, float volume = 1, float pitch = 1)
            {
                var trs = _character.GetTransformOfBodyPoint(point);
                AudioManager.Instance.PlayClipAtPoint(clip, trs.position, volume, pitch);
            }

            public void PlayDelayed(AudioClip clip, BodyPoint point, float delay, float volume = 1, float pitch = 1)
            {
                var trs = _character.GetTransformOfBodyPoint(point);
                AudioManager.Instance.PlayClipAtTransformDelayed(clip, trs, delay, volume, pitch);
            }

            public int StartLoop(AudioClip clip, BodyPoint point, float volume = 1, float pitch = 1, float duration = Mathf.Infinity)
            {
                var trs = _character.GetTransformOfBodyPoint(point);
                return AudioManager.Instance.StartLoopAtTransform(clip, trs, true, volume, pitch, duration);
            }

            public bool IsLoopPlaying(int loopId) => AudioManager.Instance.IsLoopPlaying(loopId);
            public void StopLoop(ref int loopId) => AudioManager.Instance.StopLoop(ref loopId);
            #endregion
        }
        #endregion


        #region Editor
#if UNITY_EDITOR
        protected virtual void Reset()
        {
            gameObject.layer = LayerConstants.CHARACTER;
        }

        public ICharacterComponent GetCC_Editor(Type type)
        {
            _components ??= GetCharacterComponentsInChildren(gameObject);
            _components.TryGetValue(type, out var characterComponent);
            return characterComponent;
        }
#endif
        #endregion
    }
}
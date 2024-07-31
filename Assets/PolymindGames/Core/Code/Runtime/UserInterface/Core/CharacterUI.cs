using System.Collections.Generic;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_2)]
    public class CharacterUI : MonoBehaviour, ICharacterUI
    {
        [SerializeField, BeginGroup, EndGroup]
        private AttachMode _attachMode = AttachMode.Manual;

        private readonly List<ICharacterUIBehaviour> _behaviours = new();
        private bool _isDestroyed;
        

        public ICharacter Character { get; private set; }

        public void AttachToCharacter(ICharacter character)
        {
#if DEBUG
            if (Character == character)
            {
                Debug.LogWarning("Character is already attached.", gameObject);
                return;
            }
#endif

            if (Character != null)
                Character.Destroyed -= OnDestroyed;

            Character = character;

            if (character != null)
                character.Destroyed += OnDestroyed;

            foreach (var behaviour in _behaviours)
                behaviour.OnCharacterChanged(character);

            return;

            void OnDestroyed(ICharacter c)
            {
                c.Destroyed -= OnDestroyed;
                Character = null;
                foreach (var behaviour in _behaviours)
                    behaviour.OnCharacterChanged(null);
            }
        }

        public void AddBehaviour(ICharacterUIBehaviour behaviour)
        {
#if DEBUG
            if (_isDestroyed)
            {
                Debug.LogError("Character UI is destroyed, cannot add a behaviour to it.");
                return;
            }

            if (behaviour == null)
            {
                Debug.LogWarning("Behaviour is null.");
                return;
            }

            if (_behaviours.Contains(behaviour))
            {
                Debug.LogWarning($"Behaviour is already added.", behaviour.gameObject);
                return;
            }
#endif

            _behaviours.Add(behaviour);

            if (Character != null)
                behaviour.OnCharacterChanged(Character);
        }

        public void RemoveBehaviour(ICharacterUIBehaviour behaviour)
        {
            if (!_isDestroyed)
            {
#if DEBUG
                if (behaviour == null)
                {
                    Debug.LogWarning("Behaviour is null.");
                    return;
                }

                if (!_behaviours.Remove(behaviour))
                {
                    Debug.LogWarning($"Behaviour is already removed.", behaviour.gameObject);
                    return;
                }
#else
                _behaviours.Remove(behaviour);
#endif
            }

            behaviour.OnCharacterChanged(null);
        }
        
        protected virtual void Start()
        {
            if (_attachMode == AttachMode.Manual)
                return;

            ICharacter character = null;

            switch (_attachMode)
            {
                case AttachMode.ToParentCharacter:
                    character = gameObject.GetComponentInRoot<ICharacter>();
                    break;
                case AttachMode.ToChildCharacter:
                    character = GetComponentInChildren<ICharacter>();
                    break;
            }

            if (character != null)
                AttachToCharacter(character);
#if DEBUG
            else
                Debug.LogWarning("No character found for this UI", gameObject);
#endif
        }

        protected virtual void OnDestroy()
        {
            _isDestroyed = true;
            _behaviours.Clear();
        }

        #region Internal
        private enum AttachMode
        {
            Manual,
            ToParentCharacter,
            ToChildCharacter
        }
        #endregion
    }
}
﻿using UnityEngine;

namespace PolymindGames.UserInterface
{
    /// <summary>
    /// Inherit from this if you need access to the parent character and its components
    /// </summary>
    public abstract class CharacterUIBehaviour : MonoBehaviour, ICharacterUIBehaviour
    {
        protected ICharacter Character { get; private set; }
        protected ICharacterUI CharacterUI { get; private set; }


        void ICharacterUIBehaviour.OnCharacterChanged(ICharacter character)
        {
            if (Character == character)
                return;

            if (Character != null)
                OnCharacterDetached(Character);

            Character = character;
            if (character != null)
                OnCharacterAttached(character);
        }

        protected virtual void Awake()
        {
            CharacterUI = gameObject.GetComponentInRoot<ICharacterUI>();

            if (CharacterUI == null)
            {
                Debug.LogError("No character UI found in the root game object.", gameObject);
                return;
            }

            CharacterUI.AddBehaviour(this);
        }

        protected virtual void OnDestroy()
        {
            CharacterUI?.RemoveBehaviour(this);
        }

        /// <summary>
        /// Gets called when this UI behaviour gets attached to a Character.
        /// </summary>
        protected virtual void OnCharacterAttached(ICharacter character) { }

        /// <summary>
        /// Gets called when this UI behaviour gets detached from the Character.
        /// </summary>
        protected virtual void OnCharacterDetached(ICharacter character) { }
    }
}
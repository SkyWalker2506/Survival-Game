using PolymindGames.UserInterface;
using UnityEngine;

namespace PolymindGames.InputSystem.Behaviours
{
    public abstract class PlayerUIInputBehaviour : CharacterUIBehaviour, IInputBehaviour
    {
        public bool Enabled
        {
            get => enabled;
            set => enabled = value;
        }
        
        [field: SerializeField, BeginGroup, EndGroup]
        public InputEnableMode EnableMode { get; private set; } = InputEnableMode.BasedOnContext;


        protected override void Awake()
        {
            base.Awake();
            InputManager.Instance.RegisterBehaviour(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            InputManager.Instance.UnregisterBehaviour(this);
        }
    }
}
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames
{
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_2)]
    public class Player : Character
    {
        [SerializeField, NotNull, BeginGroup]
        [Tooltip("The head root of this player (you can think of it as the eyes of the character)")]
        private Transform _headTransform;

        [SerializeField, NotNull]
        [Tooltip("The body root of this player.")]
        private Transform _torsoTransform;

        [SerializeField, NotNull]
        [Tooltip("Feet root of this player.")]
        private Transform _feetTransform;
        
        [SerializeField, NotNull, EndGroup]
        [Tooltip("Hands root of this player.")]
        private Transform _handsTransform;

        private string _name;
        
        
        public static Player LocalPlayer { get; private set; }
        public static event UnityAction LocalPlayerChanged;
        
        public override string Name => _name;

        public override Transform GetTransformOfBodyPoint(BodyPoint point) => point switch
        {
            BodyPoint.Head => _headTransform,
            BodyPoint.Torso => _torsoTransform,
            BodyPoint.Hands => _handsTransform,
            _ => _feetTransform
        };

        public void SetName(string value) => _name = value;

        protected override void Awake()
        {
            if (LocalPlayer != null)
                Destroy(gameObject);
            else
            {
                LocalPlayer = this;
                LocalPlayerChanged?.Invoke();
                base.Awake();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (LocalPlayer == this)
                LocalPlayer = null;
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            gameObject.tag = TagConstants.PLAYER;
        }
#endif
    }
}
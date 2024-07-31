using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [DefaultExecutionOrder(ExecutionOrderConstants.AFTER_DEFAULT_2)]
    [RequireCharacterComponent(typeof(IWieldableControllerCC))]
    public sealed class WieldableRetractionHandler : CharacterBehaviour, IWieldableRetractionHandlerCC
    {
        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("The transform used in ray-casting.")]
        private Transform _view;

        [SerializeField, Range(0.01f, 25f), BeginGroup]
        [Tooltip("The max detection distance, anything out of range will be ignored.")]
        private float _distance = 0.85f;

        [SerializeField, Range(0f, 1f)]
        [Tooltip("If set to a value larger than 0, the detection method will be set to SphereCast instead of Raycast.")]
        private float _radius = 0.04f;

        [SerializeField, Range(0.01f, 25f), EndGroup]
        private float _blockDistance = 0.45f;

        private IWieldable _wieldable;
        private Transform _ignoredRoot;
        private bool _isBlocked; 

        
        public float ClosestObjectDistance { get; private set; }

        protected override void OnBehaviourStart(ICharacter character)
        {
            base.OnBehaviourStart(character);
            _ignoredRoot = character.transform;
        }

        protected override void OnBehaviourEnable(ICharacter character)
        {
            var controller = character.GetCC<IWieldableControllerCC>();
            controller.EquippingStarted += OnEquippingStarted;
            _wieldable = controller.ActiveWieldable;
        }

        protected override void OnBehaviourDisable(ICharacter character)
        {
            var controller = character.GetCC<IWieldableControllerCC>();
            controller.EquippingStarted -= OnEquippingStarted;
        }

        private void OnEquippingStarted(IWieldable wieldable)
        {
            BlockWieldableActions(false);
            _wieldable = wieldable;
        }

        private void FixedUpdate()
        {
            if (_wieldable == null)
                return;

            Ray ray = new(_view.position, _view.forward);

            ClosestObjectDistance = _radius > 0.001f
                ? PhysicsUtils.SphereCastOptimizedClosestDistance(ray, _radius, _distance, LayerConstants.SIMPLE_SOLID_OBJECTS_MASK, _ignoredRoot)
                : PhysicsUtils.RaycastOptimizedClosestDistance(ray, _distance, LayerConstants.SIMPLE_SOLID_OBJECTS_MASK, _ignoredRoot);

            BlockWieldableActions(ClosestObjectDistance < _blockDistance);
        }

        private void BlockWieldableActions(bool block)
        {
            if (_isBlocked == block || _wieldable == null)
                return;

            if (block)
            {
                if (_wieldable is IUseInputHandler useInput)
                    useInput.UseBlocker.AddBlocker(this);

                if (_wieldable is IAimInputHandler aimInput)
                    aimInput.AimBlocker.AddBlocker(this);
            }
            else
            {
                if (_wieldable is IUseInputHandler useInput)
                    useInput.UseBlocker.RemoveBlocker(this);

                if (_wieldable is IAimInputHandler aimInput)
                    aimInput.AimBlocker.RemoveBlocker(this);
            }

            _isBlocked = block;
        }
    }
}
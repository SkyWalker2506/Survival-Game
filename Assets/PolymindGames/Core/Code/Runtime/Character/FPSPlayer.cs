using PolymindGames.ProceduralMotion;
using UnityEngine;

namespace PolymindGames
{
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_2)]
    public sealed class FPSPlayer : Player, IFPSCharacter
    {
        [SerializeField, NotNull, BeginGroup]
        private CharacterShakeHandler _shakeHandler;

        [SerializeField, NotNull]
        private MotionMixer _headMotionMixer;

        [SerializeField, NotNull]
        private MotionDataHandler _headMotionDataHandler;

        [SerializeField, NotNull]
        private MotionMixer _handsMotionMixer;

        [SerializeField, NotNull, EndGroup]
        private MotionDataHandler _handsMotionDataHandler;
        
        
        public IShakeHandler ShakeHandler => _shakeHandler;
        public IMotionMixer HeadMotionMixer => _headMotionMixer;
        public IMotionMixer HandsMotionMixer => _handsMotionMixer;
        public IMotionDataHandler HeadMotionDataHandler => _headMotionDataHandler;
        public IMotionDataHandler HandsMotionDataHandler => _handsMotionDataHandler;

        private void OnEnable()
        {
            UnityUtils.LockCursor();
            if (TryGetCC(out IMovementControllerCC movement))
                movement.AddStateTransitionListener(MovementStateType.None, OnStateTypeChanged);
        }

        private void OnDisable()
        {
            if (LocalPlayer != this)
                return;
            
            UnityUtils.UnlockCursor();
            if (TryGetCC(out IMovementControllerCC movement))
                movement.RemoveStateTransitionListener(MovementStateType.None, OnStateTypeChanged);
        }

        private void OnStateTypeChanged(MovementStateType stateType)
        {
            _headMotionDataHandler.SetStateType(stateType);
            _handsMotionDataHandler.SetStateType(stateType);
        }
    }
}
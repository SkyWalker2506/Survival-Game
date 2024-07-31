using System;
using System.Collections.Generic;
using UnityEngine;

namespace PolymindGames.ProceduralMotion
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(AdditiveForceMotion))]
    [RequireCharacterComponent(typeof(IMovementControllerCC))]
    [AddComponentMenu("Polymind Games/Motion/Offset Motion")]
    public sealed class OffsetMotion : DataMotionBehaviour<IOffsetMotionData>
    {
        [SerializeField, ReorderableList(ListStyle.Boxed, elementLabel: "From")]
        private OffsetTransitionData[] _ignoredMovementTransitions = Array.Empty<OffsetTransitionData>();

        [SerializeField, ReorderableList(ListStyle.Boxed, HasLabels = false)]
        private MovementStateType[] _ignoredCustomTransitions = Array.Empty<MovementStateType>();

        private readonly List<TransitionForce> _transitionForces = new(4);

        private MovementStateType _prevStateType;
        private IMovementControllerCC _movement;
        private IOffsetMotionData _prevData;

        private const float POSITION_OFFSET_MOD = 0.01f;
        private const float ROTATION_FORCE_MOD = 3f;


        protected override void OnBehaviourStart(ICharacter character)
        {
            _movement = character.GetCC<IMovementControllerCC>();
            _prevStateType = _movement.ActiveState;
        }

        protected override void OnDataChanged(IOffsetMotionData data)
        {
            AllowedTransitionType allowedTransitions = data != _prevData
                ? CanAddTransitionForce(_prevStateType, _movement.ActiveState)
                : AllowedTransitionType.None;

            // Add exit force
            if (allowedTransitions is AllowedTransitionType.Both or AllowedTransitionType.Exit && _prevData != null)
            {
                var force = new TransitionForce(_prevData.ExitForce.Force * ROTATION_FORCE_MOD, _prevData.ExitForce.Duration);
                _transitionForces.Add(force);
            }

            if (data != null)
            {
                PositionSpring.Adjust(data.PositionSettings);
                RotationSpring.Adjust(data.RotationSettings);

                // Add enter force
                if (allowedTransitions is AllowedTransitionType.Both or AllowedTransitionType.Enter)
                {
                    var force = new TransitionForce(data.EnterForce.Force * ROTATION_FORCE_MOD, data.EnterForce.Duration);
                    _transitionForces.Add(force);
                }
            }

            _prevData = data;
            _prevStateType = _movement.ActiveState;
        }

        public override void UpdateMotion(float deltaTime)
        {
            if (Data == null)
            {
                if (!RotationSpring.IsIdle)
                    SetTargetRotation(EvaluateTransitionForces(Time.time));
                return;
            }

            SetTargetPosition(Data.PositionOffset, POSITION_OFFSET_MOD);
            SetTargetRotation(Data.RotationOffset + EvaluateTransitionForces(Time.time));
        }

        private AllowedTransitionType CanAddTransitionForce(MovementStateType from, MovementStateType to)
        {
            if (from == to)
            {
                for (int i = 0; i < _ignoredCustomTransitions.Length; i++)
                {
                    if (_ignoredCustomTransitions[i] == from)
                        return AllowedTransitionType.Enter;
                }
            }
            else
            {
                for (int i = 0; i < _ignoredMovementTransitions.Length; i++)
                {
                    var transition = _ignoredMovementTransitions[i];
                    if (to == transition.To && from == transition.From)
                        return AllowedTransitionType.None;
                }
            }

            return AllowedTransitionType.Both;
        }

        private Vector3 EvaluateTransitionForces(float time)
        {
            Vector3 force = Vector3.zero;
            for (int i = _transitionForces.Count - 1; i >= 0; i--)
            {
                force += _transitionForces[i].Force;

                if (time > _transitionForces[i].EndTime)
                    _transitionForces.RemoveAt(i);
            }

            return force;
        }

        #region Internal
        [Serializable]
        private struct OffsetTransitionData
        {
            public MovementStateType From;
            public MovementStateType To;
        }

        private enum AllowedTransitionType : byte
        {
            None,
            Enter,
            Exit,
            Both
        }

        private readonly struct TransitionForce
        {
            public readonly Vector3 Force;
            public readonly float EndTime;

            public TransitionForce(Vector3 force, float duration)
            {
                Force = force;
                EndTime = Time.time + duration;
            }
        }
        #endregion
    }
}
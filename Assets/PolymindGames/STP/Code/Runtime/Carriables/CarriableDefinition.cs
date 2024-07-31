using PolymindGames.ProceduralMotion;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolymindGames.WieldableSystem
{
    [CreateAssetMenu(menuName = "Polymind Games/Building/Carriable Definition", fileName = "Carriable_")]
    public sealed class CarriableDefinition : DataDefinition<CarriableDefinition>
    {
        [SerializeField, NewLabel("Name "), BeginGroup]
        [Tooltip("Carriable name.")]
        private string _carriableName;
        
        [SerializeField, NotNull, PrefabObjectOnly, EndGroup]
        [Tooltip("Corresponding pickup for this carriable, so you can actually drop it, or pick it up from the ground.")]
        private CarriablePickup _pickup;

        [SerializeField, BeginGroup("Dropping")]
        private Vector3 _dropForce;

        [SerializeField, Range(0f, 1f), EndGroup]
        private float _dropTorque = 0.25f;
        
        [SerializeField, BeginGroup, EndGroup]
        [NestedScriptableListInLine(Foldable = false)]
        private CarriableAction[] _useActions;

        [SerializeField, BeginGroup, EndGroup]
        [EditorButton(nameof(GetOffsetsFromTransforms), null, ButtonActivityType.OnPlayMode)]
        [EditorButton(nameof(RefreshVisuals), null, ButtonActivityType.OnPlayMode)]
        private WieldableCarrySettings _wieldableSettings;
        
        
        public override string Name
        {
            get => _carriableName;
            protected set => _carriableName = value;
        }

        public CarriablePickup Pickup => _pickup;
        public Vector3 DropForce => _dropForce;
        public float DropTorque => _dropTorque;
        public int MaxCarryCount => _wieldableSettings.Offsets.Length;
        public IEnumerable<CarriableAction> UseActions => _useActions;
        public WieldableCarrySettings WieldableSettings => _wieldableSettings;

        [Conditional("UNITY_EDITOR")]
        private void GetOffsetsFromTransforms()
        {
#if UNITY_EDITOR
            var wieldableCarriable = Player.LocalPlayer.GetCC<IWieldableControllerCC>().gameObject.GetComponentInChildren<WieldableCarriable>();

            Undo.RecordObject(this, "carriable");
            
            var offsets = _wieldableSettings.Offsets;
            for (int i = 0; i < wieldableCarriable.CarryCount; i++)
            {
                var (position, rotation) = wieldableCarriable.GetOffsetsAtIndex(i);
                offsets[i].PositionOffset = position;
                offsets[i].RotationOffset = rotation;
            }
            
            EditorUtility.SetDirty(this);
#endif
        }
        
        [Conditional("UNITY_EDITOR")]
        private void RefreshVisuals()
        { 
#if UNITY_EDITOR
            var wieldableCarriable = Player.LocalPlayer.GetCC<IWieldableControllerCC>().gameObject.GetComponentInChildren<WieldableCarriable>();
            wieldableCarriable.RefreshVisuals();
#endif
        }
    }
    
    [Serializable]
    public sealed class WieldableCarrySettings
    {
        [Serializable]
        public struct Offset
        {
            public Vector3 PositionOffset;
            public Vector3 RotationOffset;
        }
        
        public enum Socket : byte
        {
            RightHand,
            LeftHand
        }
        
        [SerializeField, NotNull]
        [Tooltip("The animation override clips.")]
        private AnimatorOverrideController _animator;

        [SerializeField, NotNull]
        private MotionProfile _motion;
        
        [SerializeField]
        private Socket _socket;

        [SerializeField]
        private Vector3 _positionOffset;

        [SerializeField]
        private Vector3 _rotationOffset;
        
        [SerializeField, SpaceArea]
        [ReorderableList(ListStyle.Lined, ElementLabel = "Offset")]
        private Offset[] _offsets;
            
            
        public AnimatorOverrideController Animator => _animator;
        public MotionProfile Motion => _motion;
        public Socket TargetSocket => _socket;
        public Vector3 PositionOffset => _positionOffset;
        public Vector3 RotationOffset => _rotationOffset;
        public Offset[] Offsets => _offsets;
    }
}
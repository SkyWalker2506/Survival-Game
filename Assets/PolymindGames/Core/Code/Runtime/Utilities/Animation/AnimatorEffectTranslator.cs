using System;
using System.Linq;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [Serializable]
    public abstract class AnimatorTranslatorData : ISerializationCallbackReceiver, IComparable<AnimatorTranslatorData>
    {
        [NonSerialized]
        public int Hash;

        public AnimatorControllerParameterType ParamType = AnimatorControllerParameterType.Trigger;
            
        [DisableInPlayMode]
        [AnimatorParameter(nameof(ParamType))]
        public string ParamName;
            
        [HideIf(nameof(ParamType), AnimatorControllerParameterType.Trigger)]
        public int TargetValue;
            
            
        void ISerializationCallbackReceiver.OnBeforeSerialize() { }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            if (Hash == 0)
                Hash = Animator.StringToHash(ParamName);

            if (ParamType == AnimatorControllerParameterType.Trigger)
                TargetValue = 0;
        }

        public int CompareTo(AnimatorTranslatorData other) => ParamType.CompareTo(other.ParamType);
    }
    
    public abstract class AnimatorEffectTranslator<AnimationData> : MonoBehaviour, IAnimator where AnimationData : AnimatorTranslatorData
    {
        [LabelFromChild(nameof(AnimatorTranslatorData.ParamName))]
        [SerializeField, ReorderableList(ListStyle.Boxed, elementLabel: "Animation")]
        protected AnimationData[] _animations;
        
        private int _integerEndIndex;
        private int _boolEndIndex;
        private int _triggerStartIndex;
        private bool _isAnimating;


        public bool IsAnimating
        {
            get => _isAnimating;
            set => _isAnimating = value;
        }

        public bool IsVisible
        {
            get => true;
            set { }
        }

        public void SetFloat(int id, float value)
        {
            int intValue = Mathf.FloorToInt(value);
            SetInteger(id, intValue);
        }

        public void SetInteger(int id, int value)
        {
            if (_isAnimating && _integerEndIndex != -1)
                Play(id, _animations.AsSpan(0, _integerEndIndex + 1), value);
        }

        public void SetBool(int id, bool value)
        {
            if (_isAnimating && _boolEndIndex != -1)
                Play(id, _animations.AsSpan(_integerEndIndex + 1, _boolEndIndex), value ? 1 : 0);
        }
        
        public void SetTrigger(int id)
        {
            if (_isAnimating && _triggerStartIndex != -1)
                Play(id, _animations.AsSpan(_triggerStartIndex), 0);
        }

        public void ResetTrigger(int id) { }
        
        protected abstract void PlayAnimation(AnimationData animationData);

        private void Play(int id, Span<AnimationData> animations, int value)
        {
            foreach (var animData in animations)
            {
                if (animData.Hash == id && animData.TargetValue == value)
                {
                    PlayAnimation(animData);
                    return;
                }
            }
        }
        
        protected virtual void Awake()
        {
            _isAnimating = true;
            CalculateIndexes();
        }

        private void CalculateIndexes()
        {
            _integerEndIndex = Array.FindLastIndex(_animations, data => data.ParamType is AnimatorControllerParameterType.Float or AnimatorControllerParameterType.Int);
            _boolEndIndex = Array.FindLastIndex(_animations, data => data.ParamType is AnimatorControllerParameterType.Bool);
            _triggerStartIndex = Array.FindIndex(_animations, data => data.ParamType is AnimatorControllerParameterType.Trigger);
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (!_animations.IsOrdered())
                _animations = _animations.OrderBy(data => data.ParamType).ToArray();
            
            if (Application.isPlaying)
                CalculateIndexes();
        }
#endif

        #region Old
        // #if UNITY_EDITOR
        // private void Convert()
        // {
        //     foreach (var data in _triggerParameterAnimations)
        //         Convert(data);
        //
        //     foreach (var data in _floatParameterAnimations)
        //         Convert(data);
        //
        //     foreach (var data in _boolParameterAnimations)
        //         Convert(data);
        // }
        //
        // private void Convert(CameraTriggerAnimationData data)
        // {
        //     Undo.RecordObject(this, "Convert");
        //
        //     var xCurve = new AnimationCurve(new Keyframe(0f, 0f));
        //     var yCurve = new AnimationCurve(new Keyframe(0f, 0f));
        //     var zCurve = new AnimationCurve(new Keyframe(0f, 0f));
        //
        //     for (int i = 0; i < data.CameraForces.Length; i++)
        //     {
        //         var force = data.CameraForces[i];
        //         xCurve.AddKey(force.Delay, force.SpringForce.Force.x);
        //         yCurve.AddKey(force.Delay, force.SpringForce.Force.y);
        //         zCurve.AddKey(force.Delay, force.SpringForce.Force.z);
        //     }
        //
        //     if (xCurve.keys[^1].time > 0.01f)
        //         xCurve.AddKey(new Keyframe(xCurve.keys[^1].time + 0.1f, 0f));
        //
        //     if (yCurve.keys[^1].time > 0.01f)
        //         yCurve.AddKey(new Keyframe(yCurve.keys[^1].time + 0.1f, 0f));
        //
        //     if (zCurve.keys[^1].time > 0.01f)
        //         zCurve.AddKey(new Keyframe(zCurve.keys[^1].time + 0.1f, 0f));
        //
        //     for (int i = 0; i < xCurve.keys.Length; i++)
        //         AnimationUtility.SetKeyLeftTangentMode(xCurve, i, AnimationUtility.TangentMode.Constant);
        //
        //     for (int i = 0; i < yCurve.keys.Length; i++)
        //         AnimationUtility.SetKeyLeftTangentMode(yCurve, i, AnimationUtility.TangentMode.Constant);
        //
        //     for (int i = 0; i < zCurve.keys.Length; i++)
        //         AnimationUtility.SetKeyLeftTangentMode(zCurve, i, AnimationUtility.TangentMode.Constant);
        //
        //     data.Animation = new AnimCurves3D(1.5f, xCurve, yCurve, zCurve);
        //
        //     EditorUtility.SetDirty(this);
        // }
        //
        // private void Convert(CameraFloatAnimationData data)
        // {
        //     Undo.RecordObject(this, "Convert");
        //
        //     var xCurve = new AnimationCurve(new Keyframe(0f, 0f));
        //     var yCurve = new AnimationCurve(new Keyframe(0f, 0f));
        //     var zCurve = new AnimationCurve(new Keyframe(0f, 0f));
        //
        //     for (int i = 0; i < data.CameraForces.Length; i++)
        //     {
        //         var force = data.CameraForces[i];
        //         xCurve.AddKey(force.Delay, force.SpringForce.Force.x);
        //         yCurve.AddKey(force.Delay, force.SpringForce.Force.y);
        //         zCurve.AddKey(force.Delay, force.SpringForce.Force.z);
        //     }
        //
        //     if (xCurve.keys[^1].time > 0.01f)
        //         xCurve.AddKey(new Keyframe(xCurve.keys[^1].time + 0.1f, 0f));
        //
        //     if (yCurve.keys[^1].time > 0.01f)
        //         yCurve.AddKey(new Keyframe(yCurve.keys[^1].time + 0.1f, 0f));
        //
        //     if (zCurve.keys[^1].time > 0.01f)
        //         zCurve.AddKey(new Keyframe(zCurve.keys[^1].time + 0.1f, 0f));
        //
        //     for (int i = 0; i < xCurve.keys.Length; i++)
        //         AnimationUtility.SetKeyLeftTangentMode(xCurve, i, AnimationUtility.TangentMode.Constant);
        //
        //     for (int i = 0; i < yCurve.keys.Length; i++)
        //         AnimationUtility.SetKeyLeftTangentMode(yCurve, i, AnimationUtility.TangentMode.Constant);
        //
        //     for (int i = 0; i < zCurve.keys.Length; i++)
        //         AnimationUtility.SetKeyLeftTangentMode(zCurve, i, AnimationUtility.TangentMode.Constant);
        //
        //     data.Animation = new AnimCurves3D(1.5f, xCurve, yCurve, zCurve);
        //
        //     EditorUtility.SetDirty(this);
        // }
        //
        // private void Convert(CameraBoolAnimationData data)
        // {
        //     Undo.RecordObject(this, "Convert");
        //
        //     var xCurve = new AnimationCurve(new Keyframe(0f, 0f));
        //     var yCurve = new AnimationCurve(new Keyframe(0f, 0f));
        //     var zCurve = new AnimationCurve(new Keyframe(0f, 0f));
        //
        //     for (int i = 0; i < data.CameraForces.Length; i++)
        //     {
        //         var force = data.CameraForces[i];
        //         xCurve.AddKey(force.Delay, force.SpringForce.Force.x);
        //         yCurve.AddKey(force.Delay, force.SpringForce.Force.y);
        //         zCurve.AddKey(force.Delay, force.SpringForce.Force.z);
        //     }
        //
        //     if (xCurve.keys[^1].time > 0.01f)
        //         xCurve.AddKey(new Keyframe(xCurve.keys[^1].time + 0.1f, 0f));
        //
        //     if (yCurve.keys[^1].time > 0.01f)
        //         yCurve.AddKey(new Keyframe(yCurve.keys[^1].time + 0.1f, 0f));
        //
        //     if (zCurve.keys[^1].time > 0.01f)
        //         zCurve.AddKey(new Keyframe(zCurve.keys[^1].time + 0.1f, 0f));
        //
        //     for (int i = 0; i < xCurve.keys.Length; i++)
        //         AnimationUtility.SetKeyLeftTangentMode(xCurve, i, AnimationUtility.TangentMode.Constant);
        //
        //     for (int i = 0; i < yCurve.keys.Length; i++)
        //         AnimationUtility.SetKeyLeftTangentMode(yCurve, i, AnimationUtility.TangentMode.Constant);
        //
        //     for (int i = 0; i < zCurve.keys.Length; i++)
        //         AnimationUtility.SetKeyLeftTangentMode(zCurve, i, AnimationUtility.TangentMode.Constant);
        //
        //     data.Animation = new AnimCurves3D(1.5f, xCurve, yCurve, zCurve);
        //
        //     EditorUtility.SetDirty(this);
        // }
        // #endif
        #endregion
    }
}
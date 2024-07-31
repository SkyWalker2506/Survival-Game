using System;
using UnityEngine;

namespace PolymindGames
{
    public interface IAnimator
    {
        bool IsAnimating { get; set; }
        bool IsVisible { set; get; }

        void SetFloat(int id, float value);
        void SetBool(int id, bool value);
        void SetInteger(int id, int value);
        void SetTrigger(int id);
        void ResetTrigger(int id);
    }

    public static class AnimatorExtensions
    {
        public static void SetParameter(this IAnimator animator, AnimatorControllerParameterType paramType, int hash, float value)
        {
            switch (paramType)
            {
                case AnimatorControllerParameterType.Float:
                    animator.SetFloat(hash, value);
                    break;
                case AnimatorControllerParameterType.Trigger:
                    animator.SetTrigger(hash);
                    break;
                case AnimatorControllerParameterType.Bool:
                    animator.SetBool(hash, value > 0f);
                    break;
                case AnimatorControllerParameterType.Int:
                    animator.SetInteger(hash, (int)value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static void SetFloat(this IAnimator animator, string param, float value)
        {
            var id = Animator.StringToHash(param);
            animator.SetFloat(id, value);
        }

        public static void SetBool(this IAnimator animator, string param, bool value)
        {
            var id = Animator.StringToHash(param);
            animator.SetBool(id, value);
        }

        public static void SetInteger(this IAnimator animator, string param, int value)
        {
            var id = Animator.StringToHash(param);
            animator.SetInteger(id, value);
        }

        public static void SetTrigger(this IAnimator animator, string param)
        {
            var id = Animator.StringToHash(param);
            animator.SetTrigger(id);
        }

        public static void ResetTrigger(this IAnimator animator, string param)
        {
            var id = Animator.StringToHash(param);
            animator.ResetTrigger(id);
        }
    }

    public sealed class MultiAnimator : IAnimator
    {
        private readonly IAnimator[] _animators;


        public MultiAnimator(IAnimator[] animators)
        {
            _animators = animators;
        }

        public bool IsAnimating
        {
            get => _animators.Length > 0 && _animators[0].IsAnimating;
            set
            {
                for (int i = 0; i < _animators.Length; i++)
                    _animators[i].IsAnimating = value;
            }
        }

        public bool IsVisible
        {
            get => _animators.Length > 0 && _animators[0].IsVisible;
            set
            {
                for (int i = 0; i < _animators.Length; i++)
                    _animators[i].IsVisible = value;
            }
        }

        public void SetFloat(int id, float value)
        {
            for (int i = 0; i < _animators.Length; i++)
                _animators[i].SetFloat(id, value);
        }

        public void SetBool(int id, bool value)
        {
            for (int i = 0; i < _animators.Length; i++)
                _animators[i].SetBool(id, value);
        }

        public void SetInteger(int id, int value)
        {
            for (int i = 0; i < _animators.Length; i++)
                _animators[i].SetInteger(id, value);
        }

        public void SetTrigger(int id)
        {
            for (int i = 0; i < _animators.Length; i++)
                _animators[i].SetTrigger(id);
        }

        public void ResetTrigger(int id)
        {
            for (int i = 0; i < _animators.Length; i++)
                _animators[i].ResetTrigger(id);
        }
    }
}
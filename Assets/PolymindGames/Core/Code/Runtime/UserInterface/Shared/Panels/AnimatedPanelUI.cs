using UnityEngine;

namespace PolymindGames.UserInterface
{
    [RequireComponent(typeof(Animator))]
    [AddComponentMenu("Polymind Games/User Interface/Panels/Animated Panel")]
    public class AnimatedPanelUI : AudioPanelUI
    {
        [SerializeField, Range(0f, 2f), BeginGroup("Animation")]
        private float _showSpeed = 1f;

        [SerializeField, Range(0f, 2f), EndGroup]
        private float _hideSpeed = 1f;
        
        private static readonly int ANIM_HIDE = Animator.StringToHash("Hide");
        private static readonly int ANIM_HIDE_SPEED = Animator.StringToHash("Hide Speed");
        private static readonly int ANIM_SHOW = Animator.StringToHash("Show");
        private static readonly int ANIM_SHOW_SPEED = Animator.StringToHash("Show Speed");

        private Coroutine _hideRoutine;
        private Animator _animator;

        private const float DISABLE_ANIMATOR_DELAY = 1f;


        protected override void OnShowPanel(bool show)
        {
            base.OnShowPanel(show);

            CoroutineUtils.StopCoroutine(this, ref _hideRoutine);
            _animator.enabled = true;
            
            if (!show)
            {
                _animator.SetTrigger(ANIM_HIDE);
                _hideRoutine = CoroutineUtils.InvokeDelayed(this, DisableAnimator, DISABLE_ANIMATOR_DELAY);
            }
            else
                _animator.SetTrigger(ANIM_SHOW);
        }

        protected override void Awake()
        {
            base.Awake();

            _animator = GetComponent<Animator>();
            _animator.SetFloat(ANIM_SHOW_SPEED, _showSpeed);
            _animator.SetFloat(ANIM_HIDE_SPEED, _hideSpeed);

            _animator.fireEvents = false;
            _animator.keepAnimatorStateOnDisable = true;
            _animator.writeDefaultValuesOnDisable = false;
            _animator.enabled = false;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            CoroutineUtils.StopCoroutine(this, ref _hideRoutine);
        }

        private void DisableAnimator()
        {
            _animator.enabled = false;
            _hideRoutine = null;
        }

#if UNITY_EDITOR
        protected override void Reset()
        {
            base.Reset();
            _animator = GetComponent<Animator>();
        }
#endif
    }
}
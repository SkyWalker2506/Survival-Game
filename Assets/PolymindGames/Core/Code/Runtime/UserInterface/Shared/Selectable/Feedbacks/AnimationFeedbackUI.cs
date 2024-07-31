using UnityEngine;

namespace PolymindGames.UserInterface
{
    [NestedObjectPath(MenuName = "Animation Feedback")]
    public sealed class AnimationFeedbackUI : SelectableFeedbackUI
    {
        [SerializeField, NotNull]
        private Animator _animator;
        
        private static readonly int s_Disabled = Animator.StringToHash("Disabled");
        private static readonly int s_Highlight = Animator.StringToHash("Highlighted");
        private static readonly int s_Normal = Animator.StringToHash("Normal");
        private static readonly int s_Pressed = Animator.StringToHash("Pressed");
        private static readonly int s_Selected = Animator.StringToHash("Selected");
        

        public override void OnNormal(bool instant) => _animator.SetTrigger(s_Normal);
        public override void OnHighlighted(bool instant) => _animator.SetTrigger(s_Highlight);
        public override void OnSelected(bool instant) => _animator.SetTrigger(s_Selected);
        public override void OnPressed(bool instant) => _animator.SetTrigger(s_Pressed);
        public override void OnDisabled(bool instant) => _animator.SetTrigger(s_Disabled);

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_animator == null)
                _animator = GetComponent<Animator>();
        }
#endif
    }
}
using PolymindGames.WieldableSystem;
using PolymindGames.PostProcessing;
using PolymindGames.InputSystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    public sealed class SurvivalBookUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup, EndGroup]
        private InputContext _bookContext;

        [SerializeField, BeginGroup]
        private SelectableGroupBaseUI _menuGroup;
        
        [SerializeField]
        private Canvas _menuCanvas;

        [SerializeField, EndGroup]
        private Canvas _contentCanvas;

        [SerializeField, BeginGroup, EndGroup]
        private VolumeAnimationProfile _blurAnimation;

        private WieldableTool _bookWieldable;
        
        
        public void StopBookInspection()
        {
            if (_bookWieldable != null)
            {
                var controller = Character.GetCC<IWieldableControllerCC>();
                controller.TryHolsterWieldable(_bookWieldable);
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            _menuCanvas.worldCamera = UnityUtils.CachedMainCamera;
            _contentCanvas.worldCamera = UnityUtils.CachedMainCamera;
            
            _bookWieldable = GetComponentInParent<WieldableTool>();

            if (_bookWieldable == null)
            {
                Debug.LogError("No Book Wieldable (Simple Tool) found in the parent", gameObject);
                return;
            }

            _bookWieldable.EquippingStarted += ShowBookUI;
            _bookWieldable.HolsteringStarted += HideBookUI;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            
            if (_bookWieldable != null)
            {
                _bookWieldable.EquippingStarted -= ShowBookUI;
                _bookWieldable.HolsteringStarted -= HideBookUI;
            }
        }

        private void ShowBookUI()
        {
            InputManager.Instance.PushEscapeCallback(StopBookInspection);
            InputManager.Instance.PushContext(_bookContext);
            PostProcessingManager.Instance.TryPlayAnimation(this, _blurAnimation);
        }

        private void OnDisable()
        {
            if (_menuGroup != null)
                _menuGroup.SelectDefault();
        }

        private void HideBookUI()
        {
            InputManager.Instance.PopEscapeCallback(StopBookInspection);
            InputManager.Instance.PopContext(_bookContext);
            PostProcessingManager.Instance.CancelAnimation(this, _blurAnimation);
        }
    }
}
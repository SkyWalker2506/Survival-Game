using System.Collections.Generic;
using PolymindGames.InputSystem;
using PolymindGames.InventorySystem;
using PolymindGames.PostProcessing;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.UserInterface
{
    public sealed class ItemWheelUI : CharacterUIBehaviour, IWheelUI
    {
        [SerializeField, BeginGroup, EndGroup]
        private InputContext _itemWheelContext;

        [SerializeField, NotNull, BeginGroup("References")]
        private SelectableGroupBaseUI _slotsGroup;

        [SerializeField, NotNull]
        private Transform _slotsRoot;

        [SerializeField, NotNull, EndGroup]
        private PanelUI _panel;

        [SerializeField, Range(0.1f, 25f), BeginGroup("Settings")]
        private float _range = 3f;

        [SerializeField, Range(0f, 5f)]
        private float _cooldown = 0.35f;

        [SerializeField, Range(0.1f, 25f), EndGroup]
        private float _sensitivity = 3f;

        [SerializeField, NotNull, BeginGroup("Item Info")]
        private GameObject _infoRoot;

        [SerializeField, IgnoreParent]
        private ItemNameInfo _nameInfo;

        [SerializeField, IgnoreParent]
        private ItemDescriptionInfo _descriptionInfo;

        [SerializeField, IgnoreParent]
        private ItemWeightInfo _weightInfo;

        [SerializeField, IgnoreParent, EndGroup]
        private ItemFiremodeInfo _firemodeInfo;

        [SerializeField, BeginGroup("Effects")]
        private VolumeAnimationProfile _inspectEffect;

        [SerializeField, SpaceArea]
        private UnityEvent _startInspectionEvent;

        [SerializeField, EndGroup]
        private UnityEvent _stopInspectionEvent;

        private IReadOnlyList<ItemWheelSlotUI> _wheelSlots;
        private IWieldableInventory _selection;
        private Vector2 _cursorDirection;
        private int _highlightedSlotIndex = -1;
        private float _inspectTimer = 1f;


        public bool IsInspecting { get; private set; }

        private SelectableUI HighlightedSelectable =>
            _highlightedSlotIndex != -1 ? _wheelSlots[_highlightedSlotIndex].Selectable : null;

        public void StartInspection()
        {
            if (IsInspecting || _inspectTimer > Time.time)
                return;

            _panel.Show();

            _cursorDirection = Vector2.zero;

            InputManager.Instance.PushEscapeCallback(EndInspection);
            InputManager.Instance.PushContext(_itemWheelContext);
            PostProcessingManager.Instance.TryPlayAnimation(this, _inspectEffect);

            IsInspecting = true;
            _startInspectionEvent.Invoke();
        }

        public void EndInspectionAndSelectHighlighted()
        {
            if (!IsInspecting)
                return;

            HighlightedSelectable.Select();
            EndInspection();
        }

        public void EndInspection()
        {
            if (!IsInspecting)
                return;

            var highlightedSlot = _wheelSlots[_highlightedSlotIndex];
            highlightedSlot.Selectable.OnPointerExit(null);
            UpdateInfo(highlightedSlot.Item);

            IsInspecting = false;
            _inspectTimer = Time.time + _cooldown;
            _panel.Hide();

            InputManager.Instance.PopEscapeCallback(EndInspection);
            InputManager.Instance.PopContext(_itemWheelContext);

            PostProcessingManager.Instance.CancelAnimation(this, _inspectEffect);

            _stopInspectionEvent.Invoke();
        }

        public void UpdateSelection(Vector2 input)
        {
            if (!IsInspecting)
                return;

            int highlightedSlot = GetHighlightedSlot(input);

            if (highlightedSlot != _highlightedSlotIndex && highlightedSlot != -1)
                HandleSlotHighlighting(highlightedSlot);
        }
        
        protected override void OnCharacterAttached(ICharacter character)
        {
            _selection = character.GetCC<IWieldableInventory>();
            _slotsGroup.SelectedChanged += OnSelectableSelected;
            _panel.PanelToggled += OnPanelToggled;
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            _slotsGroup.SelectedChanged -= OnSelectableSelected;
            _panel.PanelToggled -= OnPanelToggled;
        }

        protected override void Awake()
        {
            base.Awake();
            _wheelSlots = GetComponentsInChildren<ItemWheelSlotUI>();
        }

        private void OnPanelToggled(bool isVisible)
        {
            if (!isVisible)
                return;
            
            int selectedIndex = _selection.SelectedIndex == -1
                ? Mathf.Max(_selection.PreviousIndex, 0)
                : _selection.SelectedIndex;

            HandleSlotHighlighting(selectedIndex);
            _slotsGroup.SelectSelectable(HighlightedSelectable);
            _slotsGroup.HighlightSelectable(HighlightedSelectable);
        }

        private int GetHighlightedSlot(Vector2 input)
        {
            var directionOfSelection = new Vector2(input.x, input.y).normalized * _range;

            if (directionOfSelection != Vector2.zero)
                _cursorDirection = Vector2.Lerp(_cursorDirection, directionOfSelection, Time.deltaTime * _sensitivity);

            float angle = -Vector2.SignedAngle(Vector2.up, _cursorDirection);

            if (angle < 0)
                angle = 360f - Mathf.Abs(angle);

            angle = 360f - angle;

            for (int i = 0; i < _wheelSlots.Count; i++)
            {
                Vector2 angleCoverage = _wheelSlots[i].AngleCoverage;

                if (angleCoverage.x < angleCoverage.y)
                {
                    if (angle >= angleCoverage.x && angle <= angleCoverage.y)
                        return i;
                }
                else
                {
                    if (angle <= angleCoverage.x && angle >= angleCoverage.y)
                        return i;
                }
            }

            return -1;
        }

        private void HandleSlotHighlighting(int slotToHighlight)
        {
            if (_highlightedSlotIndex != -1)
                HighlightedSelectable.OnPointerExit(null);

            _highlightedSlotIndex = slotToHighlight;

            var highlightedSlot = _wheelSlots[slotToHighlight];
            HighlightedSelectable.OnPointerEnter(null);

            UpdateInfo(highlightedSlot.Item);
        }

        private void OnSelectableSelected(SelectableUI selectable)
        {
            var slot = selectable.GetComponent<ItemWheelSlotUI>();
            var slotIndex = _wheelSlots.IndexOf(slot);
            
            if (_highlightedSlotIndex != -1)
                _wheelSlots[_highlightedSlotIndex].Selectable.OnPointerExit(null);

            if (slotIndex != -1)
            {
                _highlightedSlotIndex = slotIndex;
                _selection.SelectAtIndex(slotIndex);
                UpdateInfo(slot != null ? slot.Item : null);
            }
        }

        private void UpdateInfo(IItem item)
        {
            _infoRoot.SetActive(item != null);
            _nameInfo.UpdateInfo(item);
            _descriptionInfo.UpdateInfo(item);
            _weightInfo.UpdateInfo(item);
            _firemodeInfo.UpdateInfo(item);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_slotsRoot == null)
            {
                var container = GetComponentInChildren<ItemContainerUI>();
                _slotsRoot = container != null ? container.transform : transform;
            }
        }
#endif
    }
}
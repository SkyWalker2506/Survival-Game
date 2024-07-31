using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace PolymindGames.UserInterface
{
    [SelectionBase]
    [DisallowMultipleComponent]
    [AddComponentMenu("Polymind Games/User Interface/Selectables/Selectable")]
    public class SelectableUI : UIBehaviour, IMoveHandler, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler
    {
        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("Can this object be selected?")]
        private bool _isSelectable = true;

        [SerializeField, BeginGroup, EndGroup]
        private NavigationUI _navigation = NavigationUI.DefaultNavigation;

        [SerializeField, NestedBehaviourListInLine(ListStyle = ListStyle.Boxed), NewLabel("Feedbacks")]
        protected SelectableFeedbackUI[] _feedbacks = Array.Empty<SelectableFeedbackUI>();

        [SerializeField, BeginGroup, EndGroup]
        private UnityEvent _onSelected;

        private readonly List<CanvasGroup> _canvasGroupCache = new();
        private SelectableGroupBaseUI _parentGroup;
        private int _currentIndex = -1;
        private bool _enableCalled;
        private bool _hasParentGroup;
        
        protected bool _isInteractable = true;
        protected bool _isPointerDown;
        protected bool _isPointerInside;
        protected bool _isSelected;
        
        private static SelectableUI[] s_Selectables = new SelectableUI[10];
        private static int s_SelectableCount;


        #region Initialization
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reload()
        {
            Array.Clear(s_Selectables, 0, s_Selectables.Length);
            s_SelectableCount = 0;
        }
#endif
        
        protected SelectableUI()
        { }
        #endregion

        /// <summary>
        /// Is this object selectable. 
        /// </summary>
        public bool IsSelectable
        {
            get => _isSelectable && _isInteractable;
            set
            {
                if (value != _isSelectable)
                {
                    _isSelectable = value;
                    DoStateTransition(CurrentSelectionState, false);
                }
            }
        }

        protected SelectionState CurrentSelectionState
        {
            get
            {
                if (_isPointerDown)
                    return SelectionState.Pressed;
                if (_hasParentGroup && _parentGroup.Selected == this)
                    return SelectionState.Selected;
                if (_isPointerInside)
                    return SelectionState.Highlighted;
                return SelectionState.Normal;
            }
        }

        public bool IsSelected => _isSelected;

        public event UnityAction OnSelected
        {
            add => _onSelected.AddListener(value);
            remove => _onSelected.RemoveListener(value);
        }

        public event UnityAction<SelectableUI> OnSelectableSelected;

        /// <summary>
        /// Selects this Selectable.
        /// </summary>
        public void Select()
        {
            if (!_isSelectable || !_isInteractable)
                return;

            if (EventSystem.current != null && !EventSystem.current.alreadySelecting)
                EventSystem.current.SetSelectedGameObject(gameObject);
        }

        public void ForceSelect()
        {
            // Only select if there's a parent group.
            if (_hasParentGroup && !_isSelected)
            {
                _isSelected = true;
                _parentGroup.SelectSelectable(this);
                EvaluateAndTransitionToSelectionState();
            }

            RaiseSelectedEvent();
        }

        /// <summary>
        /// Deselects this Selectable.
        /// </summary>
        public virtual void Deselect()
        {
            _isSelected = false;
            EvaluateAndTransitionToSelectionState();
        }

        public virtual void OnSelect(BaseEventData eventData)
        {
            if (!_isInteractable)
                return;

            // Only select if there's a parent group.
            if (_hasParentGroup && !_isSelected && _isSelectable)
            {
                _isSelected = true;
                _parentGroup.SelectSelectable(this);
                EvaluateAndTransitionToSelectionState();
            }

            RaiseSelectedEvent();
        }

        /// <summary>
        /// Evaluate current state and transition to pressed state.
        /// </summary>
        public virtual void OnPointerDown(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            _isPointerDown = true;
            EvaluateAndTransitionToSelectionState();
        }

        /// <summary>
        /// Evaluate eventData and transition to appropriate state.
        /// </summary>
        public virtual void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left)
                return;

            // Selection tracking
            if (_isPointerInside && _isInteractable)
                Select();

            _isPointerDown = false;
            EvaluateAndTransitionToSelectionState();
        }

        /// <summary>
        /// Evaluate current state and transition to appropriate state.
        /// New state could be pressed or hover depending on pressed state.
        /// </summary>
        public void OnPointerEnter(PointerEventData eventData)
        {
            _isPointerInside = true;

            if (!_isSelected)
                EvaluateAndTransitionToSelectionState();

            if (_hasParentGroup)
                _parentGroup.HighlightSelectable(this);
        }

        /// <summary>
        /// Evaluate current state and transition to normal state.
        /// </summary>
        public void OnPointerExit(PointerEventData eventData)
        {
            _isPointerInside = false;

            if (!_isSelected)
                EvaluateAndTransitionToSelectionState();

            if (_hasParentGroup)
                _parentGroup.HighlightSelectable(null);
        }

        protected void RaiseSelectedEvent()
        {
            OnSelectableSelected?.Invoke(this);
            _onSelected.Invoke();
        }

        /// <summary>
        /// Finds the selectable object next to this one.
        /// </summary>
        /// <remarks>
        /// The direction is determined by a Vector3 variable.
        /// </remarks>
        /// <param name="dir">The direction in which to search for a neighbouring Selectable object.</param>
        /// <returns>The neighbouring Selectable object. Null if none found.</returns>
        private SelectableUI FindSelectable(Vector3 dir)
        {
            dir = dir.normalized;
            Vector3 localDir = Quaternion.Inverse(transform.rotation) * dir;
            Vector3 pos = transform.TransformPoint(GetPointOnRectEdge(transform as RectTransform, localDir));
            float maxScore = Mathf.NegativeInfinity;
            float maxFurthestScore = Mathf.NegativeInfinity;

            bool wantsWrapAround = _navigation.WrapAround && (_navigation.Mode == NavigationUI.NavigationMode.Vertical || _navigation.Mode == NavigationUI.NavigationMode.Horizontal);

            SelectableUI bestPick = null;
            SelectableUI bestFurthestPick = null;

            for (int i = 0; i < s_SelectableCount; ++i)
            {
                SelectableUI sel = s_Selectables[i];

                if (sel == this)
                    continue;

                if (!sel.IsSelectable || sel._navigation.Mode == NavigationUI.NavigationMode.None)
                    continue;

                if (sel._parentGroup != _parentGroup)
                {
                    if (sel._hasParentGroup && _hasParentGroup)
                    {
                        if (sel._parentGroup.RegisteredSelectables != _parentGroup.RegisteredSelectables)
                            continue;
                    }
                    else
                        continue;
                }

#if UNITY_EDITOR

                // Apart from runtime use, FindSelectable is used by custom editors to
                // draw arrows between different selectables. For scene view cameras,
                // only selectables in the same stage should be considered.
                if (Camera.current != null && !UnityEditor.SceneManagement.StageUtility.IsGameObjectRenderedByCamera(sel.gameObject, Camera.current))
                    continue;
#endif

                var selRect = sel.transform as RectTransform;
                Vector3 selCenter = selRect != null ? selRect.rect.center : Vector3.zero;
                Vector3 myVector = sel.transform.TransformPoint(selCenter) - pos;

                // Value that is the distance out along the direction.
                float dot = Vector3.Dot(dir, myVector);
 
                // If element is in wrong direction and we have wrapAround enabled check and cache it if furthest away.
                float score;
                if (wantsWrapAround && dot < 0)
                {
                    score = -dot * myVector.sqrMagnitude;

                    if (score > maxFurthestScore)
                    {
                        maxFurthestScore = score;
                        bestFurthestPick = sel;
                    }

                    continue;
                }

                // Skip elements that are in the wrong direction or which have zero distance.
                // This also ensures that the scoring formula below will not have a division by zero error.
                if (dot <= 0)
                    continue;

                // This scoring function has two priorities:
                // - Score higher for positions that are closer.
                // - Score higher for positions that are located in the right direction.
                // This scoring function combines both of these criteria.
                // It can be seen as this:
                //   Dot (dir, myVector.normalized) / myVector.magnitude
                // The first part equals 1 if the direction of myVector is the same as dir, and 0 if it's orthogonal.
                // The second part scores lower the greater the distance is by dividing by the distance.
                // The formula below is equivalent but more optimized.
                //
                // If a given score is chosen, the positions that evaluate to that score will form a circle
                // that touches pos and whose center is located along dir. A way to visualize the resulting functionality is this:
                // From the position pos, blow up a circular balloon so it grows in the direction of dir.
                // The first Selectable whose center the circular balloon touches is the one that's chosen.
                score = dot / myVector.sqrMagnitude;

                if (score > maxScore)
                {
                    maxScore = score;
                    bestPick = sel;
                }
            }

            if (wantsWrapAround && null == bestPick)
                return bestFurthestPick;

            return bestPick;
        }

        /// <summary>
        /// Find the selectable object to the left of this one.
        /// </summary>
        public SelectableUI FindSelectableOnLeft()
        {
            if (_navigation.Mode == NavigationUI.NavigationMode.Explicit)
                return _navigation.SelectOnLeft;

            if ((_navigation.Mode & NavigationUI.NavigationMode.Horizontal) != 0)
                return FindSelectable(transform.rotation * Vector3.left);

            return null;
        }

        /// <summary>
        /// Find the selectable object to the right of this one.
        /// </summary>
        public SelectableUI FindSelectableOnRight()
        {
            if (_navigation.Mode == NavigationUI.NavigationMode.Explicit)
                return _navigation.SelectOnRight;

            if ((_navigation.Mode & NavigationUI.NavigationMode.Horizontal) != 0)
                return FindSelectable(transform.rotation * Vector3.right);

            return null;
        }

        /// <summary>
        /// The Selectable object above current
        /// </summary>
        public SelectableUI FindSelectableOnUp()
        {
            if (_navigation.Mode == NavigationUI.NavigationMode.Explicit)
                return _navigation.SelectOnUp;

            if ((_navigation.Mode & NavigationUI.NavigationMode.Vertical) != 0)
                return FindSelectable(transform.rotation * Vector3.up);

            return null;
        }

        /// <summary>
        /// Find the selectable object below this one.
        /// </summary>
        public SelectableUI FindSelectableOnDown()
        {
            if (_navigation.Mode == NavigationUI.NavigationMode.Explicit)
                return _navigation.SelectOnDown;

            if ((_navigation.Mode & NavigationUI.NavigationMode.Vertical) != 0)
                return FindSelectable(transform.rotation * Vector3.down);

            return null;
        }

        /// <summary>
        /// Determine in which of the 4 move directions the next selectable object should be found.
        /// </summary>
        public void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Right:
                    Navigate(eventData, FindSelectableOnRight());
                    break;

                case MoveDirection.Up:
                    Navigate(eventData, FindSelectableOnUp());
                    break;

                case MoveDirection.Left:
                    Navigate(eventData, FindSelectableOnLeft());
                    break;

                case MoveDirection.Down:
                    Navigate(eventData, FindSelectableOnDown());
                    break;
            }
        }

        // Convenience function -- change the selection to the specified object if it's not null and happens to be active.
        private static void Navigate(AxisEventData eventData, SelectableUI sel)
        {
            if (sel != null && sel.IsActive())
                eventData.selectedObject = sel.gameObject;
        }

        /// <summary>
        /// Whether the current selectable is being pressed.
        /// </summary>
        private bool IsPressed()
        {
            if (!IsActive() || !_isInteractable)
                return false;

            return _isPointerDown;
        }

        // Change the button to the correct state
        private void EvaluateAndTransitionToSelectionState()
        {
            if (!IsActive() || !_isInteractable)
                return;

            DoStateTransition(CurrentSelectionState, false);
        }

        protected override void OnCanvasGroupChanged()
        {
            // Figure out if parent groups allow interaction
            // If no interaction is allowed... then we need
            // to not do that :)
            var groupAllowInteraction = true;
            Transform t = transform;
            while (t != null)
            {
                t.GetComponents(_canvasGroupCache);
                bool shouldBreak = false;
                for (var i = 0; i < _canvasGroupCache.Count; i++)
                {
                    // if the parent group does not allow interaction
                    // we need to break
                    if (_canvasGroupCache[i].enabled && !_canvasGroupCache[i].interactable)
                    {
                        groupAllowInteraction = false;
                        shouldBreak = true;
                    }

                    // if this is a 'fresh' group, then break
                    // as we should not consider parents
                    if (_canvasGroupCache[i].ignoreParentGroups)
                        shouldBreak = true;
                }
                if (shouldBreak)
                    break;

                t = t.parent;
            }

            if (groupAllowInteraction != _isInteractable)
            {
                _isInteractable = groupAllowInteraction;
                DoStateTransition(CurrentSelectionState, true);
            }
        }

        // Call from unity if animation properties have changed
        protected override void OnDidApplyAnimationProperties() => DoStateTransition(CurrentSelectionState, true);

        protected override void Awake()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            var parent = transform.parent;
            if (parent != null && parent.TryGetComponent(out _parentGroup))
            {
                _hasParentGroup = true;
                _parentGroup.RegisterSelectable(this);
            }
        }

        protected override void OnDestroy()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            if (_hasParentGroup)
                _parentGroup.UnregisterSelectable(this);
        }

        // Select on enable and add to the list.
        protected override void OnEnable()
        {
            //Check to avoid multiple OnEnable() calls for each selectable
            if (_enableCalled)
                return;

            if (s_SelectableCount == s_Selectables.Length)
            {
                var temp = new SelectableUI[s_Selectables.Length * 2];
                Array.Copy(s_Selectables, temp, s_Selectables.Length);
                s_Selectables = temp;
            }

            _currentIndex = s_SelectableCount;
            s_Selectables[_currentIndex] = this;
            s_SelectableCount++;
            _isPointerDown = false;

            if (_hasParentGroup && _parentGroup.Selected == this)
                OnSelect(null);

            DoStateTransition(CurrentSelectionState, true);

            _enableCalled = true;
        }

        // Remove from the list.
        protected override void OnDisable()
        {
#if UNITY_EDITOR
            if (!UnityUtils.IsPlayMode)
                return;
#endif

            //Check to avoid multiple OnDisable() calls for each selectable
            if (!_enableCalled)
                return;

            s_SelectableCount--;

            // Update the last elements index to be this index
            s_Selectables[s_SelectableCount]._currentIndex = _currentIndex;

            // Swap the last element and this element
            s_Selectables[_currentIndex] = s_Selectables[s_SelectableCount];

            // null out last element.
            s_Selectables[s_SelectableCount] = null;

            Deselect();
            InstantClearState();
            base.OnDisable();

            _enableCalled = false;
        }

        protected override void OnTransformParentChanged()
        {
            base.OnTransformParentChanged();

            // If our parenting changes figure out if we are under a new CanvasGroup.
            OnCanvasGroupChanged();
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && IsPressed())
                InstantClearState();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (_feedbacks.Length == 0)
                _feedbacks = GetComponents<SelectableFeedbackUI>();

            foreach (var feedback in _feedbacks)
                feedback.hideFlags = HideFlags.HideInInspector;

            // OnValidate can be called before OnEnable, this makes it unsafe to access other components
            // since they might not have been initialized yet.
            if (isActiveAndEnabled)
            {
                // And now go to the right state.
                DoStateTransition(CurrentSelectionState, true);
            }
        }
#endif

        /// <summary>
        /// Clear any internal state from the Selectable (used when disabling).
        /// </summary>
        private void InstantClearState()
        {
            _isPointerInside = false;
            _isPointerDown = false;
            _isSelected = false;

            DoStateTransition(SelectionState.Normal, true);
        }

        /// <summary>
        /// Transition the Selectable to the entered state.
        /// </summary>
        /// <param name="state">State to transition to</param>
        /// <param name="instant">Should the transition occur instantly.</param>
        protected void DoStateTransition(SelectionState state, bool instant)
        {
            if (!_isSelectable || !_isInteractable || !gameObject.activeInHierarchy)
            {
                foreach (var feedback in _feedbacks)
                    feedback.OnDisabled(instant);

                return;
            }

            switch (state)
            {
                case SelectionState.Normal:
                    {
                        foreach (var feedback in _feedbacks)
                            feedback.OnNormal(instant);
                        break;
                    }
                case SelectionState.Highlighted:
                    {
                        foreach (var feedback in _feedbacks)
                            feedback.OnHighlighted(instant);
                        break;
                    }
                case SelectionState.Pressed:
                    {
                        foreach (var feedback in _feedbacks)
                            feedback.OnPressed(instant);
                        break;
                    }
                case SelectionState.Selected:
                    {
                        foreach (var feedback in _feedbacks)
                            feedback.OnSelected(instant);
                        break;
                    }
                default:
                    return;
            }
        }

        private static Vector3 GetPointOnRectEdge(RectTransform rectTrs, Vector2 dir)
        {
            if (rectTrs == null)
                return Vector3.zero;
            if (dir != Vector2.zero)
                dir /= Mathf.Max(Mathf.Abs(dir.x), Mathf.Abs(dir.y));
            Rect rect = rectTrs.rect;
            dir = rect.center + Vector2.Scale(rect.size, dir * 0.5f);
            return dir;
        }

        #region Internal
        /// <summary>
        /// An enumeration of selected states of objects
        /// </summary>
        protected enum SelectionState
        {
            /// <summary>
            /// The UI object can be selected.
            /// </summary>
            Normal,

            /// <summary>
            /// The UI object is highlighted.
            /// </summary>
            Highlighted,

            /// <summary>
            /// The UI object is pressed.
            /// </summary>
            Pressed,

            /// <summary>
            /// The UI object is selected
            /// </summary>
            Selected
        }
        #endregion
    }
}
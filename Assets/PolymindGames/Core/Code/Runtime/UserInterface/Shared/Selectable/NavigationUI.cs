using System;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    /// <summary>
    /// Structure storing details related to navigation.
    /// </summary>
    [Serializable]
    public struct NavigationUI : IEquatable<NavigationUI>
    {
        /*
         * This looks like it's not flags, but it is flags,
         * the reason is that Automatic is considered horizontal
         * and vertical mode combined
         */
        [Flags]
        public enum NavigationMode
        {
            /// <summary>
            /// No navigation is allowed from this object.
            /// </summary>
            None = 0,

            /// <summary>
            /// Horizontal Navigation.
            /// </summary>
            /// <remarks>
            /// Navigation should only be allowed when left / right move events happen.
            /// </remarks>
            Horizontal = 1,

            /// <summary>
            /// Vertical navigation.
            /// </summary>
            /// <remarks>
            /// Navigation should only be allowed when up / down move events happen.
            /// </remarks>
            Vertical = 2,

            /// <summary>
            /// Automatic navigation.
            /// </summary>
            /// <remarks>
            /// Attempt to find the 'best' next object to select. This should be based on a sensible heuristic.
            /// </remarks>
            Automatic = 3,

            /// <summary>
            /// Explicit navigation.
            /// </summary>
            /// <remarks>
            /// User should explicitly specify what is selected by each move event.
            /// </remarks>
            Explicit = 4
        }

        // Which method of navigation will be used.

        [SerializeField]
        private NavigationMode _mode;

        [Tooltip("Enables navigation to wrap around from last to first or first to last element. Does not work for automatic grid navigation")]
        [SerializeField]
        private bool _wrapAround;

        // Game object selected when the joystick moves up. Used when navigation is set to "Explicit".

        [SerializeField]
        private SelectableUI _selectOnUp;

        // Game object selected when the joystick moves down. Used when navigation is set to "Explicit".

        [SerializeField]
        private SelectableUI _selectOnDown;

        // Game object selected when the joystick moves left. Used when navigation is set to "Explicit".

        [SerializeField]
        private SelectableUI _selectOnLeft;

        // Game object selected when the joystick moves right. Used when navigation is set to "Explicit".

        [SerializeField]
        private SelectableUI _selectOnRight;

        /// <summary>
        /// Navigation mode.
        /// </summary>
        public NavigationMode Mode
        {
            readonly get => _mode;
            set => _mode = value;
        }

        /// <summary>
        /// Enables navigation to wrap around from last to first or first to last element.
        /// Will find the furthest element from the current element in the opposite direction of movement.
        /// </summary>
        /// <example>
        /// Note: If you have a grid of elements and you are on the last element in a row it will not wrap over to the next row it will pick the furthest element in the opposite direction.
        /// </example>
        public bool WrapAround
        {
            readonly get => _wrapAround;
            set => _wrapAround = value;
        }

        /// <summary>
        /// Specify a Selectable UI GameObject to highlight when the Up arrow key is pressed.
        /// </summary>
        public SelectableUI SelectOnUp
        {
            readonly get => _selectOnUp;
            set => _selectOnUp = value;
        }

        /// <summary>
        /// Specify a Selectable UI GameObject to highlight when the down arrow key is pressed.
        /// </summary>
        public SelectableUI SelectOnDown
        {
            readonly get => _selectOnDown;
            set => _selectOnDown = value;
        }

        /// <summary>
        /// Specify a Selectable UI GameObject to highlight when the left arrow key is pressed.
        /// </summary>
        public SelectableUI SelectOnLeft
        {
            readonly get => _selectOnLeft;
            set => _selectOnLeft = value;
        }

        /// <summary>
        /// Specify a Selectable UI GameObject to highlight when the right arrow key is pressed.
        /// </summary>
        public SelectableUI SelectOnRight
        {
            readonly get => _selectOnRight;
            set => _selectOnRight = value;
        }

        /// <summary>
        /// Return a Navigation with sensible default values.
        /// </summary>
        public static NavigationUI DefaultNavigation
        {
            get
            {
                NavigationUI defaultNav = new NavigationUI
                {
                    _mode = NavigationMode.Automatic,
                    _wrapAround = false
                };

                return defaultNav;
            }
        }

        public readonly bool Equals(NavigationUI other)
        {
            return Mode == other.Mode &&
                   SelectOnUp == other.SelectOnUp &&
                   SelectOnDown == other.SelectOnDown &&
                   SelectOnLeft == other.SelectOnLeft &&
                   SelectOnRight == other.SelectOnRight;
        }
    }
}
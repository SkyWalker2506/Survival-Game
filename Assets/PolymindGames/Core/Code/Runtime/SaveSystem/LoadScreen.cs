using System.Collections;
using UnityEngine.Events;
using UnityEngine;

namespace PolymindGames
{
    /// <summary>
    /// Abstract class representing a load screen in the game.
    /// </summary>
    public abstract class LoadScreen : MonoBehaviour
    {
        /// <summary>
        /// Event invoked when the load screen is started.
        /// </summary>
        public event UnityAction Started;

        /// <summary>
        /// Event invoked when the load screen is destroyed.
        /// </summary>
        public event UnityAction Destroyed;

        /// <summary>
        /// Abstract method to fade in the load screen.
        /// </summary>
        /// <returns>An IEnumerator representing the fade-in process.</returns>
        public abstract IEnumerator FadeIn();

        /// <summary>
        /// Abstract method to fade out the load screen.
        /// </summary>
        public abstract void FadeOut();

        /// <summary>
        /// Abstract method to hide the save icon on the load screen.
        /// </summary>
        public abstract void HideSaveIcon();

        /// <summary>
        /// Abstract method to show the load icon on the load screen.
        /// </summary>
        public abstract void ShowLoadIcon();

        protected virtual void Start() => Started?.Invoke();
        protected virtual void OnDestroy() => Destroyed?.Invoke();
    }

}
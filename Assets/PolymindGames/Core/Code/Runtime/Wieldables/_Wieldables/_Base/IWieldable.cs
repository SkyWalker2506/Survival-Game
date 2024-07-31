using System.Collections;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public interface IWieldable : IMonoBehaviour
    {
        ICharacter Character { get; }
        IWieldableMotion Motion { get; }
        IAnimator Animation { get; }
        IAudioPlayer AudioPlayer { get; }
        bool IsGeometryVisible { get; set; }

        /// <summary>
        /// Sets the character/wielder of this wieldable.
        /// </summary>
        /// <param name="character">Character to set</param>
        void SetCharacter(ICharacter character);

        /// <summary>
        /// Initiates the equipping process for this wieldable.
        /// </summary>
        IEnumerator Equip();

        /// <summary>
        /// Initiates the holstering process for this wieldable at a specified speed.
        /// </summary>
        /// <param name="holsterSpeed">Speed of holstering</param>
        IEnumerator Holster(float holsterSpeed);
    }

    public sealed class NullWieldable : IWieldable
    {
        public ICharacter Character => null;
        public IWieldableMotion Motion => null;
        public IAnimator Animation => null;
        public IAudioPlayer AudioPlayer => null;
        public GameObject gameObject => null;
        public Transform transform => null;
        public bool enabled { get => true; set { } }
        public bool IsGeometryVisible { get => false; set { } }
        
        public void SetCharacter(ICharacter character) { }
        public IEnumerator Equip() { yield break; }
        public IEnumerator Holster(float holsterSpeed) { yield break; }
    }
}
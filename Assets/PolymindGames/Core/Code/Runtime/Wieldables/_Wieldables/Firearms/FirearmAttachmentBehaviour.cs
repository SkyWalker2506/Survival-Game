using System;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public abstract class FirearmAttachmentBehaviour : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        [Tooltip("Indicates whether this attachment is currently attached to a firearm.")]
        private bool _isAttached;

        private EnableMode _enableMode = EnableMode.None;
        
        public bool IsAttached => _isAttached;
        protected IWieldable Wieldable { get; private set; }
        protected IFirearm Firearm { get; private set; }
        
        public void Attach()
        {
            if (_isAttached)
                return;
            
            if (_enableMode == EnableMode.None)
                Initialize();

            switch (_enableMode)
            {
                case EnableMode.Component:
                    _isAttached = true;
                    enabled = true;
                    break;
                case EnableMode.GameObject:
                    _isAttached = true;
                    gameObject.SetActive(true);
                    enabled = true;
                    break;
                case EnableMode.None:
                    return;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void Detach()
        {
            if (!_isAttached)
                return;

            if (_enableMode == EnableMode.None)
                Initialize();

            switch (_enableMode)
            {
                case EnableMode.Component:
                    _isAttached = false;
                    enabled = false;
                    break;
                case EnableMode.GameObject:
                    _isAttached = false;
                    gameObject.SetActive(false);
                    enabled = false;
                    break;
                case EnableMode.None:
                    return;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual void Awake()
        {
            if (_enableMode == EnableMode.None)
                Initialize();
            
            if (!_isAttached)
                Detach();
        }

        private void Initialize()
        {
            Wieldable = GetComponentInParent<IWieldable>();

            if (Wieldable == null)
                _enableMode = EnableMode.None;
            else
            {
                _enableMode = Wieldable.gameObject == gameObject
                    ? EnableMode.Component
                    : EnableMode.GameObject;
                
                if (Wieldable is IFirearm firearm)
                    Firearm = firearm;
            }
        }

        #region Internal
        private enum EnableMode
        {
            None,
            Component,
            GameObject
        }
        #endregion
    }
}
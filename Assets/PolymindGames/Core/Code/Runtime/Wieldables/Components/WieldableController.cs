using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

namespace PolymindGames.WieldableSystem
{
    /// <summary>
    /// Controller responsible for equipping and holstering wieldables.
    /// </summary>
    [DefaultExecutionOrder(ExecutionOrderConstants.BEFORE_DEFAULT_2)]
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/player/modules-and-behaviours/wieldable#wieldables-controller-module")]
    public sealed class WieldableController : CharacterBehaviour, IWieldableControllerCC
    {
        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("The parent of the spawned wieldables.")]
        private Transform _spawnRoot;

        [DisableInPlayMode, BeginGroup, EndGroup]
        [SerializeField, Help("The wieldable that will be equipped when equipping a NULL wieldable. (Tip: you can set it to an arms/unarmed wieldable).")]
        private Wieldable _defaultWieldable;

        private readonly List<IWieldable> _registeredWieldables = new();
        private readonly List<WieldableEntry> _equipStack = new();
        private WieldableControllerState _state;
        private Coroutine _updateCoroutine;
        private IWieldable _activeWieldable;
        private IWieldable _nullWieldable;
        private int _activeWieldableId;
        private float _holsterSpeed = 1f;

        private static readonly NullWieldable s_NullWieldable = new();
        
        private const float MIN_HOLSTER_SPEED = 0.25f;
        private const float MAX_HOLSTER_SPEED = 5f;

        
        WieldableControllerState IWieldableControllerCC.State => _state;
        public Transform WieldableParent => _spawnRoot != null ? _spawnRoot : transform;
        public IWieldable ActiveWieldable => _activeWieldable == s_NullWieldable ? null : _activeWieldable;
        private WieldableEntry LastWieldableInEquipStack => _equipStack[^1];

        public event WieldableEquipDelegate HolsteringStarted;
        public event WieldableEquipDelegate HolsteringStopped;
        public event WieldableEquipDelegate EquippingStarted;
        public event WieldableEquipDelegate EquippingStopped;

        public IWieldable RegisterWieldable(IWieldable wieldable)
        {
            if (wieldable == null || _registeredWieldables.Contains(wieldable))
                return wieldable;

            // If the wieldable is a prefab, instantiate it first.
            if (wieldable.gameObject != null)
            {
                if (wieldable.gameObject.IsPrefab())
                {
                    var trs = transform;
                    var position = trs.position;
                    var rotation = trs.rotation;
                    wieldable = Instantiate(wieldable.gameObject, position, rotation, WieldableParent)
                        .GetComponent<IWieldable>();
                }
                else
                    wieldable.transform.SetParent(WieldableParent);
            }

            _registeredWieldables.Add(wieldable);
            wieldable.SetCharacter(Character);

            return wieldable;
        }

        public void UnregisterWieldable(IWieldable wieldable, bool destroy = false)
        {
            if (!IsRegistered(wieldable))
                return;

            if (_activeWieldable == wieldable)
                TryEquipWieldable(null, 1.35f);

            if (destroy)
                Destroy(wieldable.gameObject);
        }

        public bool TryEquipWieldable(IWieldable wieldable, float holsterSpeedMod = 1, UnityAction equipCallback = null)
        {
            wieldable ??= _nullWieldable;

            if (IsRegistered(wieldable))
            {
                int indexOfEntry = _equipStack.Count > 1
                    ? _equipStack.FindIndex(1, entry => entry.Wieldable == wieldable)
                    : -1;

                // Case 1: Wieldable not in the equip stack.
                if (indexOfEntry == -1)
                    _equipStack.Add(new WieldableEntry(wieldable, equipCallback, GenerateUniqueId(wieldable)));

                // Case 2: Wieldable already equipped or will be equipped.
                else if (indexOfEntry == _equipStack.Count - 1)
                    return false;

                // Case 3: Move the wieldable to the top of the equip stack.
                else
                {
                    _equipStack.RemoveAt(indexOfEntry);
                    _equipStack.Add(new WieldableEntry(wieldable, equipCallback, GenerateUniqueId(wieldable)));
                }

                _holsterSpeed = Mathf.Clamp(holsterSpeedMod, MIN_HOLSTER_SPEED, MAX_HOLSTER_SPEED);

                UpdateActiveWieldable();

                return true;
            }

            return false;
        }

        public bool TryHolsterWieldable(IWieldable wieldable, float holsterSpeedMod = 1f)
        {
            wieldable ??= _nullWieldable;

            int indexOfWieldable = _equipStack.FindIndex(1, entry => entry.Wieldable == wieldable);

            if (indexOfWieldable > 0)
            {
                _equipStack.RemoveAt(indexOfWieldable);

                if (indexOfWieldable == _equipStack.Count)
                {
                    _holsterSpeed = Mathf.Clamp(holsterSpeedMod, MIN_HOLSTER_SPEED, MAX_HOLSTER_SPEED);
                    UpdateActiveWieldable();
                }

                return true;
            }

            return false;
        }

        public void HolsterAll()
        {
            if (_equipStack.Count > 1)
            {
                _equipStack.RemoveRange(1, _equipStack.Count - 2);
                UpdateActiveWieldable();
            }
        }

        protected override void OnBehaviourStart(ICharacter character)
        {
            _nullWieldable = _defaultWieldable != null
                ? RegisterWieldable(_defaultWieldable)
                : RegisterWieldable(s_NullWieldable);

#if !UNITY_EDITOR
            _defaultWieldable = null;
#endif

            TryEquipWieldable(null);
        }

        private int GenerateUniqueId(IWieldable wieldable)
        {
            return wieldable != _nullWieldable ? Random.Range(int.MinValue, int.MaxValue) : 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateActiveWieldable() => _updateCoroutine ??= StartCoroutine(C_Update());

        private IEnumerator C_Update()
        {
            do
            {
                // Holster the active wieldable if it doesn't match the top element of the stack.
                if (_activeWieldable != null && LastWieldableInEquipStack.UniqueId != _activeWieldableId)
                {
                    _state = WieldableControllerState.Holstering;

                    HolsteringStarted?.Invoke(_activeWieldable);
                    yield return _activeWieldable.Holster(_holsterSpeed);
                    HolsteringStopped?.Invoke(_activeWieldable);

                    _activeWieldable = null;
                    _activeWieldableId = 0;
                }

                // Equip the top element of the stack if it isn't already.
                if (LastWieldableInEquipStack.Wieldable != _activeWieldable)
                {
                    _state = WieldableControllerState.Equipping;

                    var entry = LastWieldableInEquipStack;
                    _activeWieldable = entry.Wieldable;
                    _activeWieldableId = entry.UniqueId;

                    EquippingStarted?.Invoke(_activeWieldable);

                    if (entry.EquipCallback != null)
                        CoroutineUtils.InvokeNextFrame(this, entry.EquipCallback);

                    yield return _activeWieldable.Equip();

                    EquippingStopped?.Invoke(_activeWieldable);
                }

            } while (LastWieldableInEquipStack.UniqueId != _activeWieldableId);

            _state = WieldableControllerState.None;
            _updateCoroutine = null;
        }

        private bool IsRegistered(IWieldable wieldable)
        {
            if (_registeredWieldables.Contains(wieldable))
                return true;

            string wieldableName = wieldable != null
                ? wieldable.gameObject != null ? wieldable.gameObject.name : wieldable.ToString()
                : "Null";

            Debug.LogError($"The wieldable: ''{wieldableName}'' is not registered.");

            return false;
        }

        #region Internal
        private readonly struct WieldableEntry
        {
            public readonly int UniqueId;
            public readonly IWieldable Wieldable;
            public readonly UnityAction EquipCallback;


            public WieldableEntry(IWieldable wieldable, UnityAction equipCallback, int id)
            {
                Wieldable = wieldable;
                EquipCallback = equipCallback;
                UniqueId = id;
            }
        }
        #endregion
    }
}
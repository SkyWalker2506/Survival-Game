using System;
using System.Collections.Generic;
using PolymindGames.InventorySystem;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace PolymindGames.UserInterface
{
    public sealed class ItemContainerUI : MonoBehaviour
    {
        [SerializeField, PrefabObjectOnly]
        private ItemSlotUI _slotTemplate;
        
        private IItemContainer _container;
        private List<ItemSlotUI> _slots;


        public IItemContainer Container => _container;
        
        public void AttachToContainer(IItemContainer container)
        {
            if (container == null)
            {
                Debug.LogWarning($"Cannot attach a null container to ''{gameObject.name}''");
                return;
            }

            _container = container;
            _slots ??= new List<ItemSlotUI>(container.Capacity);
            GenerateSlots(container.Capacity);

            for (int i = 0; i < container.Capacity; i++)
                _slots![i].SetItemSlot(container[i]);

            container.CapacityChanged += OnCapacityChanged;
        }

        public void DetachFromContainer()
        {
            if (_container == null)
                return;

            for (int i = 0; i < _slots.Count; i++)
                _slots[i].SetItemSlot(null);

            _container.CapacityChanged -= OnCapacityChanged;
        }

        private void OnDestroy() => DetachFromContainer();

        public void GenerateSlots(int count)
        {
            if (count == _slots.Count)
                return;

            if (count is < 0 or > 128)
                throw new IndexOutOfRangeException();

            // Get the children slots.
            transform.GetComponentsInFirstChildren(_slots);

            if (count < _slots.Count)
            {
                // Hide or delete the extra slots.
                for (int i = count; i < _slots.Count; i++)
                {
#if UNITY_EDITOR
                    if (!Application.isPlaying)
                        DestroyImmediate(_slots[i].gameObject);
                    else
                        _slots[i].gameObject.SetActive(false);
#else
					_slots[i].gameObject.SetActive(false);
#endif
                }

                // Make sure all of the remaining active slots are visible.
                for (int i = 0; i < count; i++)
                    _slots[i].gameObject.SetActive(true);

                // Shrink the slots list so that it matches the new count.
                _slots.RemoveRange(count - 1, _slots.Count - count);
                return;
            }

            int amountToActivate = _slots.Count < count ? _slots.Count : count;
            for (int i = 0; i < amountToActivate; i++)
                _slots[i].gameObject.SetActive(true);

            if (count > _slots.Count)
            {
                if (_slotTemplate == null)
                {
                    Debug.LogError("No slot template is provided, can't generate any slots.", gameObject);
                    return;
                }

                int slotsToCreateCount = count - _slots.Count;

#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    EditorUtility.SetDirty(this);
                    for (int i = 0; i < slotsToCreateCount; i++)
                    {
                        ItemSlotUI slot = PrefabUtility.InstantiatePrefab(_slotTemplate, transform) as ItemSlotUI;
                        slot.gameObject.SetActive(true);
                        slot.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                        EditorUtility.SetDirty(slot);
                    }

                    return;
                }
#endif

                _slots.Capacity = count;
                for (int i = 0; i < slotsToCreateCount; i++)
                {
                    ItemSlotUI slotInterface = Instantiate(_slotTemplate, transform);
                    slotInterface.gameObject.SetActive(true);
                    slotInterface.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    _slots.Add(slotInterface);
                }
            }
        }

        private void OnCapacityChanged(int prevCapacity, int newCapacity)
        {
            GenerateSlots(newCapacity);
            for (int i = 0; i < _container.Capacity; i++)
                _slots[i].SetItemSlot(_container[i]);
        }
    }
}
using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    public abstract class WieldableItemBehaviour : MonoBehaviour
    {
        protected IWieldableItem WieldableItem { get; private set; }

        protected virtual void Awake()
        {
            WieldableItem = GetComponentInParent<IWieldableItem>();

            if (WieldableItem == null)
            {
                Debug.LogError($"No wieldable item found for {GetType().Name}!", gameObject);
                return;
            }

            WieldableItem.AttachedItemChanged += OnItemChanged;
        }

        protected virtual void OnItemChanged(IItem item) { }
    }
}
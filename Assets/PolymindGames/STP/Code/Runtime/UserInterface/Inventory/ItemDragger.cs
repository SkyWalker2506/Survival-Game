using UnityEngine;

namespace PolymindGames.UserInterface
{
    [DefaultExecutionOrder(ExecutionOrderConstants.SCENE_SINGLETON)]
    public abstract class ItemDragger : MonoBehaviour
    {
        public bool IsDragging { get; protected set; }


        public static ItemDragger Instance { get; private set; }
        
        public abstract void OnDragStart(ItemSlotUI startSlot, Vector2 pointerPosition, bool splitItemStack = false);
        public abstract void CancelDrag(ItemSlotUI initialSlot);
        public abstract void OnDrag(Vector2 pointerPosition);
        public abstract void OnDragEnd(ItemSlotUI startSlot, ItemSlotUI dropSlot, GameObject dropObject);

        protected virtual void Awake()
        {
            // Ensure only one instance of ItemDragger exists
            if (Instance == null)
                Instance = this;
        }

        protected void OnDestroy()
        {
            // Clear singleton instance when destroyed
            if (Instance == this)
                Instance = null;
        }
    }
}
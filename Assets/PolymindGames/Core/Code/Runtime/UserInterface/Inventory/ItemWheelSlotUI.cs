using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [AddComponentMenu("Polymind Games/User Interface/Slots/Item Wheel Slot")]
    public sealed class ItemWheelSlotUI : ItemSlotUI
    {
        [SerializeField, IgnoreParent, BeginGroup]
        private ItemPropertyTextInfo _propertyTextInfo;

        [SerializeField, Range(-360f, 360f), SpaceArea]
        private float _angleStart;

        [SerializeField, Range(-360f, 360f), EndGroup]
        private float _angleEnd;
        
        
        public Vector2 AngleCoverage => new(_angleStart, _angleEnd);

        protected override void UpdateInfo(IItem item)
        {
            IconInfo.UpdateInfo(item);
            PropertyFillBarInfo.UpdateInfo(item);
            _propertyTextInfo.UpdateInfo(item);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_angleStart < 0 || _angleEnd < 0)
                Gizmos.color = Color.red;
            else
                Gizmos.color = Color.blue;

            var pos = transform.parent.position;
            Gizmos.DrawRay(pos, Quaternion.Euler(0, 0, _angleStart) * Vector3.up * 150);
            Gizmos.DrawRay(pos, Quaternion.Euler(0, 0, _angleEnd) * Vector3.up * 150);
        }
#endif
    }
}
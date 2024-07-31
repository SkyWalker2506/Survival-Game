using PolymindGames.InventorySystem;
using UnityEngine;

namespace PolymindGames.WieldableSystem
{
    [AddComponentMenu("Polymind Games/Wieldables/Behaviours/Wieldable Compass Rose")]
    public sealed class WieldableCompassRose : WieldableItemBehaviour
    {
        [SerializeField, BeginGroup]
        [Tooltip("The Transform component representing the compass rose.")]
        private Transform _compassRose;

        [SerializeField]
        [Tooltip("The axis around which the compass rose will rotate.")]
        private Vector3 _rotationAxis = new Vector3(0, 0, 1);

        [SerializeField, Range(0f, 100f), EndGroup]
        [Tooltip("The speed at which the compass rose rotates around its axis.")]
        private float _rotationSpeed = 3f;

        [SerializeField, BeginGroup, EndGroup]
        [Tooltip("The item property definition used to track durability.")]
        private DataIdReference<ItemPropertyDefinition> _durabilityProperty;

        private ItemProperty _durability;
        private Transform _root;
        private float _angle;


        protected override void OnItemChanged(IItem item)
        {
            if (item != null)
            {
                _durability = item.GetPropertyWithId(_durabilityProperty);
                _root = WieldableItem.Wieldable.Character.transform;
            }
            else
            {
                _durability = null;
            }
        }

        private void LateUpdate()
        {
            if (_durability == null || _durability.Float < 0.01f)
                return;

            _angle = UpdateRoseAngle(_angle, _rotationSpeed * Time.deltaTime);
            _compassRose.localRotation = Quaternion.AngleAxis(_angle, _rotationAxis);
        }

        private float UpdateRoseAngle(float angle, float delta)
        {
            angle = Mathf.LerpAngle(angle, Vector3.SignedAngle(_root.forward, Vector3.forward, Vector3.up), delta);
            angle = Mathf.Repeat(angle, 360f);

            return angle;
        }
    }
}
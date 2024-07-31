using UnityEngine;

namespace PolymindGames
{
    public sealed class SnapToGround : MonoBehaviour
    {
        [SerializeField, NotNull, BeginGroup]
        private Transform _root;

        [SerializeField, Range(0f, 1000f)]
        private float _disableDuration = 20f;

        [SerializeField, Range(0f, 100f)]
        private float _targetDistance = 0.25f;

        [SerializeField, Range(0f, 100f), EndGroup]
        private float _maxDistance = 0.3f;
        
        private float _disableTimer;
        private Vector3 _position;


        private void OnEnable()
        {
            if (_root == null)
                _root = transform.root;

            _position = _root.position;
            _disableTimer = Time.time + _disableDuration;
        }

        private void LateUpdate()
        {
            Ray ray = new(_root.position + Vector3.up * 0.1f, Vector3.down);

            Vector3 newPosition;
            if (PhysicsUtils.RaycastOptimized(ray, _maxDistance, out var hitInfo, LayerConstants.ALL_SOLID_OBJECTS_MASK, _root))
                newPosition = new Vector3(_root.transform.position.x, hitInfo.point.y + _targetDistance, _root.transform.position.z);
            else
                newPosition = _position;

            transform.position = newPosition;
            _position = newPosition;

            if (Time.time > _disableTimer)
                enabled = false;
        }
    }
}
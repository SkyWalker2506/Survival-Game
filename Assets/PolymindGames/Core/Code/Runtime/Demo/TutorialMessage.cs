using UnityEngine;
using TMPro;

namespace PolymindGames.Demo
{
    public sealed class TutorialMessage : MonoBehaviour
    {
        [SerializeField, Range(0f, 10f), BeginGroup]
        private float _transitionSpeed = 3f;

        [SerializeField, Range(0f, 100f), EndGroup]
        private float _maxDistance = 15f;

        private TextMeshPro[] _textMeshes;
        private Color[] _textMeshColors;


        private void Start()
        {
            _textMeshes = GetComponentsInChildren<TextMeshPro>();
            _textMeshColors = new Color[_textMeshes.Length];

            for (int i = 0; i < _textMeshes.Length; i++)
                _textMeshColors[i] = _textMeshes[i].color;
        }

        private void LateUpdate()
        {
            if (UnityUtils.CachedMainCamera == null)
                return;

            float angle = Vector3.Angle(gameObject.transform.forward, UnityUtils.CachedMainCamera.transform.forward);
            float distance = Vector3.Distance(UnityUtils.CachedMainCamera.transform.position, transform.position);

            UpdateTextAlpha(angle < 90 && distance < _maxDistance);
        }

        private void UpdateTextAlpha(bool enable)
        {
            for (int i = 0; i < _textMeshes.Length; i++)
                _textMeshes[i].CrossFadeAlpha(enable ? 1f : 0f, Time.deltaTime * _transitionSpeed, true);
        }

#if UNITY_EDITOR
        private void Reset()
        {
            if (_textMeshes == null || _textMeshes.Length == 0)
                _textMeshes = GetComponentsInChildren<TextMeshPro>();
        }
#endif
    }
}
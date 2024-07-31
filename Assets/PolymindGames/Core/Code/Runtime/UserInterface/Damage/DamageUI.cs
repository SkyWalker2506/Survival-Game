using System.Collections;
using UnityEngine;

namespace PolymindGames.UserInterface
{
    [HelpURL("https://polymindgames.gitbook.io/welcome-to-gitbook/qgUktTCVlUDA7CAODZfe/user-interface/behaviours/ui_damage#damage-ui")]
    public sealed class DamageUI : CharacterUIBehaviour
    {
        [SerializeField, BeginGroup("Blood Screen")]
        [Tooltip("Image fading settings for the blood screen.")]
        private ImageFaderUI _bloodScreenFader;

        [SerializeField, Range(0f, 100f), EndGroup]
        [Tooltip("How much damage does the player have to take for the blood screen effect to show. ")]
        private float _bloodScreenDamageThreshold = 5f;

        [SerializeField, BeginGroup("Directional Indicator")]
        [Tooltip("Image fading settings for the directional damage indicator.")]
        private ImageFaderUI _directionalIndicatorFader;

        [SerializeField, Range(0f, 1024), EndGroup]
        [Tooltip("Damage indicator distance (in pixels) from the screen center.")]
        private int _damageIndicatorDistance = 128;
        
        private RectTransform _damageIndicatorRT;
        private Coroutine _indicatorRoutine;
        private Vector3 _lastHitPoint;


        protected override void Awake()
        {
            base.Awake();
            _damageIndicatorRT = _directionalIndicatorFader.Image.rectTransform;
        }

        protected override void OnCharacterAttached(ICharacter character)
        {
            character.HealthManager.DamageReceived += OnTakeDamage;
            
        }

        protected override void OnCharacterDetached(ICharacter character)
        {
            character.HealthManager.DamageReceived -= OnTakeDamage;
        }

        private void OnTakeDamage(float damage, in DamageArgs dmgInfo)
        {
            var health = Character.HealthManager;
            float targetAlpha = (health.PrevHealth - health.Health) / 100f;

            if (damage > 1f)
            {
                Vector3 lastHitPoint = dmgInfo.HitPoint;
                if (lastHitPoint != Vector3.zero)
                {
                    _lastHitPoint = lastHitPoint;
                    _directionalIndicatorFader.DoFadeCycle(this, targetAlpha);

                    CoroutineUtils.StartAndReplaceCoroutine(this, C_UpdateDirectionalSpriteDirection(), ref _indicatorRoutine);
                }

                if (damage > _bloodScreenDamageThreshold)
                    _bloodScreenFader.DoFadeCycle(this, targetAlpha);
            }
        }

        private void DoBloodScreen()
        {
            
        }

        private IEnumerator C_UpdateDirectionalSpriteDirection()
        {
            var headTransform = Character.GetTransformOfBodyPoint(BodyPoint.Head);
            while (_directionalIndicatorFader.Fading)
            {
                Vector3 lookDir = Vector3.ProjectOnPlane(headTransform.forward, Vector3.up).normalized;
                Vector3 dirToPoint = Vector3.ProjectOnPlane(_lastHitPoint - Character.transform.position, Vector3.up).normalized;

                Vector3 rightDir = Vector3.Cross(lookDir, Vector3.up);

                float angle = Vector3.Angle(lookDir, dirToPoint) * Mathf.Sign(Vector3.Dot(rightDir, dirToPoint));

                _damageIndicatorRT.localEulerAngles = Vector3.forward * angle;
                _damageIndicatorRT.localPosition = _damageIndicatorRT.up * _damageIndicatorDistance;

                yield return null;
            }

            _indicatorRoutine = null;
        }
    }
}
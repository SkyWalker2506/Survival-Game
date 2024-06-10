using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace HQFPSTemplate
{
    public class PostProcessingManager_HDRP : MonoBehaviour
    {
        [SerializeField]
        private Volume m_GlobalVolume;

        [BHeader("DepthOfField", true)]

        [SerializeField]
        private bool m_EnableAimDOF = true;

        [SerializeField]
        [EnableIf("m_EnableAimDOF", true)]
        private Vector2 m_AimDOFStrengthRange = new Vector2(0.1f, 0.5f);

        [SerializeField]
        private bool m_EnableItemWheelDOF = true;

        [SerializeField]
        [EnableIf("m_EnableItemWheelDOF", true)]
        private Vector2 m_ItemWheelDOFStrengthRange = new Vector2(5f, 10f);

        [BHeader("DeathAnim", true)]

        [SerializeField]
        [Range(0.01f, 15f)]
        private float m_DeathAnimSpeed = 1f;

        [SerializeField]
        [Range(-1,0)]
        private float m_MinColorSaturation = -50f;

        private float m_DefaultSaturation;
        private bool m_PlayerDead;

        private ColorAdjustments m_ColorAdjComponent;
        private DepthOfField m_DOFComponent;

        private Player m_Player;
        private UserInterface.UIManager m_UIManager;


        private void EnableDOFNearRange(bool enable)
        {
            Vector2 dofValue = enable ? m_AimDOFStrengthRange : Vector2.zero;

            m_DOFComponent.nearFocusStart.SetValue(new MinFloatParameter(dofValue.x, 0f, true));
            m_DOFComponent.nearFocusEnd.SetValue(new MinFloatParameter(dofValue.y, 0f, true));
        }

        private void EnableDOFFarRange(bool enable)
        {
            Vector2 dofValue = enable ? m_ItemWheelDOFStrengthRange : new Vector2(1000f, 1000f);

            m_DOFComponent.farFocusStart.SetValue(new MinFloatParameter(dofValue.x, 0f, true));
            m_DOFComponent.farFocusEnd.SetValue(new MinFloatParameter(dofValue.y, 0f, true));
        }

        private void DoDeathAnim() 
        {
            m_PlayerDead = true;

            StartCoroutine(C_DoDeathAnim());
        }

        private void RestoreDefaultProfile() 
        {
            m_PlayerDead = false;
        }

        private void Start()
        {
            if (m_GlobalVolume.profile.TryGet(out DepthOfField tempDOF))
                m_DOFComponent = tempDOF;

            if (m_GlobalVolume.profile.TryGet(out ColorAdjustments tempColorAdj))
                m_ColorAdjComponent = tempColorAdj;

            m_Player = GameManager.Instance.CurrentPlayer;
            m_UIManager = GameManager.Instance.CurrentInterface;

            if (m_Player != null)
            {
                m_Player.Death.AddListener(DoDeathAnim);
                m_Player.Respawn.AddListener(RestoreDefaultProfile);

                if (m_EnableAimDOF)
                {
                    m_Player.Aim.AddStartListener(() => EnableDOFNearRange(m_Player.ActiveEquipmentItem.Get().EInfo.Aiming.UseAimBlur));
                    m_Player.Aim.AddStopListener(() => EnableDOFNearRange(false));
                }
            }

            if (m_UIManager != null)
            {
                if (m_EnableItemWheelDOF)
                {
                    m_UIManager.ItemWheel.AddStartListener(() => EnableDOFFarRange(true));
                    m_UIManager.ItemWheel.AddStopListener(() => EnableDOFFarRange(false));
                }
            }
        }

        private IEnumerator C_DoDeathAnim() 
        {
            float saturation = m_ColorAdjComponent.saturation.value;
            float requiredSaturation = m_MinColorSaturation * 100;

            while (m_PlayerDead) 
            {
                saturation = Mathf.Lerp(saturation, requiredSaturation, Time.deltaTime * m_DeathAnimSpeed);

                m_ColorAdjComponent.saturation.SetValue(new ClampedFloatParameter(saturation, -1f, 1f, true));

                yield return null;
            }

            if(!m_PlayerDead)
                m_ColorAdjComponent.saturation.SetValue(new ClampedFloatParameter(m_DefaultSaturation, -1f, 1f, true));
        }
    }
}
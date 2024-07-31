using PolymindGames;
using PolymindGames.WieldableSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace PolymindGamesEditor.WieldableSystem
{
    [CustomEditor(typeof(WieldableAnimator), true)]
    public sealed class WieldableAnimatorEditor : ObjectEditor
    {
        private WieldableAnimator _wieldableAnimator;


        public override void DrawCustomInspector()
        {
            if (_wieldableAnimator.Animator == null)
                EditorGUILayout.HelpBox("Assign an animator controller", UnityEditor.MessageType.Error);

            base.DrawCustomInspector();

            if (_wieldableAnimator.Animator != null)
            {
                using (new EditorGUI.DisabledScope(Application.isPlaying))
                {
                    EditorGUILayout.Space();

                    if (CanShowSetupAnimatorButton())
                        DrawSetupGUI();
                }
            }
        }

        private void OnEnable() => _wieldableAnimator = (WieldableAnimator)target;

        private void DrawSetupGUI()
        {
            EditorGUILayout.Space();
            GuiLayout.Separator();

            GUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.HelpBox("Animator is not properly set up", UnityEditor.MessageType.Warning);

            if (GUILayout.Button("Fix First Person Settings"))
            {
                FixModelAndAnimator();

                EditorUtility.SetDirty(_wieldableAnimator.gameObject);
                PrefabUtility.RecordPrefabInstancePropertyModifications(_wieldableAnimator.gameObject);
            }

            GUILayout.EndVertical();

            GuiLayout.Separator();
        }

        private bool CanShowSetupAnimatorButton()
        {
            if (_wieldableAnimator == null)
                return false;

            var animator = _wieldableAnimator.Animator;

            bool canShow = animator.GetComponentInChildren<SkinnedMeshRenderer>(true).motionVectorGenerationMode != MotionVectorGenerationMode.ForceNoMotion;
            canShow |= animator.cullingMode != AnimatorCullingMode.AlwaysAnimate;
            canShow |= animator.gameObject.layer != LayerConstants.VIEW_MODEL;

            foreach (var renderer in animator.GetComponentsInChildren<MeshRenderer>())
            {
                canShow |= renderer.gameObject.layer != LayerConstants.VIEW_MODEL;
                canShow |= renderer.shadowCastingMode != ShadowCastingMode.Off;

                if (canShow)
                    return true;
            }

            return canShow;
        }

        private void FixModelAndAnimator()
        {
            var animator = _wieldableAnimator.Animator;
            var skinnedRenderers = animator.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            var renderers = animator.GetComponentsInChildren<MeshRenderer>(true);

            if (animator != null)
            {
                _wieldableAnimator.gameObject.SetLayersInChildren(LayerConstants.VIEW_MODEL);

                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                animator.updateMode = AnimatorUpdateMode.Normal;
                animator.applyRootMotion = false;
            }

            if (skinnedRenderers != null)
            {
                foreach (var skinRenderer in skinnedRenderers)
                {
                    skinRenderer.updateWhenOffscreen = true;
                    skinRenderer.shadowCastingMode = ShadowCastingMode.Off;
                    skinRenderer.skinnedMotionVectors = false;
                    skinRenderer.allowOcclusionWhenDynamic = false;
                    skinRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                }
            }

            if (renderers != null)
            {
                foreach (var renderer in renderers)
                {
                    renderer.shadowCastingMode = ShadowCastingMode.Off;
                    renderer.allowOcclusionWhenDynamic = false;
                }
            }
        }
    }
}
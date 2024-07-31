using UnityEngine;

#if POLYMIND_GAMES_FPS_HDRP
using VolumeComponent = UnityEngine.Rendering.HighDefinition.LensDistortion;
#else
using VolumeComponent = UnityEngine.Rendering.PostProcessing.LensDistortion;
#endif

namespace PolymindGames.PostProcessing
{
    [NestedObjectPath(MenuName = "Distortion Animation", FileName = "DistortionAnimation")]
    public sealed class DistortionAnimation : VolumeAnimation<VolumeComponent>
    {
        [SerializeField]
        private VolumeParameterAnimation<float> _intensity = new(0f, 0f);
            
            
        protected override void AddAnimations(VolumeParameterAnimationCollection list, VolumeComponent component)
        {
            list.Add(_intensity.SetParameter(component.intensity));
        }
    }
}

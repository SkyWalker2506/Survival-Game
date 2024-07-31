using UnityEngine;

#if POLYMIND_GAMES_FPS_HDRP
using VolumeComponent = UnityEngine.Rendering.HighDefinition.ColorAdjustments;
#else
using VolumeComponent = UnityEngine.Rendering.PostProcessing.ColorGrading;
#endif

namespace PolymindGames.PostProcessing
{
    [NestedObjectPath(MenuName = "Saturation Animation", FileName = "SaturationAnimation")]
    public sealed class SaturationAnimation : VolumeAnimation<VolumeComponent>
    {
        [SerializeField]
        private VolumeParameterAnimation<float> _saturation = new(0f, 0f);
        
        
        protected override void AddAnimations(VolumeParameterAnimationCollection list, VolumeComponent component)
        {
            list.Add(_saturation.SetParameter(component.saturation));
        }
    }
}

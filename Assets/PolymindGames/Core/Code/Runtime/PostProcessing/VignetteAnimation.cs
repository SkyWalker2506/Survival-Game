using UnityEngine;

#if POLYMIND_GAMES_FPS_HDRP
using VolumeComponent = UnityEngine.Rendering.HighDefinition.Vignette;
#else
using VolumeComponent = UnityEngine.Rendering.PostProcessing.Vignette;
#endif

namespace PolymindGames.PostProcessing
{
    [NestedObjectPath(MenuName = "Vignette Animation", FileName = "VignetteAnimation")]
    public sealed class VignetteAnimation : VolumeAnimation<VolumeComponent>
    {
        [SerializeField]
        private VolumeParameterAnimation<float> _intensity = new(0f, 0f);
        
        
        protected override void AddAnimations(VolumeParameterAnimationCollection list, VolumeComponent component)
        {
            list.Add(_intensity.SetParameter(component.intensity));
        }
    }
}

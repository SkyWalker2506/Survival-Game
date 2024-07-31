using UnityEngine;

#if POLYMIND_GAMES_FPS_HDRP
using VolumeComponent = UnityEngine.Rendering.HighDefinition.ChromaticAberration;
#else
using VolumeComponent = UnityEngine.Rendering.PostProcessing.ChromaticAberration;
#endif

namespace PolymindGames.PostProcessing
{
    [NestedObjectPath(MenuName = "Chromatic Aberration Animation", FileName = "ChromaticAberrationAnimation")]
    public sealed class ChromaticAnimation : VolumeAnimation<VolumeComponent>
    {
        [SerializeField]
        private VolumeParameterAnimation<float> _intensity = new(0f, 0f);
        
        
        protected override void AddAnimations(VolumeParameterAnimationCollection list, VolumeComponent component)
        {
            list.Add(_intensity.SetParameter(component.intensity));
        }
    }
}
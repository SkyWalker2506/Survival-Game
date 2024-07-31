using UnityEngine;

#if POLYMIND_GAMES_FPS_HDRP || POLYMIND_GAMES_FPS_URP
using Volume = UnityEngine.Rendering.Volume;
#else
using Volume = UnityEngine.Rendering.PostProcessing.PostProcessVolume;
#endif

namespace PolymindGames.PostProcessing
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Volume))]
    [AddComponentMenu("Polymind Games/Miscellaneous/Global Volume")]
    public sealed class GlobalVolume : MonoBehaviour
    {
        private void OnEnable() => PostProcessingManager.Instance.RegisterGlobalProfile(GetComponent<Volume>().profile);
        private void OnDisable() => PostProcessingManager.Instance.UnregisterGlobalProfile(GetComponent<Volume>().profile);
        private void OnValidate() => UnityUtils.SafeOnValidate(this, () => gameObject.GetOrAddComponent<Volume>().isGlobal = true);
    }
}


using System.Collections.Generic;

using UnityEngine;

using Raymarcher.Toolkit.Experimental;
using Raymarcher.Objects.Volumes;
using Raymarcher.Utilities;
using Raymarcher.Attributes;

namespace Raymarcher.ExampleContent
{
    using static RMVolumeUtils;
    using static RMAttributes;

    [System.Obsolete("Highly experimental feature! It may be removed in the future, and using it is at your own risk.")]
    public sealed class RMSample_VolumeColliderSampler : MonoBehaviour
    {
        [Space]
        [SerializeField] private RMSdf_Tex3DVolumeBox targetVolumeBoxTex3D;
        [SerializeField] private CommonVolumeResolution targetCanvasResolution = CommonVolumeResolution.x32;
        [SerializeField, Range(0f, 1f)] private float pixelThreshold = 0.2f;
        [SerializeField] private bool buildSphereColliders = true;
        [SerializeField] private bool buildOnStart = false;
        [Space]
        [SerializeField] private List<Transform> generatedColliders = new List<Transform>();
#if UNITY_EDITOR
        [Space]
        [SerializeField, Button("Build Colliders In Editor", "BuildColliders")] private int TEMP_BUTTON0;
        [SerializeField, Button("Clear Colliders In Editor", "ClearColliders")] private int TEMP_BUTTON1;
#endif
        private void Start()
        {
            if (buildOnStart)
                BuildColliders();
        }

        [System.Obsolete("Highly experimental feature! It may be removed in the future, and using it is at your own risk.")]
        public void BuildColliders()
        {
            ClearColliders();
            if (targetVolumeBoxTex3D.VolumeTexture == null)
            {
                RMDebug.Debug(this, "Target volume 3D Texture is null!", true);
                return;
            }

            RenderTexture tempRT = targetVolumeBoxTex3D.VolumeTexture as RenderTexture;
            bool createdTemporary = false;
            if(tempRT == null)
            {
                Texture3D tex3D = targetVolumeBoxTex3D.VolumeTexture as Texture3D;
                if(tex3D == null)
                {
                    RMDebug.Debug(this, "Target volume 3D Texture is not a 3D texture!", true);
                    return;
                }
                tempRT = RMTextureUtils.ConvertTexture3DToRenderTexture3D(tex3D);
                createdTemporary = true;
            }
            RMVolumePointCollisionSampler.GeneratePrimitiveCollidersOnVolumePoints(ref generatedColliders, transform,
                targetVolumeBoxTex3D,
                tempRT,
                targetCanvasResolution, 1, pixelThreshold, buildSphereColliders);

            if (createdTemporary)
                tempRT.Release();
        }

        [System.Obsolete("Highly experimental feature! It may be removed in the future, and using it is at your own risk.")]
        public void ClearColliders()
        {
            if (generatedColliders != null && generatedColliders.Count > 0)
            {
                for (int i = generatedColliders.Count - 1; i >= 0; i--)
                {
                    if (generatedColliders[i] == null)
                        continue;
                    if(Application.isPlaying)
                        Destroy(generatedColliders[i].gameObject);
                    else
                        DestroyImmediate(generatedColliders[i].gameObject);
                }
                generatedColliders.Clear();
            }
        }
    }
}
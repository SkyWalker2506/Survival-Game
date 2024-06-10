using UnityEngine;

using Raymarcher.Toolkit;
using Raymarcher.Attributes;
using Raymarcher.Utilities;
using Raymarcher.Objects.Volumes;

namespace Raymarcher.ExampleContent
{
    using static RMAttributes;
    using static RMVolumeUtils;

    public sealed class RMSample_VolumeBrushLocator : MonoBehaviour
    {
        [Space]
        [SerializeField] private bool autoInitialize = false;
        [SerializeField, ShowIf("autoInitialize", 1)] private CommonVolumeResolution targetVolumeTexResolution = CommonVolumeResolution.x64;
        [SerializeField, ShowIf("autoInitialize", 1)] private RMSdf_Tex3DVolumeBox targetVolumeBox;
        [Header("Locator Settings")]
        [SerializeField, Required] private Transform rayOriginTransform;
        [SerializeField, Required] private Transform targetBrush;
        [SerializeField, Range(1f, 128f)] private float targetBrushSmoothFollow = 8f;
        [Header("Depth Sampler Settings")]
        [SerializeField, Range(0.5f, 64), Tooltip("The longer the ray, the more depth iterations are recommended")] private float rayLength = 3;
        [SerializeField, Range(8, 128)] private int depthIterations = 64;
        [SerializeField, Range(0, 1)] private float pixelThreshold = 0.05f;

        private RMVolumeDepthSampler volumeDepthSampler;
        private float lastDepth = 2;

        private void OnDrawGizmos()
        {
            if (rayOriginTransform == null)
                return;

            Gizmos.color = Color.green;
            Gizmos.DrawRay(rayOriginTransform.position, rayOriginTransform.forward * rayLength);
        }

        private void Start()
        {
            // Automatic initialization for target volume box

            if (!autoInitialize)
                return;

            if(!targetVolumeBox)
            {
                Debug.LogError("Target volume box is null!");
                return;
            }

            if(targetVolumeBox.VolumeTexture == null)
            {
                Debug.LogError("Target volume box has no texture!");
                return;
            }

            RenderTexture rt;
            if (targetVolumeBox.VolumeTexture is RenderTexture rtrt)
                rt = rtrt;
            else
                rt = RMTextureUtils.ConvertTexture3DToRenderTexture3D((Texture3D)targetVolumeBox.VolumeTexture);

            volumeDepthSampler = new RMVolumeDepthSampler(rt, depthIterations, targetVolumeTexResolution, pixelThreshold);
        }

        public void Initialize(RMVolumeVoxelPainter existingVoxelPainter)
        {
            // Manual/external initialization based on an existing volume voxel painter

            var targetVolumePainter = existingVoxelPainter;
            if (targetVolumePainter == null)
            {
                Debug.LogError("Target volume painter is null!");
                return;
            }
            if(!targetVolumePainter.IsInitialized)
            {
                Debug.LogError("Target Volume Voxel Painter is not initialized!");
                return;
            }
            if (volumeDepthSampler != null && volumeDepthSampler.Initialized)
                volumeDepthSampler.Dispose();

            targetVolumeBox = targetVolumePainter.TargetTex3DVolumeBox;
            volumeDepthSampler = new RMVolumeDepthSampler(targetVolumePainter, depthIterations, pixelThreshold);
        }

        private void OnDestroy()
        {
            // It is required to call this to avoid leaks!
            volumeDepthSampler?.Dispose();
        }

        private void Update()
        {
            if (rayOriginTransform == null || targetBrush == null || !volumeDepthSampler.Initialized || targetVolumeBox == null)
                return;

            if (!volumeDepthSampler.SampleVolumeDepth(out Vector3 pos, rayOriginTransform.position, rayOriginTransform.forward, targetVolumeBox, rayLength))
                pos = rayOriginTransform.position + rayOriginTransform.forward * lastDepth;
            else
                lastDepth = Vector3.Distance(rayOriginTransform.position, pos);
            targetBrush.position = Vector3.Lerp(targetBrush.position, pos, Time.deltaTime * targetBrushSmoothFollow);
        }

        /// <summary>
        /// Returns true whether the depth sampler hits a voxel. Outputs the hit position in world space
        /// </summary>
        public bool SampleVolumeDepthPoint(out Vector3 hitPos, Vector3 rayOrigin, Vector3 rayDirection, float rayLength)
        {
            hitPos = default;
            if (!volumeDepthSampler.Initialized || targetVolumeBox == null)
                return false;
            return volumeDepthSampler.SampleVolumeDepth(out hitPos, rayOrigin, rayDirection, targetVolumeBox, rayLength);
        }
        
        /// <summary>
        /// Returns a voxel value (x) and a voxel material value (y)
        /// </summary>
        public Vector2 SampleVolumePixel(Vector3 worldSpacePosition)
        {
            if (!volumeDepthSampler.Initialized || targetVolumeBox == null)
                return Vector2.zero;
            volumeDepthSampler.SampleVolumePixel(out Vector2 pixel, worldSpacePosition, targetVolumeBox);
            return pixel;
        }
    }
}
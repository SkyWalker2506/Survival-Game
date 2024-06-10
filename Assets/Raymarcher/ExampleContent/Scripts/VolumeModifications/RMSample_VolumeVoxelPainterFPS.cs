using UnityEngine;

using Raymarcher.Toolkit;
using Raymarcher.Attributes;
using Raymarcher.Objects.Modifiers;
using Raymarcher.Objects.Volumes;
using Raymarcher.Utilities;

namespace Raymarcher.ExampleContent
{
    using static RMAttributes;
    using static RMVolumeUtils;

    public sealed class RMSample_VolumeVoxelPainterFPS : MonoBehaviour
    {
        public enum PaintingType { PaintVoxels, PaintMaterialOnly };

        // Serialized & public

        [Header("Essentials")]
        [SerializeField, Required] private RMSdf_Tex3DVolumeBox targetTex3DVolumeBox;
        [SerializeField] private CommonVolumeResolution volumeCanvasResolution = CommonVolumeResolution.x64;
        [SerializeField] private Texture3D initialVolumeCanvasTexture;
        [Space]
        public PaintingType paintingType;

        [Header("Addons")]
        [SerializeField] private RMSample_VolumeBrushLocator volumeBrushLocator;
        [SerializeField] private RMVolumeCharacterController volumeCharacterController;

        [Header("Input (Legacy)")]
        [SerializeField] private KeyCode key_ToPaint = KeyCode.Mouse0;
        [SerializeField] private KeyCode key_ToErase = KeyCode.Mouse1;
        [Header("Brush Settings")]
        [Required] public Transform brushTransform;
        public float brushSize = 1;
        [SerializeField, Range(0f, 1f)] private float brushIntensity = 0.5f;
        [SerializeField, Range(0f, 1f)] private float brushSmoothness = 1;
        [Header("Material Settings")]
        [Range(0, 8)] public int selectedMaterialIndex = 0;
        [Range(0, 8)] public int maxMaterialInstanceIndex = 0;
        [SerializeField] private RMModifier_VolumeMaterialCompositor materialCompositor;

        public RMVolumeVoxelPainter VolumeVoxelPainter => volumeVoxelPainter;

        // Privates

        private RMVolumeVoxelPainter volumeVoxelPainter;

        private RMVolumeVoxelPainter.MaterialData CreateMaterialData =>
            new RMVolumeVoxelPainter.MaterialData(selectedMaterialIndex, materialCompositor == null ? 1 : materialCompositor.MaterialFamilyTotalCount,
            new RMVolumeVoxelPainter.VoxelException(maxMaterialInstanceIndex));

        private void Start()
        {
            // Target volume box is very required!
            if(targetTex3DVolumeBox == null)
            {
                Debug.LogError("Target Tex3D Volume Box is null!");
                enabled = false;
                return;
            }

            // Allocate a new volume voxel painter object that will handle voxels
            volumeVoxelPainter = new RMVolumeVoxelPainter(volumeCanvasResolution, targetTex3DVolumeBox, initialVolumeCanvasTexture);

            // Initialize addons - they will share the same data so it is not needed to allocate new voxel painters/depth samplers in individual classes...
            if (volumeBrushLocator != null)
                volumeBrushLocator.Initialize(volumeVoxelPainter);
            if(volumeCharacterController != null)
                volumeCharacterController.InitializeCharacter(volumeVoxelPainter);
        }

        private void OnDestroy()
        {
            // This is very required!
            volumeVoxelPainter?.Dispose();
        }

        private void Update()
        {
            if (brushTransform == null)
            {
                Debug.LogError($"'{nameof(brushTransform)}' is null!");
                return;
            }
            if(paintingType != PaintingType.PaintVoxels && materialCompositor == null)
            {
                Debug.LogError($"'{nameof(materialCompositor)}' is null!");
                return;
            }

            if (Input.GetKey(key_ToPaint))
                ProcessPaint(false);
            else if (Input.GetKey(key_ToErase))
                ProcessPaint(true);
        }

        private void ProcessPaint(bool erase)
        {
            switch (paintingType)
            {
                // Process painting based on the brush variables

                case PaintingType.PaintVoxels:
                    volumeVoxelPainter.PaintVoxel(brushTransform.position, brushSize + (erase ? 0.25f : 0), brushIntensity, brushSmoothness, erase, CreateMaterialData);
                    break;
                case PaintingType.PaintMaterialOnly:
                    volumeVoxelPainter.PaintMaterialOnly(brushTransform.position, CreateMaterialData, brushSize, brushSmoothness);
                    break;
            }
        }
    }
}
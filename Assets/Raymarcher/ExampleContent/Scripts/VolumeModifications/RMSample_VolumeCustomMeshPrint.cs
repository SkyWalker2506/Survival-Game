using UnityEngine;

using Raymarcher.Attributes;
using Raymarcher.Toolkit;

namespace Raymarcher.ExampleContent
{
    using static RMAttributes;

    public sealed class RMSample_VolumeCustomMeshPrint : MonoBehaviour
    {
        [SerializeField, Required] private Transform targetMeshToPrint;
        [SerializeField, Required] private RMVolumeMeshPrinter targetVolumeMeshPrinter;
        [SerializeField, Required] private RMSample_VolumeVoxelPainterFPS targetVoxelFPSPainter;
        [SerializeField] private KeyCode keyToPrint = KeyCode.E;

        private void Update()
        {
            if(Input.GetKeyDown(keyToPrint))
            {
                if (!targetMeshToPrint)
                {
                    Debug.LogError("Target Mesh To Print is null! Can't print...");
                    return;
                }
                if (!targetVolumeMeshPrinter)
                {
                    Debug.LogError("Target Volume Mesh Printer is null! Can't print...");
                    return;
                }
                if (!targetVoxelFPSPainter)
                {
                    Debug.LogError("Target Voxel FPS Painter is null! Can't print...");
                    return;
                }

                // We need to update the printer in case the 'targetMeshToPrint' has changed
                targetVolumeMeshPrinter.UpdateMeshPrinter(targetVoxelFPSPainter.VolumeVoxelPainter, targetMeshToPrint);
                // Print the target mesh as 'Additive' to add to voxels
                targetVolumeMeshPrinter.PrintTargetMeshesToTex3DVolume(true);
            }
        }
    }
}
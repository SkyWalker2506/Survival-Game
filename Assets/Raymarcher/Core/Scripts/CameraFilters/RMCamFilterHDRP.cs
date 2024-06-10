using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

using Raymarcher.Constants;

namespace Raymarcher.CameraFilters
{
    using static RMCamFilterUtils;

    public sealed class RMCamFilterHDRP : CustomPass
    {
        [Header("RM Camera Filter - HDRP Settings")]
        public Material rmSessionMaterial;
        public bool renderInSceneView = true;
        public float projectorSize = 0;

        private readonly int[] TRIANGLE_INDICES = new int[6] { 0, 1, 2, 0, 2, 3 };

        protected override bool executeInSceneView => renderInSceneView;

        protected override void Execute(CustomPassContext ctx)
        {
            if (rmSessionMaterial != null)
            {
                Camera cam = ctx.hdCamera.camera;
                Vector3[] outCorners = CalculateFrustum(cam);
                rmSessionMaterial.SetMatrix(RMConstants.CommonRendererProperties.CamSpaceToWorldMatrix, cam.cameraToWorldMatrix);
                AdjustFrustumToProjector(ref outCorners, projectorSize);
                ctx.cmd.DrawMesh(new Mesh() { vertices = outCorners, triangles = TRIANGLE_INDICES }, cam.cameraToWorldMatrix, rmSessionMaterial);
            }
        }
    }
}
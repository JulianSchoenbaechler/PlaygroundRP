#if UNITY_EDITOR
using System.Collections;
using UnityEditor;

namespace JulianSchoenbaechler.Rendering.PlaygroundRP
{
    internal static class SceneViewDrawMode
    {
        /// <summary>
        /// Setup editor scene view draw mode validate delegates.
        /// </summary>
        public static void SetupDrawMode()
        {
            ArrayList sceneViewArray = SceneView.sceneViews;

            foreach(SceneView sceneView in sceneViewArray)
                sceneView.onValidateCameraMode += RejectDrawMode;
        }

        /// <summary>
        /// Reset editor scene view draw mode validate delegates.
        /// </summary>
        public static void ResetDrawMode()
        {
            ArrayList sceneViewArray = SceneView.sceneViews;

            foreach(SceneView sceneView in sceneViewArray)
                sceneView.onValidateCameraMode -= RejectDrawMode;
        }

        /// <summary>
        /// Reject unsupported scene view draw modes.
        /// </summary>
        /// <param name="cameraMode">The scene view mode to check.</param>
        /// <returns><c>true</c> if draw mode should be rejected; otherwise <c>false</c>.</returns>
        private static bool RejectDrawMode(SceneView.CameraMode cameraMode)
        {
            return !(cameraMode.drawMode == DrawCameraMode.TexturedWire ||
                    cameraMode.drawMode == DrawCameraMode.ShadowCascades ||
                    cameraMode.drawMode == DrawCameraMode.RenderPaths ||
                    cameraMode.drawMode == DrawCameraMode.AlphaChannel ||
                    cameraMode.drawMode == DrawCameraMode.Overdraw ||
                    cameraMode.drawMode == DrawCameraMode.Mipmaps ||
                    cameraMode.drawMode == DrawCameraMode.SpriteMask ||
                    cameraMode.drawMode == DrawCameraMode.DeferredDiffuse ||
                    cameraMode.drawMode == DrawCameraMode.DeferredSpecular ||
                    cameraMode.drawMode == DrawCameraMode.DeferredSmoothness ||
                    cameraMode.drawMode == DrawCameraMode.DeferredNormal ||
                    cameraMode.drawMode == DrawCameraMode.ValidateAlbedo ||
                    cameraMode.drawMode == DrawCameraMode.ValidateMetalSpecular ||
                    cameraMode.drawMode == DrawCameraMode.ShadowMasks ||
                    cameraMode.drawMode == DrawCameraMode.LightOverlap);
        }
    }
}
#endif

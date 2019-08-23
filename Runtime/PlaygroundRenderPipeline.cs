using UnityEngine;
using UnityEngine.Rendering;

namespace JulianSchoenbaechler.Rendering.PlaygroundRP
{
    public sealed class PlaygroundRenderPipeline : RenderPipeline
    {
        public const string ShaderTagName = "PlaygroundPipeline";

        private const string SetRenderTargetTag = "Set RenderTarget";
        private const string SetCameraRenderStateTag = "Clear Render State";
        private const string ReleaseResourcesTag = "Release Resources";

        private PlaygroundRenderPipelineAsset pipelineAsset;
        private ShaderTagId basePassId = new ShaderTagId("BasePass");
        private bool isStereoSupported = false;

        private string cachedCameraTag;

        /// <summary>
        /// Initializes an instance of the <see cref="PlaygroundRenderPipeline"/> class.
        /// </summary>
        /// <param name="asset">The render pipeline asset.</param>
        public PlaygroundRenderPipeline(PlaygroundRenderPipelineAsset pipelineAsset)
        {
            this.pipelineAsset = pipelineAsset;

            SetSupportedRenderingFeatures();

#if UNITY_EDITOR
            SceneViewDrawMode.ResetDrawMode();
#endif

            RenderingUtils.ClearSystemInfoCache();

            Shader.globalRenderPipeline = ShaderTagName;
        }

        /// <summary>
        /// Defines custom rendering for this <see cref="RenderPipeline"/>.
        /// </summary>
        /// <param name="context">The render context.</param>
        /// <param name="cameras">All the cameras to render.</param>
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            FilteringSettings opaqueFilteringSettings = new FilteringSettings(RenderQueueRange.opaque);
            FilteringSettings transparentFilteringSettings = new FilteringSettings(RenderQueueRange.transparent);

            GraphicsSettings.useScriptableRenderPipelineBatching = pipelineAsset.UseSRPBatcher;

            // Camera render loop
            foreach(Camera camera in cameras)
            {
#if UNITY_EDITOR
                cachedCameraTag = camera.name;
#else
                cachedCameraTag = "Render Camera";
#endif

#if UNITY_EDITOR
                if(camera.cameraType == CameraType.SceneView)
                    ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
#endif

                // Culling. Adjust culling parameters for your needs. One could enable/disable
                // per-object lighting or control shadow caster distance.
                camera.TryGetCullingParameters(isStereoSupported, out var cullingParameters);
                var cullingResults = context.Cull(ref cullingParameters);

                SortingSettings opaqueSortingSettings = new SortingSettings(camera);
                opaqueSortingSettings.criteria = SortingCriteria.CommonOpaque;

                // ShaderTagId must match the "LightMode" tag inside the shader pass.
                // If not "LightMode" tag is found the object won't render.
                DrawingSettings opaqueDrawingSettings = new DrawingSettings(basePassId, opaqueSortingSettings);
                opaqueDrawingSettings.enableDynamicBatching = pipelineAsset.EnableDynamicBatching;
                opaqueDrawingSettings.enableInstancing = pipelineAsset.EnableInstancing;
                opaqueDrawingSettings.perObjectData = PerObjectData.None;

                // Helper method to setup some per-camera shader constants and camera matrices
                context.SetupCameraProperties(camera, isStereoSupported);

                // Sets active render target and clear based on camera backgroud color
                CommandBuffer cmd = CommandBufferPool.Get(cachedCameraTag);

                using(new ProfilingSample(cmd, SetRenderTargetTag))
                {
                    cmd.SetRenderTarget(BuiltinRenderTextureType.CurrentActive);
                    cmd.ClearRenderTarget(true, true, camera.backgroundColor);
                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();
                }

                // Render Opaque objects given the filtering and settings computed above
                // This functions will sort and batch objects
                context.DrawRenderers(cullingResults, ref opaqueDrawingSettings, ref opaqueFilteringSettings);

                // Render remaining objects that do not match the shader passes with the Unity's error shader
                RenderingUtils.RenderObjectsWithError(context, ref cullingResults, camera, opaqueFilteringSettings, SortingCriteria.None);

                // Renders skybox if required
                if(camera.clearFlags == CameraClearFlags.Skybox && RenderSettings.skybox != null)
                    context.DrawSkybox(camera);

#if UNITY_EDITOR
                if(UnityEditor.Handles.ShouldRenderGizmos())
                    context.DrawGizmos(camera, GizmoSubset.PreImageEffects);
#endif

#if UNITY_EDITOR
                if(UnityEditor.Handles.ShouldRenderGizmos())
                    context.DrawGizmos(camera, GizmoSubset.PostImageEffects);
#endif

                context.ExecuteCommandBuffer(cmd);
                CommandBufferPool.Release(cmd);

                // Submit commands to GPU. Up to this point all commands have been enqueued in the context.
                // Several submits can be done in a frame to better controls CPU/GPU workload.
                context.Submit();
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">From finalizer?</param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            Shader.globalRenderPipeline = "";
            SupportedRenderingFeatures.active = new SupportedRenderingFeatures();

#if UNITY_EDITOR
            SceneViewDrawMode.ResetDrawMode();
#endif
        }

        /// <summary>
        /// Set supported render features of this pipeline.
        /// </summary>
        private void SetSupportedRenderingFeatures()
        {
#if UNITY_EDITOR
            SupportedRenderingFeatures.active = new SupportedRenderingFeatures()
            {
                reflectionProbeModes = SupportedRenderingFeatures.ReflectionProbeModes.None,
                defaultMixedLightingModes = SupportedRenderingFeatures.LightmapMixedBakeModes.Subtractive,
                mixedLightingModes = SupportedRenderingFeatures.LightmapMixedBakeModes.Subtractive | SupportedRenderingFeatures.LightmapMixedBakeModes.IndirectOnly,
                lightmapBakeTypes = LightmapBakeType.Baked | LightmapBakeType.Mixed,
                lightmapsModes = LightmapsMode.CombinedDirectional | LightmapsMode.NonDirectional,
                lightProbeProxyVolumes = false,
                motionVectors = false,
                receiveShadows = true,
                reflectionProbes = true
            };

            SceneViewDrawMode.SetupDrawMode();
#endif
        }
    }
}

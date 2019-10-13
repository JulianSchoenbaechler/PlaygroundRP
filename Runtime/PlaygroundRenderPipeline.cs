using Unity.Collections;
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

        private const int MaxVisibleLights = 16;

        private static int visibleLightsCount = Shader.PropertyToID("_VisibleLightsCount");
        private static int visibleLightDirectionsId = Shader.PropertyToID("_VisibleLightDirections");
        private static int visibleLightColorsId = Shader.PropertyToID("_VisibleLightColors");
        private static int visibleLightAttenuationsId = Shader.PropertyToID("_VisibleLightAttenuations");
        private static int visibleLightSpotDirectionsId = Shader.PropertyToID("_VisibleLightSpotDirections");

        private PlaygroundRenderPipelineAsset pipelineAsset;
        private ShaderTagId basePassId = new ShaderTagId("BasePass");
        private bool isStereoSupported = false;

        private Vector4[] visibleLightColors = new Vector4[MaxVisibleLights];
        private Vector4[] visibleLightDirections = new Vector4[MaxVisibleLights];
        private Vector4[] visibleLightAttenuations = new Vector4[MaxVisibleLights];
        private Vector4[] visibleLightSpotDirections = new Vector4[MaxVisibleLights];

        private string cachedCameraTag;

        /// <summary>
        /// Gets the maximum shadow bias that can be applied.
        /// </summary>
        /// <value>Maximum shadow bias.</value>
        public static float MaxShadowBias => 10f;

        /// <summary>
        /// Gets the minimum render scale that can be applied.
        /// </summary>
        /// <value>Minimum render scale factor.</value>
        public static float MinRenderScale => 0.1f;

        /// <summary>
        /// Gets the maximum render scale that can be applied.
        /// </summary>
        /// <value>Maximum render scale factor.</value>
        public static float MaxRenderScale => 2.0f;

        /// <summary>
        /// Gets the amount of lights that can be shaded per object (in the for loop in the shader).
        /// No support to bitfield mask and int[] on gles2. Can't index fast more than 4 lights.
        /// Check Lighting.hlsl for more details.
        /// </summary>
        /// <value>Maximum per object lights.</value>
        public static int MaxPerObjectLights => (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLES2) ? 4 : 8;

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

            GraphicsSettings.lightsUseLinearIntensity = false;
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

            int lightsPerObjectLimit = pipelineAsset.LightsPerObjectLimit;
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

                // Helper method to setup some per-camera shader constants and camera matrices
                context.SetupCameraProperties(camera, isStereoSupported);

                // Configure lighting
                PerObjectData lightingPOD;
                ConfigureLights(ref cullingResults, out lightingPOD);

                CommandBuffer cmd = CommandBufferPool.Get(cachedCameraTag);

                using(new ProfilingSample(cmd, cachedCameraTag))
                {
                    // Setup visible lights
                    SetupLightConstants(context, (lightingPOD == PerObjectData.None) ? 0 : lightsPerObjectLimit);

                    SortingSettings opaqueSortingSettings = new SortingSettings(camera);
                    opaqueSortingSettings.criteria = SortingCriteria.CommonOpaque;

                    // ShaderTagId must match the "LightMode" tag inside the shader pass.
                    // If not "LightMode" tag is found the object won't render.
                    DrawingSettings opaqueDrawingSettings = new DrawingSettings(basePassId, opaqueSortingSettings);
                    opaqueDrawingSettings.enableDynamicBatching = pipelineAsset.EnableDynamicBatching;
                    opaqueDrawingSettings.enableInstancing = pipelineAsset.EnableInstancing;
                    opaqueDrawingSettings.perObjectData = lightingPOD;


                    context.ExecuteCommandBuffer(cmd);
                    cmd.Clear();

                    // Get new command buffer from pool for Profiling distinction
                    CommandBuffer cmdSetup = CommandBufferPool.Get(SetRenderTargetTag);

                    // Sets active render target and clear based on camera backgroud color
                    cmdSetup.SetRenderTarget(BuiltinRenderTextureType.CurrentActive);
                    cmdSetup.ClearRenderTarget(true, true, camera.backgroundColor);

                    context.ExecuteCommandBuffer(cmdSetup);
                    CommandBufferPool.Release(cmdSetup);

                    // Render Opaque objects given the filtering and settings computed above
                    // This functions will sort and batch objects
                    context.DrawRenderers(cullingResults, ref opaqueDrawingSettings, ref opaqueFilteringSettings);

                    // Render remaining objects that do not match the shader passes with the Unity's error shader
                    RenderingUtils.RenderObjectsWithError(context, ref cullingResults, camera, opaqueFilteringSettings, SortingCriteria.None);
                }

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

        private void SetupLightConstants(ScriptableRenderContext context, int lightsPerObjectLimit)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Setup Light Constants");

            cmd.SetGlobalVector(visibleLightsCount, new Vector4(Mathf.Min(MaxVisibleLights, lightsPerObjectLimit), 0.0f, 0.0f, 0.0f));
            cmd.SetGlobalVectorArray(visibleLightDirectionsId, visibleLightDirections);
            cmd.SetGlobalVectorArray(visibleLightColorsId, visibleLightColors);
            cmd.SetGlobalVectorArray(visibleLightAttenuationsId, visibleLightAttenuations);
            cmd.SetGlobalVectorArray(visibleLightSpotDirectionsId, visibleLightSpotDirections);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void ConfigureLights(ref CullingResults cullingResults, out PerObjectData lightingPOD)
        {
            var visibleLights = cullingResults.visibleLights;
            int count = Mathf.Min(visibleLights.Length, MaxVisibleLights);
            int i = 0;

            // No visible lights in scene
            if(count == 0)
            {
                lightingPOD = PerObjectData.None;
                return;
            }

            // When light limit gets exceeded
            if(visibleLights.Length > MaxVisibleLights)
            {
                var lightIndices = cullingResults.GetLightIndexMap(Allocator.Temp);

                for(i = MaxVisibleLights; i < visibleLights.Length; i++)
                    lightIndices[i] = -1;

                cullingResults.SetLightIndexMap(lightIndices);
                lightIndices.Dispose();
            }

            // Iterate through all visible lights
            for(i = 0; i < count; i++)
            {
                VisibleLight light = visibleLights[i];

                // Setup matrices
                if(light.lightType == LightType.Directional)
                {
                    Vector4 dir = light.localToWorldMatrix.GetColumn(2);

                    // Negate for inverting direction
                    dir.x = -dir.x;
                    dir.y = -dir.y;
                    dir.z = -dir.z;

                    visibleLightDirections[i] = dir;
                }
                else
                {
                    visibleLightDirections[i] = light.localToWorldMatrix.GetColumn(3);
                }

                // Light color
                visibleLightColors[i] = light.finalColor;

                // Attenuation
                if(light.lightType != LightType.Directional)
                {
                    // attenuation = 1.0 / distanceToLightSqr
                    // The smoothing factors make sure that the light intensity is zero at the light range limit.
                    // The smoothing factor is a linear fade starting at 80% of the light range.
                    // smoothFactor = (lightRangeSqr - distanceToLightSqr) / (lightRangeSqr - fadeStartDistanceSqr)
                    //
                    // Pre-compute the constant terms below and apply the smooth factor with one MAD instruction
                    // smoothFactor =  distanceSqr * (1.0 / (fadeDistanceSqr - lightRangeSqr)) + (-lightRangeSqr / (fadeDistanceSqr - lightRangeSqr)
                    //                 distanceSqr *           oneOverFadeRangeSqr             +              lightRangeSqrOverFadeRangeSqr
                    float lightRangeSqr = light.range * light.range;
                    float fadeStartDistanceSqr = 0.8f * 0.8f * lightRangeSqr;
                    float fadeRangeSqr = (fadeStartDistanceSqr - lightRangeSqr);
                    float oneOverFadeRangeSqr = 1.0f / fadeRangeSqr;
                    float lightRangeSqrOverFadeRangeSqr = -lightRangeSqr / fadeRangeSqr;

                    visibleLightAttenuations[i] = new Vector4(oneOverFadeRangeSqr, lightRangeSqrOverFadeRangeSqr, 0f, 1f);
                }
                else
                {
                    visibleLightAttenuations[i] = new Vector4(0f, 1f, 0f, 1f);
                }

                // Spot direction
                if(light.lightType == LightType.Spot)
                {
                    Vector4 dir = light.localToWorldMatrix.GetColumn(2);
                    visibleLightSpotDirections[i] = new Vector4(-dir.x, -dir.y, -dir.z, 0.0f);

                    // Spot attenuation with linear falloff
                    // SdotL = dot product from spot direction and light direction
                    // (SdotL - cosOuterAngle) / (cosInnerAngle - cosOuterAngle)
                    // This can be rewritten as
                    // invAngleRange = 1.0 / (cosInnerAngle - cosOuterAngle)
                    // SdotL * invAngleRange + (-cosOuterAngle * invAngleRange)

                    float outerAngle = Mathf.Deg2Rad * light.spotAngle * 0.5f;
                    float cosOuterAngle = Mathf.Cos(outerAngle);
                    float tanOuterAngle = Mathf.Tan(outerAngle);
                    float cosInnerAngle;

                    // Null check for particle lights
                    // Particle lights will use an inline function as used by the Universal RP
                    if(light.light != null)
                        cosInnerAngle = Mathf.Cos(light.light.innerSpotAngle * Mathf.Deg2Rad * 0.5f);
                    else
                        cosInnerAngle = Mathf.Cos(Mathf.Atan(tanOuterAngle * ((64.0f - 18.0f) / 64.0f)));

                    float smoothAngleRange = Mathf.Max(0.001f, cosInnerAngle - cosOuterAngle);
                    float invAngleRange = 1.0f / smoothAngleRange;

                    visibleLightAttenuations[i].z = invAngleRange;
                    visibleLightAttenuations[i].w = -cosOuterAngle * invAngleRange;
                }
                else
                {
                    visibleLightSpotDirections[i] = new Vector4(0f, 0f, 1f, 0f);
                }
            }

            // Clear unused lights
            for(; i < MaxVisibleLights; i++)
                visibleLightColors[i] = Color.clear;

            lightingPOD = PerObjectData.LightData | PerObjectData.LightIndices;
        }
    }
}

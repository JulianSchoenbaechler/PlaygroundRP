using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace JulianSchoenbaechler.Rendering.PlaygroundRP
{
    public class ForwardLights
    {
        public const int MaxVisibleLights = 16;

        protected Vector4[] visibleLightColors = new Vector4[MaxVisibleLights];
        protected Vector4[] visibleLightDirections = new Vector4[MaxVisibleLights];
        protected Vector4[] visibleLightAttenuations = new Vector4[MaxVisibleLights];
        protected Vector4[] visibleLightSpotDirections = new Vector4[MaxVisibleLights];

        /// <summary>
        /// Setup forward lights from a cameras culling results.
        /// </summary>
        /// <param name="context">The scriptable render context to be used.</param>
        /// <param name="cullingResults">The culling results of a camera.</param>
        /// <param name="lightsPerObject">The upper limit of per object lights.</param>
        /// <returns>The per object data settings used for lighting.</returns>
        public PerObjectData Setup(ScriptableRenderContext context, ref CullingResults cullingResults, int lightsPerObject)
        {
            // Configure lighting
            PerObjectData lightingPOD = ConfigureLights(ref cullingResults);

            SetupLightConstants(context, lightingPOD == PerObjectData.None ? 0 : lightsPerObject);
            return lightingPOD;
        }

        /// <summary>
        /// Configuring lights. Setting indices and light calculations for the different light types.
        /// </summary>
        /// <param name="cullingResults">The culling results of a camera.</param>
        /// <returns>The per object data settings used for lighting.</returns>
        private PerObjectData ConfigureLights(ref CullingResults cullingResults)
        {
            var visibleLights = cullingResults.visibleLights;
            int count = Mathf.Min(visibleLights.Length, MaxVisibleLights);
            int i = 0;

            // No visible lights in scene
            if(count == 0)
                return PerObjectData.None;

            // When light limit gets exceeded
            // Manually indexing lights
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

            return PerObjectData.LightData | PerObjectData.LightIndices;
        }

        private void SetupLightConstants(ScriptableRenderContext context, int lightsPerObjectLimit)
        {
            CommandBuffer cmd = CommandBufferPool.Get("Setup Light Constants");

            cmd.SetGlobalVector(LightConstantBuffer.VisibleLightsCountId, new Vector4(Mathf.Min(MaxVisibleLights, lightsPerObjectLimit), 0.0f, 0.0f, 0.0f));
            cmd.SetGlobalVectorArray(LightConstantBuffer.VisibleLightDirectionsId, visibleLightDirections);
            cmd.SetGlobalVectorArray(LightConstantBuffer.VisibleLightColorsId, visibleLightColors);
            cmd.SetGlobalVectorArray(LightConstantBuffer.VisibleLightAttenuationsId, visibleLightAttenuations);
            cmd.SetGlobalVectorArray(LightConstantBuffer.VisibleLightSpotDirectionsId, visibleLightSpotDirections);

            // Set shader keywords here...

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        /// <summary>
        /// Light constants.
        /// </summary>
        protected static class LightConstantBuffer
        {
            public static int VisibleLightsCountId = Shader.PropertyToID("_VisibleLightsCount");
            public static int VisibleLightDirectionsId = Shader.PropertyToID("_VisibleLightDirections");
            public static int VisibleLightColorsId = Shader.PropertyToID("_VisibleLightColors");
            public static int VisibleLightAttenuationsId = Shader.PropertyToID("_VisibleLightAttenuations");
            public static int VisibleLightSpotDirectionsId = Shader.PropertyToID("_VisibleLightSpotDirections");
        }
    }
}

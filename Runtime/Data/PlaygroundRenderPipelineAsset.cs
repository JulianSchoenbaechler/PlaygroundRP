using UnityEngine;
using UnityEngine.Rendering;

namespace JulianSchoenbaechler.Rendering.PlaygroundRP
{
    [CreateAssetMenu(fileName = "PlaygroundRenderPipelineAsset",
                     menuName = "Rendering/Playground Render Pipeline/Pipeline Asset",
                     order = CoreUtils.assetCreateMenuPriority1)]
    public sealed class PlaygroundRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] private int lightsPerObjectLimit = 4;
        [SerializeField] private ShadowsUtils.ShadowMapSize shadowResolution = ShadowsUtils.ShadowMapSize._1024;

        [SerializeField] private float shadowDistance = 50f;
        [SerializeField] private float shadowDepthBias = 1f;
        [SerializeField] private float shadowNormalBias = 1f;
        [SerializeField] private bool supportsSoftShadows = true;

        [SerializeField] private bool enableInstancing = true;
        [SerializeField] private bool enableDynamicBatching = true;
        [SerializeField] private bool useSRPBatcher = true;

        public int LightsPerObjectLimit => lightsPerObjectLimit;
        public ShadowsUtils.ShadowMapSize ShadowResolution => shadowResolution;

        public float ShadowDistance => shadowDistance;
        public float ShadowDepthBias => shadowDepthBias;
        public float ShadowNormalBias => shadowNormalBias;
        public bool SupportsSoftShadows => supportsSoftShadows;

        public bool EnableInstancing => enableInstancing;
        public bool EnableDynamicBatching => enableDynamicBatching;
        public bool UseSRPBatcher => useSRPBatcher;

        protected override RenderPipeline CreatePipeline()
        {
            return new PlaygroundRenderPipeline(this);
        }
    }

}

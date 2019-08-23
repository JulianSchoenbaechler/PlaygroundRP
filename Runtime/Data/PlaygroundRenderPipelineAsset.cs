using UnityEngine;
using UnityEngine.Rendering;

namespace JulianSchoenbaechler.Rendering.PlaygroundRP
{
    [CreateAssetMenu(fileName = "PlaygroundRenderPipelineAsset",
                     menuName = "Rendering/Playground Render Pipeline/Pipeline Asset",
                     order = CoreUtils.assetCreateMenuPriority1)]
    public sealed class PlaygroundRenderPipelineAsset : RenderPipelineAsset
    {
        [SerializeField] private bool enableInstancing = true;
        [SerializeField] private bool enableDynamicBatching = true;
        [SerializeField] private bool useSRPBatcher = true;

        public bool EnableInstancing => enableInstancing;
        public bool EnableDynamicBatching => enableDynamicBatching;
        public bool UseSRPBatcher => useSRPBatcher;

        protected override RenderPipeline CreatePipeline()
        {
            return new PlaygroundRenderPipeline(this);
        }
    }

}

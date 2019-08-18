using UnityEngine;
using UnityEngine.Rendering;

namespace JulianSchoenbaechler.Rendering.PlaygroundRP
{
    [CreateAssetMenu(fileName = "PlaygroundRenderPipelineAsset",
                     menuName = "Rendering/Playground Render Pipeline/Pipeline Asset",
                     order = CoreUtils.assetCreateMenuPriority1)]
    public sealed class PlaygroundRenderPipelineAsset : RenderPipelineAsset
    {
        protected override RenderPipeline CreatePipeline()
        {
            return new PlaygroundRenderPipeline(this);
        }
    }

}

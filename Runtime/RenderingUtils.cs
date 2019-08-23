using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

namespace JulianSchoenbaechler.Rendering.PlaygroundRP
{
    public static class RenderingUtils
    {
        // Caches render texture format support. SystemInfo.SupportsRenderTextureFormat allocates memory due to boxing.
        private static Dictionary<RenderTextureFormat, bool> renderTextureFormatSupport = new Dictionary<RenderTextureFormat, bool>();

        // Editor error material
        private static Material errorMaterial;

        // List of unsupported legacy shader passes
        private static List<ShaderTagId> legacyShaderPassNames = new List<ShaderTagId>()
        {
            new ShaderTagId("Always"),
            new ShaderTagId("ForwardBase"),
            new ShaderTagId("PrepassBase"),
            new ShaderTagId("Vertex"),
            new ShaderTagId("VertexLMRGBM"),
            new ShaderTagId("VertexLM"),
        };

        /// <summary>
        /// Clears render texture format support cache.
        /// </summary>
        internal static void ClearSystemInfoCache() => renderTextureFormatSupport.Clear();

        /// <summary>
        /// Check device support for a specified render texture format.
        /// This call caches the result to reduce GC allocations.
        /// </summary>
        /// <param name="format">The render texture format to check.</param>
        /// <returns><c>true</c> if format is supported; otherwise <c>false</c>.</returns>
        internal static bool SupportsRenderTextureFormat(RenderTextureFormat format)
        {
            if(!renderTextureFormatSupport.TryGetValue(format, out var support))
            {
                support = SystemInfo.SupportsRenderTextureFormat(format);
                renderTextureFormatSupport.Add(format, support);
            }

            return support;
        }

        /// <summary>
        /// Gets the Unity error material.
        /// </summary>
        /// <value>The Unity editor error material.</value>
        private static Material ErrorMaterial => errorMaterial ?? new Material(Shader.Find("Hidden/InternalErrorShader"));

        [Conditional("DEVELOPMENT_BUILD"), Conditional("UNITY_EDITOR")]
        internal static void RenderObjectsWithError(ScriptableRenderContext context, ref CullingResults cullResults, Camera camera, FilteringSettings filterSettings, SortingCriteria sortFlags)
        {
            SortingSettings sortingSettings = new SortingSettings(camera) { criteria = sortFlags };
            DrawingSettings errorSettings = new DrawingSettings(legacyShaderPassNames[0], sortingSettings)
            {
                perObjectData = PerObjectData.None,
                overrideMaterial = ErrorMaterial,
                overrideMaterialPassIndex = 0
            };

            for(int i = 1; i < legacyShaderPassNames.Count; ++i)
                errorSettings.SetShaderPassName(i, legacyShaderPassNames[i]);

            context.DrawRenderers(cullResults, ref errorSettings, ref filterSettings);
        }
    }
}

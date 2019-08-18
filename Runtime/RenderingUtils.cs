using System.Collections.Generic;
using UnityEngine;

namespace JulianSchoenbaechler.Rendering.PlaygroundRP
{
    public static class RenderingUtils
    {
        // Caches render texture format support. SystemInfo.SupportsRenderTextureFormat allocates memory due to boxing.
        static Dictionary<RenderTextureFormat, bool> renderTextureFormatSupport = new Dictionary<RenderTextureFormat, bool>();

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
    }
}

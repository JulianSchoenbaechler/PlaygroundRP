using UnityEngine;
using UnityEditor;

namespace JulianSchoenbaechler.Rendering.PlaygroundRP
{
    [CustomEditor(typeof(PlaygroundRenderPipelineAsset))]
    public sealed class PlaygroundRenderPipelineAssetEditor : Editor
    {
        private SavedBool generalSettingsFoldout;
        private SavedBool lightingSettingsFoldout;
        private SavedBool shadowsSettingsFoldout;
        private SavedBool advancedSettingsFoldout;

        private SerializedProperty lightsPerObjectLimit;
        private SerializedProperty shadowResolution;

        private SerializedProperty shadowDistance;
        private SerializedProperty shadowDepthBias;
        private SerializedProperty shadowNormalBias;
        private SerializedProperty supportsSoftShadows;

        private SerializedProperty useSRPBatcher;
        private SerializedProperty enableDynamicBatching;
        private SerializedProperty enableInstancing;

        /// <summary>
        /// Is called for rendering and handling GUI events in the inspector.
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawGeneralSettings();
            DrawLightingSettings();
            DrawShadowsSettings();
            DrawAdvancedSettings();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw general settings.
        /// Must be called from GUI call.
        /// </summary>
        private void DrawGeneralSettings()
        {
            generalSettingsFoldout.Value = EditorGUILayout.BeginFoldoutHeaderGroup(
                generalSettingsFoldout.Value,
                Styles.GeneralSettingsText
            );

            if(generalSettingsFoldout.Value)
            {
                EditorGUI.indentLevel++;
                // General setting property
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// Draw lighting settings.
        /// Must be called from GUI call.
        /// </summary>
        private void DrawLightingSettings()
        {
            lightingSettingsFoldout.Value = EditorGUILayout.BeginFoldoutHeaderGroup(
                lightingSettingsFoldout.Value,
                Styles.LightingSettingsText
            );

            if(lightingSettingsFoldout.Value)
            {
                EditorGUI.indentLevel++;
                lightsPerObjectLimit.intValue = EditorGUILayout.IntSlider(Styles.PerObjectLimit, lightsPerObjectLimit.intValue, 1, 8);
                EditorGUILayout.PropertyField(shadowResolution, Styles.LightsShadowmapResolution);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// Draw shadows settings.
        /// Must be called from GUI call.
        /// </summary>
        private void DrawShadowsSettings()
        {
            shadowsSettingsFoldout.Value = EditorGUILayout.BeginFoldoutHeaderGroup(
                shadowsSettingsFoldout.Value,
                Styles.ShadowSettingsText
            );

            if(shadowsSettingsFoldout.Value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(shadowDistance, Styles.ShadowDistanceText);
                shadowDepthBias.floatValue = EditorGUILayout.Slider(Styles.ShadowDepthBias, shadowDepthBias.floatValue, 0f, PlaygroundRenderPipeline.MaxShadowBias);
                shadowNormalBias.floatValue = EditorGUILayout.Slider(Styles.ShadowNormalBias, shadowNormalBias.floatValue, 0f, PlaygroundRenderPipeline.MaxShadowBias);
                EditorGUILayout.PropertyField(supportsSoftShadows, Styles.SupportsSoftShadows);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// Draw advanced settings.
        /// Must be called from GUI call.
        /// </summary>
        private void DrawAdvancedSettings()
        {
            advancedSettingsFoldout.Value = EditorGUILayout.BeginFoldoutHeaderGroup(
                advancedSettingsFoldout.Value,
                Styles.AdvancedSettingsText
            );

            if(advancedSettingsFoldout.Value)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(enableDynamicBatching, Styles.DynamicBatching);
                EditorGUILayout.PropertyField(enableInstancing, Styles.Instancing);
                EditorGUILayout.PropertyField(useSRPBatcher, Styles.SRPBatcher);
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
                EditorGUILayout.Space();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            generalSettingsFoldout = new SavedBool($"{target.GetType()}.GeneralSettingsFoldout", false);
            lightingSettingsFoldout = new SavedBool($"{target.GetType()}.LightingSettingsFoldout", false);
            shadowsSettingsFoldout = new SavedBool($"{target.GetType()}.ShadowsSettingsFoldout", false);
            advancedSettingsFoldout = new SavedBool($"{target.GetType()}.AdvancedSettingsFoldout", false);

            lightsPerObjectLimit = serializedObject.FindProperty("lightsPerObjectLimit");
            shadowResolution = serializedObject.FindProperty("shadowResolution");

            shadowDistance = serializedObject.FindProperty("shadowDistance");
            shadowDepthBias = serializedObject.FindProperty("shadowDepthBias");
            shadowNormalBias = serializedObject.FindProperty("shadowNormalBias");
            supportsSoftShadows = serializedObject.FindProperty("supportsSoftShadows");

            enableInstancing = serializedObject.FindProperty("enableInstancing");
            enableDynamicBatching = serializedObject.FindProperty("enableDynamicBatching");
            useSRPBatcher = serializedObject.FindProperty("useSRPBatcher");
        }

        /// <summary>
        /// Internal storage for editor styles.
        /// </summary>
        internal static class Styles
        {
            // Groups
            public static GUIContent GeneralSettingsText = EditorGUIUtility.TrTextContent("General");
            public static GUIContent QualitySettingsText = EditorGUIUtility.TrTextContent("Quality");
            public static GUIContent LightingSettingsText = EditorGUIUtility.TrTextContent("Lighting");
            public static GUIContent ShadowSettingsText = EditorGUIUtility.TrTextContent("Shadows");
            public static GUIContent PostProcessingSettingsText = EditorGUIUtility.TrTextContent("Post-processing");
            public static GUIContent AdvancedSettingsText = EditorGUIUtility.TrTextContent("Advanced");

            // General
            //public static GUIContent requireDepthTextureText = EditorGUIUtility.TrTextContent("Depth Texture", "If enabled the pipeline will generate camera's depth that can be bound in shaders as _CameraDepthTexture.");
            //public static GUIContent requireOpaqueTextureText = EditorGUIUtility.TrTextContent("Opaque Texture", "If enabled the pipeline will copy the screen to texture after opaque objects are drawn. For transparent objects this can be bound in shaders as _CameraOpaqueTexture.");

            // Quality
            public static GUIContent MSAAText = EditorGUIUtility.TrTextContent("Anti Aliasing (MSAA)", "Controls the global anti aliasing settings.");

            // Main light
            //public static GUIContent mainLightRenderingModeText = EditorGUIUtility.TrTextContent("Main Light", "Main light is the brightest directional light.");
            //public static GUIContent supportsMainLightShadowsText = EditorGUIUtility.TrTextContent("Cast Shadows", "If enabled the main light can be a shadow casting light.");
            //public static GUIContent mainLightShadowmapResolutionText = EditorGUIUtility.TrTextContent("Shadow Resolution", "Resolution of the main light shadowmap texture. If cascades are enabled, cascades will be packed into an atlas and this setting controls the maximum shadows atlas resolution.");

            // Lighting
            //public static GUIContent addditionalLightsRenderingModeText = EditorGUIUtility.TrTextContent("Additional Lights", "Additional lights support.");
            public static GUIContent PerObjectLimit = EditorGUIUtility.TrTextContent("Per Object Limit", "Maximum amount of lights. These lights are sorted and culled per-object.");
            public static GUIContent LightsShadowmapResolution = EditorGUIUtility.TrTextContent("Shadow Resolution", "All lights are packed into a single shadowmap atlas. This setting controls the atlas size.");
            //public static GUIContent supportsAdditionalShadowsText = EditorGUIUtility.TrTextContent("Cast Shadows", "If enabled shadows will be supported for spot lights.\n");

            // Shadow settings
            public static GUIContent ShadowDistanceText = EditorGUIUtility.TrTextContent("Distance", "Maximum shadow rendering distance.");
            //public static GUIContent shadowCascadesText = EditorGUIUtility.TrTextContent("Cascades", "Number of cascade splits used in for directional shadows");
            public static GUIContent ShadowDepthBias = EditorGUIUtility.TrTextContent("Depth Bias", "Controls the distance at which the shadows will be pushed away from the light. Useful for avoiding false self-shadowing artifacts.");
            public static GUIContent ShadowNormalBias = EditorGUIUtility.TrTextContent("Normal Bias", "Controls distance at which the shadow casting surfaces will be shrunk along the surface normal. Useful for avoiding false self-shadowing artifacts.");
            public static GUIContent SupportsSoftShadows = EditorGUIUtility.TrTextContent("Soft Shadows", "If enabled pipeline will perform shadow filtering. Otherwise all lights that cast shadows will fallback to perform a single shadow sample.");

            // Advanced settings
            public static GUIContent SRPBatcher = EditorGUIUtility.TrTextContent("SRP Batcher", "If enabled, the render pipeline uses the SRP batcher.");
            public static GUIContent DynamicBatching = EditorGUIUtility.TrTextContent("Dynamic Batching", "If enabled, the render pipeline will batch drawcalls with few triangles together by copying their vertex buffers into a shared buffer on a per-frame basis.");
            public static GUIContent Instancing = EditorGUIUtility.TrTextContent("GPU Instancing", "Use GPU Instancing to draw (or render) multiple copies of the same Mesh at once, using a small number of draw calls.");
            //public static GUIContent mixedLightingSupportLabel = EditorGUIUtility.TrTextContent("Mixed Lighting", "Support for mixed light mode.");

            // Dropdown menu options
            //public static string[] mainLightOptions = { "Disabled", "Per Pixel" };
            //public static string[] shadowCascadeOptions = { "No Cascades", "Two Cascades", "Four Cascades" };
            //public static string[] opaqueDownsamplingOptions = { "None", "2x (Bilinear)", "4x (Box)", "4x (Bilinear)" };
        }
    }
}

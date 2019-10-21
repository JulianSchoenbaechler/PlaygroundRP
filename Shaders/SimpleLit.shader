Shader "Playground Render Pipeline/Simple Lit"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)

        // Blending state
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0

        [ToggleOff] _ReceiveShadows("Receive Shadows", Float) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "IgnoreProjector" = "True"
            "RenderPipeline" = "PlaygroundPipeline"
        }

        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "BasePass" }

            // Use same blending / depth states as Standard shader
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma multi_compile_instancing

            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple

            #include "Packages/ch.julian-s.srp.playground/ShaderLibrary/Core.hlsl"
            #include "Packages/ch.julian-s.srp.playground/ShaderLibrary/Lighting.hlsl"

            #include "Packages/ch.julian-s.srp.playground/Shaders/SimpleLitInput.hlsl"
            #include "Packages/ch.julian-s.srp.playground/Shaders/SimpleLitForwardPass.hlsl"

            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            Cull[_Cull]

            HLSLPROGRAM

            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            // Keywords
            //#pragma shader_feature _ALPHATEST_ON
            //#pragma shader_feature _GLOSSINESS_FROM_BASE_ALPHA

            #pragma multi_compile_instancing

            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment

            #include "Packages/ch.julian-s.srp.playground/ShaderLibrary/Core.hlsl"
            #include "Packages/ch.julian-s.srp.playground/ShaderLibrary/Lighting.hlsl"

            #include "Packages/ch.julian-s.srp.playground/Shaders/SimpleLitInput.hlsl"
            #include "Packages/ch.julian-s.srp.playground/Shaders/ShadowCasterPass.hlsl"

            ENDHLSL
        }
    }

    Fallback "Hidden/InternalErrorShader"
}

Shader "Playground Render Pipeline/Simple Lit"
{
    Properties
    {
        _BaseMap ("Texture", 2D) = "white" {}
        _BaseColor ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
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
            Tags { "LightMode" = "BasePass" }

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
    }

    Fallback "Hidden/InternalErrorShader"
}

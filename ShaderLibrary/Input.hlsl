#ifndef PLAYGROUND_INPUT_INCLUDED
#define PLAYGROUND_INPUT_INCLUDED

// No use of SSBO as UBO has easier backward compatibility
// Also on D3D one cannot figure out if platforms is D3D10 without adding shader variants
// Also on Nintendo Switch, UBO path is faster
#define MAX_VISIBLE_LIGHTS      16
// Some mobile GPUs have small SP cache for constants
// Using more than 16 might cause spilling to main memory

struct InputData
{
    float3  positionWS;
    half3   normalWS;
    half3   viewDirectionWS;
    float4  shadowCoord;
    half    fogCoord;
    half3   vertexLighting;
    half3   bakedGI;
};

CBUFFER_START(_LightBuffer)
    half4 _VisibleLightsCount;
    half4 _VisibleLightDirections[MAX_VISIBLE_LIGHTS];
    half4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
    half4 _VisibleLightAttenuations[MAX_VISIBLE_LIGHTS];
    half4 _VisibleLightSpotDirections[MAX_VISIBLE_LIGHTS];
CBUFFER_END

TEXTURE2D_SHADOW(_ShadowMap);
SAMPLER_CMP(sampler_ShadowMap);

CBUFFER_START(_ShadowBuffer)
    float4x4 _WorldToShadowMatrix;
CBUFFER_END

// Move this....
float ShadowAttenuation(float3 positionWS)
{
    float4 shadowPos = mul(_WorldToShadowMatrix, float4(positionWS, 1.0));
    shadowPos.xyz /= shadowPos.w;

    float attenuation = SAMPLE_TEXTURE2D_SHADOW(_ShadowMap, sampler_ShadowMap, shadowPos.xyz);
    //attenuation = LerpWhiteTo(attenuation, 1.0);
    return attenuation;
}

#include "Packages/ch.julian-s.srp.playground/ShaderLibrary/UnityInput.hlsl"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

#endif // PLAYGROUND_INPUT_INCLUDED

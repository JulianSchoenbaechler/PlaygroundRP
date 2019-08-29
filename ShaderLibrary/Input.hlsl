#ifndef PLAYGROUND_INPUT_INCLUDED
#define PLAYGROUND_INPUT_INCLUDED

// No use of SSBO as UBO has easier backward compatibility
// Also on D3D one cannot figure out if platforms is D3D10 without adding shader variants
// Also on Nintendo Switch, UBO path is faster
#define MAX_VISIBLE_LIGHTS      32
// Some mobile GPUs have small SP cache for constants
// Using more than 32 might cause spilling to main memory

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
    float4 _VisibleLightColors[MAX_VISIBLE_LIGHTS];
    float4 _VisibleLightDirections[MAX_VISIBLE_LIGHTS];
CBUFFER_END

#include "Packages/ch.julian-s.srp.playground/ShaderLibrary/UnityInput.hlsl"

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

#endif // PLAYGROUND_INPUT_INCLUDED

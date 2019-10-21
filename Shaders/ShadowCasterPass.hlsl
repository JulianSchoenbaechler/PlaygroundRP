#ifndef PLAYGROUND_SHADOW_CASTER_PASS_INCLUDED
#define PLAYGROUND_SHADOW_CASTER_PASS_INCLUDED

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float2 texcoord     : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;
};


////////////////////////////////////////////////////////////////////////////////
// Shadow clip space

float4 GetShadowPositionHClip(Attributes input)
{
    //float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    //float3 normalWS = TransformObjectToWorldNormal(input.normalOS);

    //float4 positionCS = TransformWorldToHClip(ApplyShadowBias(positionWS, normalWS, _LightDirection));
    float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
    float4 positionCS = TransformWorldToHClip(positionWS);

#if UNITY_REVERSED_Z
    positionCS.z = min(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#else
    positionCS.z = max(positionCS.z, positionCS.w * UNITY_NEAR_CLIP_VALUE);
#endif

    return positionCS;
}


////////////////////////////////////////////////////////////////////////////////
// Vertex and Fragment programs

// Vertex shader
Varyings ShadowPassVertex(Attributes input)
{
    Varyings output;

    // Make instancing id aviable
    UNITY_SETUP_INSTANCE_ID(input);

    // Object space to homogenous space
    output.positionCS = GetShadowPositionHClip(input);

    // Transforms 2D UV by scale/bias property
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);

    return output;
}

// Fragment shader
real4 ShadowPassFragment(Varyings input) : SV_Target
{
    return 0;
}

#endif // PLAYGROUND_SHADOW_CASTER_PASS_INCLUDED

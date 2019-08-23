#ifndef PLAYGROUND_UNLIT_PASS_INCLUDED
#define PLAYGROUND_UNLIT_PASS_INCLUDED

struct Attributes
{
    float4 positionOS   : POSITION;
    float2 uv           : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Vertex shader
Varyings vert(Attributes input)
{
    Varyings output;

    // Make instancing id aviable
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    // Object space to homogenous space
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

    // Transforms 2D UV by scale/bias property
    output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

    return output;
}

// Fragmentation shader
real4 frag(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);

    return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
}

#endif // PLAYGROUND_UNLIT_PASS_INCLUDED

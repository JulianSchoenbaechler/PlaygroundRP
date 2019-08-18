#ifndef PLAYGROUND_UNLIT_PASS_INCLUDED
#define PLAYGROUND_UNLIT_PASS_INCLUDED

struct Attributes
{
    float4 positionOS   : POSITION;
    float2 uv           : TEXCOORD0;
};

struct Varyings
{
    float4 positionCS   : SV_POSITION;
    float2 uv           : TEXCOORD0;
};

// Vertex shader
Varyings vert(Attributes IN)
{
    Varyings OUT;

    // Object space to homogenous space
    OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);

    // Transforms 2D UV by scale/bias property
    OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

    return OUT;
}

// Fragmentation shader
real4 frag(Varyings IN) : SV_Target
{
    return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
}

#endif // PLAYGROUND_UNLIT_PASS_INCLUDED

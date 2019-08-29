#ifndef PLAYGROUND_SIMPLELIT_PASS_INCLUDED
#define PLAYGROUND_SIMPLELIT_PASS_INCLUDED

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
    float3 posWS        : TEXCOORD2;
    float3 normal       : TEXCOORD3;
    float3 viewDir      : TEXCOORD4;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

// Vertex shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output;

    // Make instancing id aviable
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    // Object space to homogenous space
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

    half3 posWS = TransformObjectToWorld(input.positionOS.xyz);

    // Transforms 2D UV by scale/bias property
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.viewDir = _WorldSpaceCameraPos - posWS;
    output.posWS = posWS;
    output.normal = input.normalOS;

    return output;
}

// Fragment shader
real4 LitPassFragmentSimple(Varyings input) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(input);
    input.normal = normalize(input.normal);

    //return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;

    // Basic Blinn-Phong
    half3 diffuseLight = 0;

    for(uint i = 0; i < 4; i++)
    {
        diffuseLight += LightingLambert(
            _VisibleLightColors[i].rgb,
            _VisibleLightDirections[i],
            TransformObjectToWorldNormal(input.normal),
            input.posWS
        );
    }

    return half4(diffuseLight, 1) * _BaseColor;
}

#endif // PLAYGROUND_SIMPLELIT_PASS_INCLUDED

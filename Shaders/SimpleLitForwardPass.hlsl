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

////////////////////////////////////////////////////////////////////////////////
// Input data

void InitializeInputData(Varyings input/* , half3 normalTS */, out InputData inputData)
{
    inputData.positionWS = input.posWS;

#ifdef _NORMALMAP
    /* half3 viewDirWS = half3(input.normal.w, input.tangent.w, input.bitangent.w);
    inputData.normalWS = TransformTangentToWorld(
        normalTS,
        half3x3(input.tangent.xyz, input.bitangent.xyz, input.normal.xyz)
    ); */
#else
    half3 viewDirWS = input.viewDir;
    inputData.normalWS = input.normal;
#endif

    //inputData.normalWS = normalize(inputData.normalWS);   // Beautifuller but not necessary
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;

/* #if defined(_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
    inputData.shadowCoord = input.shadowCoord;
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif */

    //inputData.fogCoord = input.fogFactorAndVertexLight.x;
    //inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    //inputData.bakedGI = SAMPLE_GI(input.lightmapUV, input.vertexSH, inputData.normalWS);

    inputData.shadowCoord = 0;
    inputData.fogCoord = 0;
    inputData.vertexLighting = 0;
    inputData.bakedGI = 0;
}

////////////////////////////////////////////////////////////////////////////////
// Vertex and Fragment programs


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

    InputData inputData;
    InitializeInputData(input, inputData);

    // Basic Blinn-Phong
    return FragmentDiffuse(inputData, _BaseColor);;
}

#endif // PLAYGROUND_SIMPLELIT_PASS_INCLUDED

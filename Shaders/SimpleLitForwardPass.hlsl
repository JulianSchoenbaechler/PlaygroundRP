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
    float3 normal       : TEXCOORD3;
    float3 viewDir      : TEXCOORD4;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

////////////////////////////////////////////////////////////////////////////////
// Basic lighting

half3 LightingLambert(half3 lightColor, half3 lightDir, half3 normal)
{
    half NdotL = saturate(dot(normal, lightDir));
    return lightColor * NdotL;
}

half3 LightingSpecular(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir, half4 specular, half smoothness)
{
    float3 halfVec = SafeNormalize(float3(lightDir) + float3(viewDir));
    half NdotH = saturate(dot(normal, halfVec));
    half modifier = pow(NdotH, smoothness);
    half3 specularReflection = specular.rgb * modifier;
    return lightColor * specularReflection;
}

////////////////////////////////////////////////////////////////////////////////

// Vertex shader
Varyings LitPassVertexSimple(Attributes input)
{
    Varyings output;

    // Make instancing id aviable
    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_TRANSFER_INSTANCE_ID(input, output);

    // Object space to homogenous space
    output.positionCS = TransformObjectToHClip(input.positionOS.xyz);

    // Transforms 2D UV by scale/bias property
    output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
    output.viewDir = _WorldSpaceCameraPos - TransformObjectToWorld(input.positionOS.xyz);
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
    half3 diffuseLight = LightingLambert(half3(1, 1, 1), TransformObjectToWorldNormal(input.normal), half3(0, 1, 0));

    return half4(diffuseLight, 1) * _BaseColor;
}

#endif // PLAYGROUND_SIMPLELIT_PASS_INCLUDED

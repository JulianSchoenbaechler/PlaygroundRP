#ifndef PLAYGROUND_LIGHTING_INCLUDED
#define PLAYGROUND_LIGHTING_INCLUDED

// Basic light structure
struct Light
{
    half3   direction;
    half3   color;
    half    distanceAttenuation;
    half    shadowAttenuation;
};

////////////////////////////////////////////////////////////////////////////////
// Light data


// Returns a per-object index given a loop index.
// This abstract the underlying data implementation for storing lights/light indices
int GetPerObjectLightIndex(uint index)
{
    // Standard UBO path
    //
    // We store 8 light indices in float4 unity_LightIndices[2];
    // Due to memory alignment unity doesn't support int[] or float[]
    // Even trying to reinterpret cast the unity_LightIndices to float[] won't work
    // it will cast to float4[] and create extra register pressure. :(
#if !defined(SHADER_API_GLES)
    // since index is uint -> shader compiler will implement
    // div & mod as bitfield ops (shift and mask).
    return unity_LightIndices[index / 4][index % 4];
#else
    // Fallback to GLES2. No bitfield magic here :(.
    // We limit to 4 indices per object and only sample unity_LightIndices[0].
    // Conditional moves are branch free even on mali-400
    // Small arithmetic cost but no extra register pressure from ImmCB_0_0_0 matrix.
    half2 lightIndex2 = (index < 2.0h) ? unity_LightIndices[0].xy : unity_LightIndices[0].zw;
    half i_rem = (index < 2.0h) ? index : index - 2.0h;
    return (i_rem < 1.0h) ? lightIndex2.x : lightIndex2.y;
#endif
}

Light GetPerObjectLight(uint perObjectLightIndex, float3 positionWS)
{
    float4 directionPositionWS = _VisibleLightDirections[perObjectLightIndex];
    half3 color = _VisibleLightColors[perObjectLightIndex].rgb;

    float3 lightVector = SafeNormalize(directionPositionWS.xyz - positionWS * directionPositionWS.w);
    float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    // lightVector / sqr(distanceSqr)
    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));

    Light light;
    light.direction = lightDirection;
    //light.distanceAttenuation = attenuation;
    //light.shadowAttenuation
    light.color = color;

    return light;
}

Light GetLightFromLoop(uint i, float3 positionWS)
{
    int perObjectLightIndex = GetPerObjectLightIndex(i);
    return GetPerObjectLight(perObjectLightIndex, positionWS);
}

uint GetLightsCount()
{
    return min(_VisibleLightsCount.x, unity_LightData.y);
}

////////////////////////////////////////////////////////////////////////////////
// Basic lighting

half3 LightingLambert(half3 lightColor, half3 lightDir, half3 normal)
{
    half NdotL = saturate(dot(normal, lightDir));
    return lightColor * NdotL;
}

half3 LightingSpecular(half3 lightColor, half3 lightDir, half3 normal, half3 viewDir, half4 specular, half smoothness)
{
    float3 halfVec = SafeNormalize(float3(lightDir.xyz) + float3(viewDir));
    half NdotH = saturate(dot(normal, halfVec));
    half modifier = pow(NdotH, smoothness);
    half3 specularReflection = specular.rgb * modifier;
    return lightColor * specularReflection;
}

half4 FragmentDiffuse(InputData inputData, half3 diffuse/*,  half4 specularGloss, half smoothness, half3 emission, half alpha */)
{
    half3 diffuseColor = 0;
    uint lightsCount = GetLightsCount();

    for(uint i = 0; i < lightsCount; i++)
    {
        Light light = GetLightFromLoop(i, inputData.positionWS);

        diffuseColor += LightingLambert(light.color, light.direction, inputData.normalWS);
    }

    half3 finalColor = diffuseColor * diffuse;

    return half4(finalColor, 1.0);
}

#endif // PLAYGROUND_LIGHTING_INCLUDED

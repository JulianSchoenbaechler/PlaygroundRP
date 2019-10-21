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
// Attenuation

// Attenuation smoothly decreases to light range.
float DistanceAttenuation(float distanceSqr, half2 distanceAttenuation)
{
    // We use a shared distance attenuation for additional directional and puctual lights
    // for directional lights attenuation will be 1
    float lightAtten = rcp(distanceSqr);

    // Smoothly fade attenuation to light range. Start fading linearly at 80% of light range.
    // Therefore:
    // fadeDistance = (0.8 * 0.8 * lightRangeSqr)
    // smoothFactor = (lightRangeSqr - distanceSqr) / (lightRangeSqr - fadeDistance)
    // To fit a MAD instruciton by doing:
    // distanceSqr * (1.0 / (fadeDistanceSqr - lightRangeSqr)) + (-lightRangeSqr / (fadeDistanceSqr - lightRangeSqr)
    // distanceSqr *          distanceAttenuation.x            +             distanceAttenuation.y
    half smoothFactor = saturate(distanceSqr * distanceAttenuation.x + distanceAttenuation.y);

    return lightAtten * smoothFactor;
}

// Spot angle attenuation
half AngleAttenuation(half3 spotDirection, half3 lightDirection, half2 spotAttenuation)
{
    // Spot attenuation with linear falloff
    // SdotL = dot product from spot direction and light direction
    // (SdotL - cosOuterAngle) / (cosInnerAngle - cosOuterAngle)
    // This can be rewritten as (fit MAD)
    // invAngleRange = 1.0 / (cosInnerAngle - cosOuterAngle)
    // SdotL * invAngleRange + (-cosOuterAngle * invAngleRange)
    // SdotL * spotAttenuation.x + spotAttenuation.y

    half SdotL = dot(spotDirection, lightDirection);
    half atten = saturate(SdotL * spotAttenuation.x + spotAttenuation.y);
    return atten * atten;
}

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
    half4 distanceAndSpotAttenuation = _VisibleLightAttenuations[perObjectLightIndex];
    half4 spotDirection = _VisibleLightSpotDirections[perObjectLightIndex];

    float3 lightVector = directionPositionWS.xyz - positionWS * directionPositionWS.w;
    float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    // lightVector / sqr(distanceSqr)
    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));
    half attenuation = DistanceAttenuation(distanceSqr, distanceAndSpotAttenuation.xy);
    attenuation *= AngleAttenuation(spotDirection.xyz, lightDirection, distanceAndSpotAttenuation.zw);

    Light light;
    light.direction = lightDirection;
    light.distanceAttenuation = attenuation;
    light.shadowAttenuation = ShadowAttenuation(positionWS);
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

    for(uint i = 0; i < MAX_VISIBLE_LIGHTS; i++)
    {
        if(i >= lightsCount)
            break;

        Light light = GetLightFromLoop(i, inputData.positionWS);
        half3 attenuatedLightColor = light.color * (light.distanceAttenuation * light.shadowAttenuation);

        diffuseColor += LightingLambert(attenuatedLightColor, light.direction, inputData.normalWS);
    }

    half3 finalColor = diffuseColor * diffuse;

    return half4(finalColor, 1.0);
}

#endif // PLAYGROUND_LIGHTING_INCLUDED

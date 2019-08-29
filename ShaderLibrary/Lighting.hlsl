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

Light GetPerObjectLight(int perObjectLightIndex, float3 positionWS)
{
    float4 directionPositionWS = _VisibleLightDirections[perObjectLightIndex];
    half3 color = _VisibleLightColors[perObjectLightIndex].rgb;

    float3 lightVector = SafeNormalize(directionPositionWS.xyz - positionWS * directionPositionWS.w);
    float distanceSqr = max(dot(lightVector, lightVector), HALF_MIN);

    half3 lightDirection = half3(lightVector * rsqrt(distanceSqr));

    Light light;
    light.direction = lightVector; //lightDirection;
    //light.distanceAttenuation = attenuation;
    //light.shadowAttenuation
    light.color = color;

    return light;
}

Light GetLightFromLoop(uint i, float3 positionWS)
{
    return GetPerObjectLight(i, positionWS);
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

    for(uint i = 0; i < MAX_VISIBLE_LIGHTS; i++)
    {
        Light light = GetLightFromLoop(i, inputData.positionWS);

        diffuseColor += LightingLambert(light.color, light.direction, inputData.normalWS);
    }

    half3 finalColor = diffuseColor * diffuse;

    return half4(finalColor, 1.0);
}

#endif // PLAYGROUND_LIGHTING_INCLUDED

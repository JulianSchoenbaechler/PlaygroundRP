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
// Basic lighting

half3 LightingLambert(half3 lightColor, half4 lightDir, half3 normal, half3 posWS)
{
    half3 lightVec = SafeNormalize(lightDir.xyz - posWS * lightDir.w);
    half NdotL = saturate(dot(normal, lightVec));
    return lightColor * NdotL;
}

half3 LightingSpecular(half3 lightColor, half4 lightDir, half3 normal, half3 posWS, half3 viewDir, half4 specular, half smoothness)
{
    half3 lightVec = lightDir.xyz - posWS * lightDir.w;
    float3 halfVec = SafeNormalize(float3(lightVec) + float3(viewDir));
    half NdotH = saturate(dot(normal, halfVec));
    half modifier = pow(NdotH, smoothness);
    half3 specularReflection = specular.rgb * modifier;
    return lightColor * specularReflection;
}

//half4 BlinnPhongLighting()

#endif // PLAYGROUND_LIGHTING_INCLUDED

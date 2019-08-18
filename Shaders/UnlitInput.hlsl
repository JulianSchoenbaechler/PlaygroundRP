#ifndef PLAYGROUND_UNLIT_INPUT_INCLUDED
#define PLAYGROUND_UNLIT_INPUT_INCLUDED

CBUFFER_START(UnityPerMaterial)
float4 _BaseMap_ST;
half4 _BaseColor;
CBUFFER_END

TEXTURE2D(_BaseMap);
SAMPLER(sampler_BaseMap);

#endif // PLAYGROUND_UNLIT_INPUT_INCLUDED

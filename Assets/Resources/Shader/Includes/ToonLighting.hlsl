#ifndef TAGPACH_TOON_LIGHTING_INCLUDED
#define TAGPACH_TOON_LIGHTING_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif

void ToonMainLight_float(
    out float3 Direction,
    out float3 Color)
{
#if defined(SHADERGRAPH_PREVIEW)
    Direction = normalize(float3(0.5, 0.5, 0.5));
    Color = float3(1.0, 1.0, 1.0);
#else
    Light mainLight = GetMainLight();
    Direction = mainLight.direction;
    Color = mainLight.color;
#endif
}

void ToonEnvironmentReflection_float(
    float3 WorldNormal,
    float3 WorldView,
    float Roughness,
    out float3 Out)
{
#if defined(SHADERGRAPH_PREVIEW)
    Out = float3(0.0, 0.0, 0.0);
#else
    float3 normalWS = SafeNormalize(WorldNormal);
    float3 viewWS = SafeNormalize(WorldView);

    float3 reflectionDirection = reflect(-viewWS, normalWS);

    float mip = PerceptualRoughnessToMipmapLevel(
        saturate(Roughness)
    );

    float4 encodedReflection = SAMPLE_TEXTURECUBE_LOD(
        unity_SpecCube0,
        samplerunity_SpecCube0,
        reflectionDirection,
        mip
    );

    Out = DecodeHDREnvironment(
        encodedReflection,
        unity_SpecCube0_HDR
    );
#endif
}

#endif

#ifndef SSS_TRANSMISSION_INCLUDED
#define SSS_TRANSMISSION_INCLUDED

#ifndef SHADERGRAPH_PREVIEW
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#endif

void SSSTransmission_float(
    float3 WorldNormal,
    float3 WorldView,
    float3 WorldPosition,
    float Distortion,
    float Power,
    float ShadowInfluence,
    out float3 Out)
{
#ifdef SHADERGRAPH_PREVIEW

    float3 lightDir =
        normalize(float3(0.5, 0.5, -1.0));

    float3 lightColor = float3(1, 1, 1);

    float shadow = 1.0;

#else

    // メインライトを取得
    Light mainLight = GetMainLight();

    float3 lightDir =
        normalize(mainLight.direction);

    float3 lightColor =
        mainLight.color *
        mainLight.distanceAttenuation;

    float shadow = 1.0;

    // メインライトのリアルタイムシャドウ
    #if defined(_MAIN_LIGHT_SHADOWS) || \
        defined(_MAIN_LIGHT_SHADOWS_CASCADE) || \
        defined(_MAIN_LIGHT_SHADOWS_SCREEN)

        float4 shadowCoord;

        #if defined(_MAIN_LIGHT_SHADOWS_SCREEN)

            float4 clipPos =
                TransformWorldToHClip(WorldPosition);

            shadowCoord = ComputeScreenPos(clipPos);

        #else

            shadowCoord =
                TransformWorldToShadowCoord(
                    WorldPosition);

        #endif

        shadow = MainLightRealtimeShadow(
            shadowCoord);

    #endif

#endif

    float3 N = normalize(WorldNormal);
    float3 V = normalize(WorldView);

    // 逆光の判定
    float backLight =
        saturate(dot(-N, lightDir));

    // 透過方向の調整
    float3 transmissionDir =
        normalize(-lightDir + N * Distortion);

    // 視線方向を考慮した散乱
    float scatter = pow(
        saturate(dot(V, transmissionDir)),
        max(Power, 0.001)
    );

    // 影の影響を調整
    float shadowFactor = lerp(
        1.0,
        shadow,
        saturate(ShadowInfluence)
    );

    // 最終的な疑似SSS
    Out =
        lightColor *
        backLight *
        scatter *
        shadowFactor;
}

#endif

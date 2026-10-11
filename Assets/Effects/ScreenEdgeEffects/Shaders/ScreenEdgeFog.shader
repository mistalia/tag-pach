Shader "TagPach/Effects/ScreenEdgeFog"
{
    Properties
    {
        // MaterialのInspectorから調整する項目。
        // 後で制御用C#からも、同じ項目を操作する。
        [Toggle] _EffectEnabled("Effect Enabled", Float) = 1

        _FogColor("Fog Color", Color) = (0.65, 0.7, 0.8, 1)
        _Intensity("Intensity", Range(0, 1)) = 0.6

        _EdgeWidth("Edge Width", Range(0, 0.5)) = 0.2
        _Softness("Softness", Range(0.01, 1)) = 0.8

        _NoiseScale("Noise Scale", Range(1, 30)) = 8
        _NoiseStrength("Noise Strength", Range(0, 1)) = 0.7
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ScreenEdgeFog"

            // 画面全体への描画なので、裏面除去や奥行き判定は不要。
            Cull Off
            ZWrite Off
            ZTest Always

            // 元の映像との合成は、下のFrag関数で行う。
            Blend Off

            HLSLPROGRAM

            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // URPの全画面描画用の頂点処理と、
            // 元の映像を読むための_BlitTextureを利用する。
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _EffectEnabled;
                float4 _FogColor;
                float _Intensity;
                float _EdgeWidth;
                float _Softness;
                float _NoiseScale;
                float _NoiseStrength;
            CBUFFER_END

            // 座標から0～1の値を作る。
            // 画像素材を使わず、もやの濃淡を生成するための処理。
            float Hash(float2 position)
            {
                return frac(
                    sin(dot(position, float2(127.1, 311.7)))
                    * 43758.5453
                );
            }

            // 格子の四隅の値を滑らかにつなぎ、連続した模様を作る。
            float ValueNoise(float2 position)
            {
                float2 cell = floor(position);
                float2 localPosition = frac(position);

                float2 blend =
                    localPosition * localPosition
                    * (3.0 - 2.0 * localPosition);

                float bottomLeft = Hash(cell);
                float bottomRight = Hash(cell + float2(1.0, 0.0));
                float topLeft = Hash(cell + float2(0.0, 1.0));
                float topRight = Hash(cell + float2(1.0, 1.0));

                return lerp(
                    lerp(bottomLeft, bottomRight, blend.x),
                    lerp(topLeft, topRight, blend.x),
                    blend.y
                );
            }

            // 大小3種類の模様を重ね、もやらしい濃淡を作る。
            // 今回は表示基盤の確認用として、模様は動かさない。
            float FogNoise(float2 position)
            {
                float large = ValueNoise(position);
                float medium =
                    ValueNoise(position * 2.03 + float2(13.1, 7.7));
                float small =
                    ValueNoise(position * 4.07 + float2(5.3, 19.2));

                return large * 0.57 + medium * 0.29 + small * 0.14;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                // Full Screen Passが渡す、エフェクト適用前の映像。
                half4 sceneColor = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    input.texcoord
                );

                // オフ・濃さ0・範囲0なら、元の映像をそのまま返す。
                if (_EffectEnabled < 0.5
                    || _Intensity <= 0.0
                    || _EdgeWidth <= 0.0)
                {
                    return sceneColor;
                }

                // 画面内の位置を0～1にする。
                float2 screenUV =
                    input.positionCS.xy / _ScaledScreenParams.xy;

                // 最も近い画面端までの距離。
                // 端で0、画面の中心では0.5になる。
                float2 edgeDistances = min(screenUV, 1.0 - screenUV);
                float edgeDistance =
                    min(edgeDistances.x, edgeDistances.y);

                float width = clamp(_EdgeWidth, 0.0001, 0.5);
                float softness = clamp(_Softness, 0.01, 1.0);

                // もやを表示する周辺領域。
                // Softnessが大きいほど、中央側の境界が柔らかくなる。
                float fadeStart = width * (1.0 - softness);
                float edgeMask =
                    1.0 - smoothstep(fadeStart, width, edgeDistance);

                // 横長画面でも模様が横に引き伸ばされないようにする。
                float aspect =
                    _ScaledScreenParams.x / _ScaledScreenParams.y;

                float2 noisePosition =
                    screenUV * float2(aspect, 1.0)
                    * max(_NoiseScale, 1.0);

                float noise = FogNoise(noisePosition);

                // Noise Strengthが0なら均一な縁取り、
                // 1に近づけるともやの濃淡が強くなる。
                float density = lerp(
                    1.0,
                    smoothstep(0.15, 0.85, noise),
                    saturate(_NoiseStrength)
                );

                float fogAmount = saturate(
                    edgeMask
                    * density
                    * _Intensity
                    * _FogColor.a
                );

                // 元の映像ともやの色を混ぜる。
                // 画面のアルファ値は元の値を維持する。
                half3 result =
                    lerp(sceneColor.rgb, _FogColor.rgb, fogAmount);

                return half4(result, sceneColor.a);
            }

            ENDHLSL
        }
    }
}
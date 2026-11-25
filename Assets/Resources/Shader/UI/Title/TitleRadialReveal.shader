Shader "UI/TitleRadialReveal"
{
    Properties
    {
        [PerRendererData] _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _RevealProgress ("Reveal Progress", Range(0,1)) = 0
        _AspectRatio ("Aspect Ratio", Float) = 1.777778
        _FeatherWidth ("Feather Width", Range(0.001,0.5)) = 0.12
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"

            struct VertexInput
            {
                float4 position : POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };
            struct FragmentInput
            {
                float4 position : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            fixed4 _Color;
            float _RevealProgress;
            float _AspectRatio;
            float _FeatherWidth;

            FragmentInput Vert(VertexInput input)
            {
                FragmentInput output;
                output.position = UnityObjectToClipPos(input.position);
                output.color = input.color * _Color;
                output.uv = input.uv;
                return output;
            }

            fixed4 Frag(FragmentInput input) : SV_Target
            {
                float2 offset = (input.uv - 0.5) * float2(_AspectRatio, 1.0);
                float maximumRadius = length(float2(_AspectRatio, 1.0)) * 0.5;
                float radius = lerp(-_FeatherWidth, maximumRadius + _FeatherWidth, _RevealProgress);
                fixed4 result = input.color;
                result.a *= smoothstep(radius, radius + _FeatherWidth, length(offset));
                return result;
            }
            ENDCG
        }
    }
}

Shader "Custom/PixelatedGlowFlicker"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("HDR Color", Color) = (1,1,1,1)
        _PixelSize ("Pixel Size", Float) = 6
        _FlickerSpeed ("Flicker Speed", Float) = 8
        _FlickerAmount ("Flicker Amount", Range(0,1)) = 0.4
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        Blend One One      // Additive blending
        ZWrite Off
        Cull Off
        Lighting Off
        Fog { Mode Off }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;
            float _PixelSize;
            float _FlickerSpeed;
            float _FlickerAmount;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float rand(float2 co)
            {
                return frac(sin(dot(co, float2(12.9898,78.233))) * 43758.5453);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Pixelation
                float2 uv = i.uv;
                uv *= _PixelSize;
                uv = floor(uv);
                uv /= _PixelSize;

                fixed4 col = tex2D(_MainTex, uv) * _Color;

                // Flicker using random + time
                float t = _Time.y * _FlickerSpeed;
                float flicker = sin(t + rand(i.uv) * 6.2831);
                flicker = 1 + flicker * _FlickerAmount;

                col.rgb *= flicker;

                return col;
            }
            ENDCG
        }
    }
}

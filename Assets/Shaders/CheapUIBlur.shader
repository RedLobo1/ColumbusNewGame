Shader "UI/BetterBlur"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "white" {}
        _Size ("Blur Size", Range(0, 10)) = 1.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float _Size;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 uv = i.uv;
                float2 res = _MainTex_TexelSize.xy * _Size;
                
                fixed4 col = tex2D(_MainTex, uv) * 0.16;
                
                col += tex2D(_MainTex, uv + float2(res.x, res.y)) * 0.15;
                col += tex2D(_MainTex, uv + float2(-res.x, res.y)) * 0.15;
                col += tex2D(_MainTex, uv + float2(res.x, -res.y)) * 0.15;
                col += tex2D(_MainTex, uv + float2(-res.x, -res.y)) * 0.15;
                
                col += tex2D(_MainTex, uv + float2(res.x * 2.0, 0)) * 0.06;
                col += tex2D(_MainTex, uv + float2(-res.x * 2.0, 0)) * 0.06;
                col += tex2D(_MainTex, uv + float2(0, res.y * 2.0)) * 0.06;
                col += tex2D(_MainTex, uv + float2(0, -res.y * 2.0)) * 0.06;
                
                return col;
            }
            ENDCG
        }
    }
}

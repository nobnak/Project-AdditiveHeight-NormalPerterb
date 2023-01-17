Shader "Hidden/Dots" {
    Properties {
    }
    SubShader {
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 _Params0;

            fixed4 frag (v2f i) : SV_Target {
                float2 screen = _ScreenParams.xy;

                float2 px = i.uv * screen;
                float2 res_fmod = fmod(px, _Params0.y);
                float2 res_step = step(res_fmod, _Params0.x);
                float c = all(res_step) ? 1 : 0;
                return float4(c, c, c, 1);
            }
            ENDCG
        }
    }
}

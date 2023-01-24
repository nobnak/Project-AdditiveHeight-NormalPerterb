Shader "Hidden/Differentiate" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Cull Off ZWrite Off ZTest Always

        CGINCLUDE
            #pragma target 5.0
            #pragma multi_compile ___ SRC_DEPTH

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

            sampler2D _MainTex;
            
            float4 frag_source (v2f i) : SV_Target {
                float4 cmain = tex2D(_MainTex, i.uv);
                float v = cmain.x;
                return float4(v, 0, 0, 0);
            }

            float4 frag_diff (v2f i) : SV_Target {
                float4 cmain = tex2D(_MainTex, i.uv);
                float v = cmain.x;
                float2 dx = float2(ddx_fine(v), ddy_fine(v));
                return float4(dx, 0, 0);
            }
        ENDCG

        // Source selection
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_source
            ENDCG
        }

        // Difference
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_diff
            ENDCG
        }
    }
}

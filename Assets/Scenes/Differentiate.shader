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
            sampler2D _PatternTex;

            float4 _MainTex_TexelSize;
            
            float4 _Params0;
            float4 _Params1;
            
            float4 frag_source (v2f i) : SV_Target {
                float4 cmain = tex2D(_MainTex, i.uv);
                float v = cmain.x;
                return float4(v, 0, 0, 0);
            }

            float4 frag_diff (v2f i) : SV_Target {
                float2 dmain = _MainTex_TexelSize.xy;

                #if false
                float v = cmain.x;
                float2 dx = float2(ddx_fine(v), ddy_fine(v));
                #else
                float4 cmain = float4(
                    tex2D(_MainTex, i.uv + float2(-dmain.x, 0)).x,
                    tex2D(_MainTex, i.uv + float2(dmain.x, 0)).x,
                    tex2D(_MainTex, i.uv + float2(0, -dmain.y)).x,
                    tex2D(_MainTex, i.uv + float2(0, dmain.y)).x);
                float2 dx = float2(cmain.y - cmain.x, cmain.w - cmain.z) * 0.5;
                #endif

                float h = dot(max(-dx, 0), 1);
                return float4(dx, h, 0);
            }
            float4 frag_pert (v2f IN) : SV_Target {
                float2 aspect = float2(_MainTex_TexelSize.z / _MainTex_TexelSize.w, 1);
                float2 aspect_inv = float2(1.0 / aspect.x, 1);

                float4 cmain = tex2D(_MainTex, IN.uv);
                float2 duv = cmain.xy * aspect_inv;

                float4 cpattern = tex2D(_PatternTex, IN.uv + duv * _Params0.z);
                //float c = pattern(IN.uv + duv * _Params0.z);
                //float4 cpattern = float4(c,c,c,1);
                return cpattern;
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

        // Perturbate
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_pert
            ENDCG
        }
    }
}

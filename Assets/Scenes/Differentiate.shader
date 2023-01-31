Shader "Hidden/Differentiate" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Cull Off ZWrite Off ZTest Always

        CGINCLUDE
            #pragma target 5.0
            #pragma multi_compile ___ SRC_DEPTH SRC_NORMAL

            #include "UnityCG.cginc"
            #include "Filter.cginc"

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
            sampler2D _CameraDepthNormalsTexture ;

            float4 _MainTex_TexelSize;
            
            float4 _Params0;
            float4 _Params1;
            
            float4 frag_source (v2f i) : SV_Target {
                float4 v = 0;

                #if defined(SRC_DEPTH)
                float4 cdepth = tex2D(_CameraDepthNormalsTexture , i.uv);
                float depth;
                float3 normal;
                DecodeDepthNormal(cdepth, depth, normal);
                v.x = 1 - depth; //Linear01Depth(depth);

                #else
                float4 cmain = tex2D(_MainTex, i.uv);
                v.x = cmain.x;
                #endif

                return v;
            }

            float4 frag_diff (v2f i) : SV_Target {
                float2 dmain = _MainTex_TexelSize.xy;
                
                float2 dx = 0;
                #if defined(SRC_NORMAL)
                float4 cdepth = tex2D(_CameraDepthNormalsTexture , i.uv);
                float depth;
                float3 normal;
                DecodeDepthNormal(cdepth, depth, normal);
                dx = normal.xy;
                if (normal.z >= 0.999) dx = 0;

                #else
                float4 dx_main, dy_main;
                Sobel(_MainTex, dmain, i.uv, dx_main, dy_main);
                dx = float2(dx_main.x, dy_main.x);
                #endif
                
                dx *= _Params0.z;
                dx = clamp(dx, -_Params0.w, _Params0.w);

                float h = dot(max(-dx, 0), 1);
                return float4(dx, h, 0);
            }
            float4 frag_pert (v2f IN) : SV_Target {
                float2 aspect = float2(_MainTex_TexelSize.z / _MainTex_TexelSize.w, 1);
                float2 aspect_inv = float2(1.0 / aspect.x, 1);

                float4 cmain = tex2D(_MainTex, IN.uv);
                float2 duv = cmain.xy * aspect_inv;

                float4 cpattern = tex2D(_PatternTex, IN.uv + duv);
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

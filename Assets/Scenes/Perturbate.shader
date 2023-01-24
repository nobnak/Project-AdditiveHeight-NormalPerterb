Shader "Hidden/Perturbate" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
        Cull Off ZWrite Off ZTest Always

        Pass {
            CGPROGRAM
            #pragma target 5.0
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

            struct Shape {
                float2 center;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            
            sampler2D _PatternTex;
            
            float4 _Params0;
            float4 _Params1;

            float smooth_f(float x0, float x1, float x){
                float s = smoothstep(x0, x1, x);
                return s;
            }

            fixed4 frag (v2f IN) : SV_Target {
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
        }
    }
}

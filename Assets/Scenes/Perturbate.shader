Shader "Hidden/Perturbate" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _Sphere ("Sphere", Vector) = (0, 0, 0, 0)
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
            float4 _Sphere;
            StructuredBuffer<Shape> _Shapes;

            fixed4 frag (v2f i) : SV_Target {

                float aspect = _ScreenParams.x / _ScreenParams.y;
                float dist = length((i.uv - _Sphere.xy) * float2(aspect, 1));
                float v = smoothstep(_Sphere.z, 0, dist);
                return float4(v, v, v, 1);
            }
            ENDCG
        }
    }
}

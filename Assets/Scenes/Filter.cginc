#ifndef __FILTER_CGINC__
#define __FILTER_CGINC__



void Sobel(sampler2D tex, float2 duv, float2 uv, out float4 dx, out float4 dy) {
	float4 c00 = tex2D(tex, uv + duv * float2(-1, -1));
	float4 c10 = tex2D(tex, uv + duv * float2( 0, -1));
	float4 c20 = tex2D(tex, uv + duv * float2( 1, -1));

	float4 c01 = tex2D(tex, uv + duv * float2(-1,  0));
	float4 c21 = tex2D(tex, uv + duv * float2( 1,  0));

	float4 c02 = tex2D(tex, uv + duv * float2(-1,  1));
	float4 c12 = tex2D(tex, uv + duv * float2( 0,  1));
	float4 c22 = tex2D(tex, uv + duv * float2( 1 , 1));

	dx = ((c20 + 2 * c21 + c22) - (c00 + 2 * c01 + c02)) / 8;
	dy = ((c02 + 2 * c12 + c22) - (c00 + 2 * c10 + c20)) / 8;
}



#endif
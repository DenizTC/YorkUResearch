Shader "Hidden/DepthMap_GuidedFilter"
{
	Properties
	{
		_MainTex("RGB (Virtual)", 2D) = "white" {}
		_RGBReal("RGB (Real)", 2D) = "white" {}
		_DepthReal("Depth Map (Real)", 2D) = "white" {}
		[MaterialToggle] _RGBMode("RGB Mode", Float) = 1

	}
	SubShader
	{
		// No culling or depth
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

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
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = v.uv;
				return o;
			}
			

			sampler2D _MainTex;
			sampler2D _RGBReal;
			uniform sampler2D _LastCameraDepthTexture;
			sampler2D _DepthReal;
			float4 _RGBReal_TexelSize;
			float4 _DepthReal_TexelSize;
			uniform float _RGBMode;
			static const int radius = 3;
			static const float sigma_s = 0.015;
			static const float sigma_c = 0.15;
			static const float filter_window = 0.1;

			float3 GuidedFilter(sampler2D guideTex, sampler2D srcTex,
				float2 gUV, float2 sUV,
				float sigma_s, float sigma_c)
			{
				float wsize = filter_window / 6.0;
				float3 res_v = float3(0.0, 0.0, 0.0);
				float res_w = 0.0;
				float3 center_g = tex2D(guideTex, gUV).rgb;

				float sigma_i = 0.5*wsize*wsize / sigma_s / sigma_s;
				for (float i = -3.0; i <= 3.0; i++) {
					for (float j = -3.0; j <= 3.0; j++) {
						float3 tmp_v = tex2D(srcTex, sUV + float2(i, j)*wsize).rgb;
						float3 tmp_g = tex2D(guideTex, gUV + float2(i, j)*wsize).rgb;
						
						float tmp_w = exp(-(i*i + j*j)*sigma_i);

						float3 diff_g = (tmp_g - center_g);
						tmp_w *= exp(-(dot(diff_g, diff_g) / 2.0 / sigma_c / sigma_c));
						
						res_v += tmp_v*tmp_w;
						res_w += tmp_w;

					}

				}
				float3 res = res_v / res_w;
				return res;
			}
			
			bool IsEdge(sampler2D tex, float2 uv, float2 texSize, float cur) {
				float bias = 0.005;
				if (cur > bias) return false;

				if (tex2D(tex, uv + float2(1, 0)*texSize).r > 0) return true;
				if (tex2D(tex, uv + float2(-1, 0)*texSize).r > 0) return true;
				if (tex2D(tex, uv + float2(0, 1)*texSize).r > 0) return true;
				if (tex2D(tex, uv + float2(0, -1)*texSize).r > 0) return true;

				return false;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed3 vRGB = tex2D(_MainTex, i.uv);
				fixed3 rRGB = tex2D(_RGBReal, (i.uv / 2.0) + 0.5);
				//float rDepth = tex2D(_DepthReal, i.uv).r;
				float vDepth = UNITY_SAMPLE_DEPTH(tex2D(_LastCameraDepthTexture, i.uv));
				vDepth = 1 - pow(Linear01Depth(vDepth), 0.5);
				
				//float2 guideSize = float2(_RGBReal_TexelSize.x, _RGBReal_TexelSize.y);
				float2 srcSize = float2(_DepthReal_TexelSize.x, _DepthReal_TexelSize.y);

				float3 result = GuidedFilter(_DepthReal, _DepthReal,i.uv, i.uv,sigma_s, sigma_c);
				//float3 result = GuidedFilter(_RGBReal, _DepthReal, (i.uv / 2.0) + 0.5, i.uv, sigma_s, sigma_c);

				//result = (IsEdge(_DepthReal, i.uv, srcSize, result.r)) ? float3(1, 0, 0) : result;
				float3 bg = lerp(result, rRGB, _RGBMode);
				float3 fg = lerp(result, vRGB, lerp(0.5, 1, _RGBMode));
				return float4((vDepth > result) ? fg : bg, 1);

			}
			ENDCG
		}
	}
}

Shader "Hidden/DepthMap_Kuwahara"
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

			float3 KawaharaFilter(sampler2D srcTex, float2 srcUV, float2 srcSize) {

				float n = float((radius + 1) * (radius + 1));

				float3 m0 = 0;
				float3 m1 = 0;
				float3 m2 = 0;
				float3 m3 = 0;

				float3 s0 = 0;
				float3 s1 = 0;
				float3 s2 = 0;
				float3 s3 = 0;
				float3 c;

				for (int j = -radius; j <= 0; ++j) {
					for (int i = -radius; i <= 0; ++i) {
						c = tex2D(srcTex, srcUV + float2(i, j) * srcSize).rgb;
						m0 += c;
						s0 += c * c;
					}
				}

				for (int j = -radius; j <= 0; ++j) {
					for (int i = 0; i <= radius; ++i) {
						c = tex2D(srcTex, srcUV + float2(i, j) * srcSize).rgb;
						m1 += c;
						s1 += c * c;
					}
				}

				for (int j = 0; j <= radius; ++j) {
					for (int i = 0; i <= radius; ++i) {
						c = tex2D(srcTex, srcUV + float2(i, j) * srcSize).rgb;
						m2 += c;
						s2 += c * c;
					}
				}

				for (int j = 0; j <= radius; ++j) {
					for (int i = -radius; i <= 0; ++i) {
						c = tex2D(srcTex, srcUV + float2(i, j) * srcSize).rgb;
						m3 += c;
						s3 += c * c;
					}
				}

				float3 result = 0;

				float min_sigma2 = 1e+2;
				m0 /= n;
				s0 = abs(s0 / n - m0 * m0);

				float sigma2 = s0.r + s0.g + s0.b;
				if (sigma2 < min_sigma2) {
					min_sigma2 = sigma2;
					result = m0;
				}

				m1 /= n;
				s1 = abs(s1 / n - m1 * m1);

				sigma2 = s1.r + s1.g + s1.b;
				if (sigma2 < min_sigma2) {
					min_sigma2 = sigma2;
					result = m1;
				}

				m2 /= n;
				s2 = abs(s2 / n - m2 * m2);

				sigma2 = s2.r + s2.g + s2.b;
				if (sigma2 < min_sigma2) {
					min_sigma2 = sigma2;
					result = m2;
				}

				m3 /= n;
				s3 = abs(s3 / n - m3 * m3);

				sigma2 = s3.r + s3.g + s3.b;
				if (sigma2 < min_sigma2) {
					min_sigma2 = sigma2;
					result = m3;
				}

				return result;
			}
			
			float3 GuidedKawaharaFilter(sampler2D guideTex, sampler2D srcTex, 
				float2 guideUV, float2 srcUV, 
				float2 guideSize, float2 srcSize) {

				float n = float((radius + 1) * (radius + 1));
				float sN = float((radius + 1) * (radius + 1));

				float3 m0 = 0; 
				float3 m1 = 0;
				float3 m2 = 0;
				float3 m3 = 0;

				float3 s0 = 0;
				float3 s1 = 0;
				float3 s2 = 0;
				float3 s3 = 0;
				float3 c;

				for (int j = -radius; j <= 0; ++j) {
					for (int i = -radius; i <= 0; ++i) {
						c = tex2D(guideTex, guideUV + float2(i, j) * guideSize).rgb;
						m0 += c;
						s0 += c * c;
					}
				}

				for (int j = -radius; j <= 0; ++j) {
					for (int i = 0; i <= radius; ++i) {
						c = tex2D(guideTex, guideUV + float2(i, j) * guideSize).rgb;
						m1 += c;
						s1 += c * c;
					}
				}

				for (int j = 0; j <= radius; ++j) {
					for (int i = 0; i <= radius; ++i) {
						c = tex2D(guideTex, guideUV + float2(i, j) * guideSize).rgb;
						m2 += c;
						s2 += c * c;
					}
				}

				for (int j = 0; j <= radius; ++j) {
					for (int i = -radius; i <= 0; ++i) {
						c = tex2D(guideTex, guideUV + float2(i, j) * guideSize).rgb;
						m3 += c;
						s3 += c * c;
					}
				}


				float min_sigma2 = 1e+2;
				m0 /= n;
				s0 = abs(s0 / n - m0 * m0);

				float3 result;

				float sigma2 = s0.r + s0.g + s0.b;
				if (sigma2 < min_sigma2) {
					min_sigma2 = sigma2;

					float3 guidedM0 = 0;
					for (int j = -radius; j <= 0; ++j) {
						for (int i = -radius; i <= 0; ++i) {
							c = tex2D(srcTex, srcUV + float2(i, j) * srcSize).rgb;
							guidedM0 += c;
						}
					}
					result = guidedM0 / n;
				}

				m1 /= n;
				s1 = abs(s1 / n - m1 * m1);

				sigma2 = s1.r + s1.g + s1.b;
				if (sigma2 < min_sigma2) {
					min_sigma2 = sigma2;

					float3 guidedM1 = 0;
					for (int j = -radius; j <= 0; ++j) {
						for (int i = 0; i <= radius; ++i) {
							c = tex2D(srcTex, srcUV + float2(i, j) * srcSize).rgb;
							guidedM1 += c;
						}
					}
					result = guidedM1 / n;
				}

				m2 /= n;
				s2 = abs(s2 / n - m2 * m2);

				sigma2 = s2.r + s2.g + s2.b;
				if (sigma2 < min_sigma2) {
					min_sigma2 = sigma2;

					float3 guidedM2 = 0;
					for (int j = 0; j <= radius; ++j) {
						for (int i = 0; i <= radius; ++i) {
							c = tex2D(srcTex, srcUV + float2(i, j) * srcSize).rgb;
							guidedM2 += c;
						}
					}
					result = guidedM2 / n;
				}

				m3 /= n;
				s3 = abs(s3 / n - m3 * m3);

				sigma2 = s3.r + s3.g + s3.b;
				if (sigma2 < min_sigma2) {
					min_sigma2 = sigma2;

					float3 guidedM3 = 0;
					for (int j = 0; j <= radius; ++j) {
						for (int i = -radius; i <= 0; ++i) {
							c = tex2D(srcTex, srcUV + float2(i, j) * srcSize).rgb;
							guidedM3 += c;
						}
					}
					result = guidedM3 / n;
				}

				return result;
			}

			bool IsEdge(sampler2D tex, float2 uv, float2 texSize, float cur) {
				if (cur > 0) return false;
				
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
				
				float3 result = KawaharaFilter(_DepthReal, i.uv, srcSize);

				//float3 result = GuidedKawaharaFilter(_RGBReal, _DepthReal,
				//	(i.uv / 2.0) + 0.5, i.uv,
				//	guideSize, srcSize);
				
				//result = (IsEdge(_DepthReal, i.uv, srcSize, result.r)) ? float3(1, 0, 0) : result;
				float3 bg = lerp(result, rRGB, _RGBMode);
				float3 fg = lerp(result, vRGB, lerp(0.5, 1, _RGBMode));
				return float4((vDepth > result) ? fg : bg, 1);

			}
			ENDCG
		}
	}
}

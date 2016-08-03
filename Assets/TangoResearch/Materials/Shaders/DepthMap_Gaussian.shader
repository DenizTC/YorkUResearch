Shader "Hidden/DepthMap_Gaussian"
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
			

			static const float Weight3x3[3][3] = {
				{ 0.095332,	0.118095,	0.095332 },
				{ 0.118095,	0.146293,	0.118095 },
				{ 0.095332,	0.118095,	0.095332 }
			};

			static const float Weight5x5[5][5] = {
				{0.003765,	0.015019,	0.023792,	0.015019,	0.003765},
				{0.015019,	0.059912,	0.094907,	0.059912,	0.015019},
				{0.023792,	0.094907,	0.150342,	0.094907,	0.023792},
				{0.015019,	0.059912,	0.094907,	0.059912,	0.015019},
				{0.003765,	0.015019,	0.023792,	0.015019,	0.003765}
			};

			

			sampler2D _MainTex;
			sampler2D _RGBReal;
			uniform sampler2D _LastCameraDepthTexture;
			sampler2D _DepthReal;
			float4 _RGBReal_TexelSize;
			float4 _DepthReal_TexelSize;
			uniform float _RGBMode;


			float3 GaussianBlur3x3(sampler2D tex, float2 uv, float2 texSize) {
				float sum = 0;
				for (int i = 0; i < 3; i++) {
					for (int j = 0; j < 3; j++) {
						sum += tex2D(tex, uv + float2(i - 1, j - 1)*texSize).r*Weight3x3[i][j];
					}
				}
				return sum;
			}

			float3 GaussianBlur5x5(sampler2D tex, float2 uv, float2 texSize) {
				float sum = 0;
				for (int i = 0; i < 5; i++) {
					for (int j = 0; j < 5; j++) {
						sum += tex2D(tex, uv + float2(i - 2, j - 2)*texSize).r*Weight5x5[i][j];
					}
				}
				return sum;
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

				float3 result = GaussianBlur3x3(_DepthReal, i.uv, srcSize);
				//float3 result = GaussianBlur5x5(_DepthReal, i.uv, srcSize);

				//result = (IsEdge(_DepthReal, i.uv, srcSize, result.r)) ? float3(1, 0, 0) : result;
				float3 bg = lerp(result, rRGB, _RGBMode);
				float3 fg = lerp(result, vRGB, lerp(0.5, 1, _RGBMode));
				return float4((vDepth > result) ? fg : bg, 1);

			}
			ENDCG
		}
	}
}

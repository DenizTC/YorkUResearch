Shader "Hidden/DepthMap_Median"
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

			float SortedWindow[25];
			void Swap(int a, int b) {
				float temp = max(SortedWindow[a], SortedWindow[b]);
				SortedWindow[a] = min(SortedWindow[a], SortedWindow[b]);
				SortedWindow[b] = temp;
			}

			float3 MedianFilter3x3(sampler2D tex, float2 uv, float2 texSize) {
				for (int i = 0; i < 3; i++) {
					for (int j = 0; j < 3; j++) {
						SortedWindow[i * 3 + j] = tex2D(tex, uv + float2(i - 1, j - 1)*texSize).r;
					}
				}
				
				// Sorting network generated from: http://pages.ripco.net/~jgamble/nw.html
				Swap(0, 1);Swap(3, 4);Swap(6, 7);
				Swap(1, 2);Swap(4, 5);Swap(7, 8);
				Swap(0, 1);Swap(3, 4);Swap(6, 7);Swap(0, 3);
				Swap(3, 6);Swap(0, 3);Swap(1, 4);
				Swap(4, 7);Swap(1, 4);Swap(2, 5);
				Swap(5, 8);Swap(2, 5);Swap(1, 3);Swap(5, 7);
				Swap(2, 6);Swap(4, 6);
				Swap(2, 4);Swap(2, 3);
				Swap(5, 6);

				return SortedWindow[4];
			}

			float3 MedianFilter5x5(sampler2D tex, float2 uv, float2 texSize) {
				for (int i = 0; i < 5; i++) {
					for (int j = 0; j < 5; j++) {
						SortedWindow[i * 5 + j] = tex2D(tex, uv + float2(i - 2, j - 2)*texSize).r;
					}
				}

				// Sorting network generated from: http://pages.ripco.net/~jgamble/nw.html
				Swap(1, 2);
				Swap(0, 2);
				Swap(0, 1);
				Swap(4, 5);
				Swap(3, 5);
				Swap(3, 4);
				Swap(0, 3);
				Swap(1, 4);
				Swap(2, 5);
				Swap(2, 4);
				Swap(1, 3);
				Swap(2, 3);
				Swap(7, 8);
				Swap(6, 8);
				Swap(6, 7);
				Swap(10, 11);
				Swap(9, 11);
				Swap(9, 10);
				Swap(6, 9);
				Swap(7, 10);
				Swap(8, 11);
				Swap(8, 10);
				Swap(7, 9);
				Swap(8, 9);
				Swap(0, 6);
				Swap(1, 7);
				Swap(2, 8);
				Swap(2, 7);
				Swap(1, 6);
				Swap(2, 6);
				Swap(3, 9);
				Swap(4, 10);
				Swap(5, 11);
				Swap(5, 10);
				Swap(4, 9);
				Swap(5, 9);
				Swap(3, 6);
				Swap(4, 7);
				Swap(5, 8);
				Swap(5, 7);
				Swap(4, 6);
				Swap(5, 6);
				Swap(13, 14);
				Swap(12, 14);
				Swap(12, 13);
				Swap(16, 17);
				Swap(15, 17);
				Swap(15, 16);
				Swap(12, 15);
				Swap(13, 16);
				Swap(14, 17);
				Swap(14, 16);
				Swap(13, 15);
				Swap(14, 15);
				Swap(19, 20);
				Swap(18, 20);
				Swap(18, 19);
				Swap(21, 22);
				Swap(23, 24);
				Swap(21, 23);
				Swap(22, 24);
				Swap(22, 23);
				Swap(18, 22);
				Swap(18, 21);
				Swap(19, 23);
				Swap(20, 24);
				Swap(20, 23);
				Swap(19, 21);
				Swap(20, 22);
				Swap(20, 21);
				Swap(12, 19);
				Swap(12, 18);
				Swap(13, 20);
				Swap(14, 21);
				Swap(14, 20);
				Swap(13, 18);
				Swap(14, 19);
				Swap(14, 18);
				Swap(15, 22);

				Swap(16, 23);

				Swap(17, 24);
				Swap(17, 23);

				Swap(16, 22);

				Swap(17, 22);

				Swap(15, 19);

				Swap(15, 18);

				Swap(16, 20);

				Swap(17, 21);

				Swap(17, 20);

				Swap(16, 18);

				Swap(17, 19);

				Swap(17, 18);

				Swap(0, 13);

				Swap(0, 12);

				Swap(1, 14);

				Swap(2, 15);

				Swap(2, 14);

				Swap(1, 12);

				Swap(2, 13);

				Swap(2, 12);

				Swap(3, 16);

				Swap(4, 17);

				Swap(5, 18);

				Swap(5, 17);

				Swap(4, 16);

				Swap(5, 16);

				Swap(3, 13);

				Swap(3, 12);

				Swap(4, 14);

				Swap(5, 15);

				Swap(5, 14);

				Swap(4, 12);

				Swap(5, 13);

				Swap(5, 12);

				Swap(6, 19);

				Swap(7, 20);

				Swap(8, 21);

				Swap(8, 20);

				Swap(7, 19);

				Swap(8, 19);

				Swap(9, 22);

				Swap(10, 23);

				Swap(11, 24);

				Swap(11, 23);

				Swap(10, 22);

				Swap(11, 22);

				Swap(9, 19);

				Swap(10, 20);

				Swap(11, 21);

				Swap(11, 20);

				Swap(10, 19);

				Swap(11, 19);

				Swap(6, 13);

				Swap(6, 12);

				Swap(7, 14);

				Swap(8, 15);

				Swap(8, 14);

				Swap(7, 12);

				Swap(8, 13);

				Swap(8, 12);

				Swap(9, 16);

				Swap(10, 17);

				Swap(11, 18);

				Swap(11, 17);

				Swap(10, 16);

				Swap(11, 16);

				Swap(9, 13);

				Swap(9, 12);

				Swap(10, 14);

				Swap(11, 15);

				Swap(11, 14);

				Swap(10, 12);

				Swap(11, 13);

				Swap(11, 12);


				return SortedWindow[12];
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

				float3 result = MedianFilter3x3(_DepthReal, i.uv, srcSize);
				//float3 result = MedianFilter5x5(_DepthReal, i.uv, srcSize);
				
				//result = (IsEdge(_DepthReal, i.uv, srcSize, result.r)) ? float3(1, 0, 0) : result;
				float3 bg = lerp(result, rRGB, _RGBMode);
				float3 fg = lerp(result, vRGB, lerp(0.5, 1, _RGBMode));
				return float4((vDepth > result) ? fg : bg, 1);

			}
			ENDCG
		}

		//UsePass "Hidden/DepthMap_NoFilter/AVERAGENEIGHBORS"

	}
}

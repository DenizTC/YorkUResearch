Shader "Hidden/DepthMap_NoFilter"
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

			bool IsEdge(sampler2D tex, float2 uv, float2 texSize, float cur) {
				if (cur > 0) return false;
				
				if (tex2D(tex, uv + float2(1, 0)*texSize).r > 0) return true;
				if (tex2D(tex, uv + float2(-1, 0)*texSize).r > 0) return true;
				if (tex2D(tex, uv + float2(0, 1)*texSize).r > 0) return true;
				if (tex2D(tex, uv + float2(0, -1)*texSize).r > 0) return true;

				return false;
			}

			float MeanNeighbors(sampler2D tex, float2 uv, float2 texSize, float original) {
				
				int count = 0;
				float sum = 0;

				for (int w = -3; w <= 3; w++) {
					for (int h = -3; h <= 3; h++) {
						float val = tex2D(tex, uv + float2(w, h)*texSize).r;
						
						count += (val > 0) ? 1 : 0;
						sum += val;
					}
				}
				
				
				return (count>0) ? sum / count : original;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				fixed3 vRGB = tex2D(_MainTex, i.uv);
				fixed3 rRGB = tex2D(_RGBReal, (i.uv / 2.0) + 0.5);
				float rDepth = tex2D(_DepthReal, i.uv).r;
				float vDepth = UNITY_SAMPLE_DEPTH(tex2D(_LastCameraDepthTexture, i.uv));
				vDepth = 1 - pow(Linear01Depth(vDepth), 0.5);
				
				//float2 guideSize = float2(_RGBReal_TexelSize.x, _RGBReal_TexelSize.y);
				float2 srcSize = float2(_DepthReal_TexelSize.x, _DepthReal_TexelSize.y);
				
				//rDepth = (vDepth > 0) ? MeanNeighbors(_DepthReal, i.uv, srcSize, rDepth) : rDepth;

				float3 bg = lerp(rDepth, rRGB, _RGBMode);
				float3 fg = lerp(rDepth, vRGB, lerp(0.5, 1, _RGBMode));
				return float4((vDepth > rDepth) ? fg : bg ,1);

			}
			ENDCG
		} 
		// Pass 1

		//Pass 
		//{
		//}
		// Pass 2


	}
}

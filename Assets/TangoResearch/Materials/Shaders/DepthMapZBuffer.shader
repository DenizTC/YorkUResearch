Shader "Hidden/DepthMapZBuffer"
{
	Properties
	{
		_MainTex ("RGB (Virtual)", 2D) = "white" {}
		_RGBReal ("RGB (Real)", 2D) = "white" {}
		//_DepthVirtual ("Depth Map (Virtual)", 2D) = "white" {}
		_DepthReal ("Depth Map (Real)", 2D) = "white" {}
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
			//sampler2D _DepthVirtual;
			uniform sampler2D _LastCameraDepthTexture;
			sampler2D _DepthReal;
			uniform int sizeNear = 2;
			uniform float _RGBMode;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 vRGB = tex2D(_MainTex, i.uv);
				fixed4 rRGB = tex2D(_RGBReal, (i.uv / 2.0) + 0.5);
				float rDepth = tex2D(_DepthReal, i.uv);
				float vDepth = UNITY_SAMPLE_DEPTH(tex2D(_LastCameraDepthTexture, i.uv));
				vDepth = 1 - pow(Linear01Depth(vDepth), 0.5);
				
				float4 v = lerp(vDepth, vRGB, _RGBMode);
				
				if (rDepth < 0.005) 
				{
					//bool neighborFound = false;
					for (int r = 0; r < 2; r++) 
					{
						for (int c = 0; c < 2; c++) 
						{
							float newDepth = tex2D(_DepthReal, i.uv + (float2(r/ 160.0, c/ 90.0)) );
							if (newDepth > rDepth) 
							{
								rDepth = newDepth;

								// Break doesn't work on android. Must return here.
								float4 bg = lerp(rDepth, rRGB, _RGBMode);
								return (vDepth > rDepth) ? v : bg;
							}
						} // For each neighbor (col)
					} // For each neighbor (row)
				} // Finding neighbor

				float4 bg = lerp(rDepth, rRGB, _RGBMode);
				return (vDepth > rDepth) ? v : bg;

			}
			ENDCG
		}
	}
}

Shader "Hidden/DepthMapZBufferDialate"
{
	Properties
	{
		_MainTex("RGB (Virtual)", 2D) = "white" {}
	_RGBReal("RGB (Real)", 2D) = "white" {}
	//_DepthVirtual ("Depth Map (Virtual)", 2D) = "white" {}
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

	v2f vert(appdata v)
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
	//uniform int sizeNear = 2;
	uniform float _RGBMode;
	uniform float _DepthWidth;
	uniform float _DepthHeight;

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 vRGB = tex2D(_MainTex, i.uv);
	fixed4 rRGB = tex2D(_RGBReal, (i.uv / 2.0) + 0.5);
	float rDepth = tex2D(_DepthReal, i.uv);
	float vDepth = UNITY_SAMPLE_DEPTH(tex2D(_LastCameraDepthTexture, i.uv));
	vDepth = 1 - pow(Linear01Depth(vDepth), 0.5);

	//float4 v = lerp(vDepth, vRGB, _RGBMode);
	int size = 3;

	if (rDepth < 0.005)
	{
		bool found = false;
		float lastDepth = 0;
		float2 curRC = float2(i.uv.x * _DepthWidth, i.uv.y * _DepthHeight);
		float lastDist;

		for (int r = -size; r < size; r++)
		{
			for (int c = -size; c < size; c++)
			{
				if (r == 0 && c == 0) continue;

				float newDepth = tex2D(_DepthReal,
					float4(i.uv + (float2(r / _DepthWidth, c / _DepthHeight)), 0, 0));

				//float newDepth = tex2D(_DepthReal,
				//	i.uv + float2(r / _DepthWidth, c / _DepthHeight) );

				if (newDepth > rDepth)
				{
					float rDist = abs(abs(r) - curRC.x);
					float cDist = abs(abs(c) - curRC.y);
					if (!found) {
						found = true;
						lastDepth = newDepth;
						lastDist = distance(float2(rDist, cDist), curRC);
						continue;
					}
					else {
						float newDist = distance(float2(rDist, cDist), curRC);
						if (newDist < lastDist) {
							lastDist = newDist;
							lastDepth = newDepth;
						}
					}

				}

			} // For each neighbor (col)
		} // For each neighbor (row)

		if (found)
			rDepth = lastDepth;
		else
			return float4(1, 0, 0, 1);
	} // Finding neighbor

	return rDepth;

	//float4 bg = lerp(rDepth, rRGB, _RGBMode);
	//return (vDepth > rDepth) ? vRGB : bg;

	}
		ENDCG
	}
	}
}

Shader "Unlit/InsideFrustum"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform float4x4 _MVTargetCam;
			
			bool in_frustum(float4x4 M, float4 p) {
				float4 Pclip = mul(M, float4(p.x, p.y, p.z, 1.0));
				return abs(Pclip.x) < Pclip.w &&
					abs(Pclip.y) < Pclip.w &&
					0 < Pclip.z &&
					Pclip.z < Pclip.w;
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				//bool visible = in_frustum(mul(UNITY_MATRIX_P, _MVTargetCam), v.vertex);
				bool visible = in_frustum(_MVTargetCam, v.vertex);
				if (visible)
					o.color = float4(0.5,0.5,0.5,1);
				else
					o.color = float4(1, 1, 0, 1);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = i.color;
				// apply fog
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
}

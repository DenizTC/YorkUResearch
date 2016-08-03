Shader "Unlit/MultiMap"
{
	Properties
	{
		_Map1 ("Map 1", 2D) = "white" {}
		_Map2 ("Map 2", 2D) = "white" {}
		[MaterialToggle] _UseMap1("UseMap1", Float) = 1

		// required for UI.Mask
		_StencilComp("Stencil Comparison", Float) = 8
		_Stencil("Stencil ID", Float) = 0
		_StencilOp("Stencil Operation", Float) = 0
		_StencilWriteMask("Stencil Write Mask", Float) = 255
		_StencilReadMask("Stencil Read Mask", Float) = 255
		_ColorMask("Color Mask", Float) = 15
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		// required for UI.Mask
		Stencil
		{
			Ref[_Stencil]
			Comp[_StencilComp]
			Pass[_StencilOp]
			ReadMask[_StencilReadMask]
			WriteMask[_StencilWriteMask]
		}
		ColorMask[_ColorMask]
		LOD 100
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

			sampler2D _Map1;
			sampler2D _Map2;
			float4 _Map1_ST;
			float4 _Map2_ST;
			uniform float _UseMap1;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.uv = (_UseMap1) ? TRANSFORM_TEX(v.uv, _Map1) : TRANSFORM_TEX(v.uv, _Map2);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 col = (_UseMap1) ? tex2D(_Map1, i.uv) : tex2D(_Map2, i.uv);
				return col;
			}
			ENDCG
		}
	}
}

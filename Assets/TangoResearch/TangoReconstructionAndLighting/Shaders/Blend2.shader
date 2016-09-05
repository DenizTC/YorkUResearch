Shader "Hidden/Blend2"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_OtherTex("Texture", 2D) = "white" {}
		[MaterialToggle] _Android("Android", Float) = 1
	}
	SubShader
	{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" }
		Cull Off 
		ZWrite Off 
		ZTest Always
		Blend SrcAlpha OneMinusSrcAlpha

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
			sampler2D _OtherTex;
			uniform float _Android;

			fixed4 frag (v2f i) : SV_Target
			{
				fixed4 colA = tex2D(_MainTex, i.uv);
				
				float2 uvB = lerp(i.uv, float2(i.uv.x, 1 - i.uv.y), 1 - _Android);
				fixed4 colB = tex2D(_OtherTex, uvB);
				
				fixed3 col = lerp(colA.rgb, colB.rgb, 1 - colA.a);

				return fixed4(col, 1);
			}
			ENDCG
		}
	}
}

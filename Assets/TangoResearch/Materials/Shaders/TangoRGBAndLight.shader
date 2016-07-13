Shader "Custom/TangoRGBAndLight" {
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
		_WireframeTex("Wireframe Texture", 2D) = "white" {}
		[MaterialToggle] _DrawWireframe("Wireframe", Float) = 1
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf ShadowOnly fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0

		sampler2D _MainTex;
		sampler2D _WireframeTex;
		uniform float _DrawWireframe;

		struct Input {
			float2 uv_MainTex : TEXCOORD0;
			float2 uv_WireframeTex;
			float4 screenPos;
		};

		inline fixed4 LightingShadowOnly(SurfaceOutput s, half3 lightDir, half atten) {
			fixed4 c;

			c.rgb = s.Albedo*atten*_LightColor0.rgb;
			c.a = s.Alpha;
			return c;
		}

		void surf(Input IN, inout SurfaceOutput o) {

			float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
			screenUV = (screenUV / 2.0) + 0.5;

			float4 col = tex2D(_MainTex, screenUV);
			float4 wire = tex2D(_WireframeTex, IN.uv_WireframeTex);
			float4 colAndWire = lerp(col, col*0.75, wire.a);

			o.Albedo = lerp(col, colAndWire, _DrawWireframe);

			//o.Albedo = float4(0.5,0.5,0.5,1);
			o.Alpha = 1.0f;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

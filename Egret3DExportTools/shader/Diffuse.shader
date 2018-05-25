Shader "Egret3D/Diffuse" 
{
	Properties 
	{
		_MainColor ("MainColor", Color) = (1,1,1,1)
		_MainTex ("MainTex", 2D) = "white" {}
		_AlphaCut("AlphaCut",float) = 0.5
	}
	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		#pragma surface surf Lambert

		sampler2D _MainTex;
		fixed4 _MainColor;
		float _AlphaCut;

		struct Input 
		{
			float2 uv_MainTex;
		};

		void surf (Input IN, inout SurfaceOutput o) 
		{
			fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _MainColor;

			clip(c.a - _AlphaCut);

			o.Albedo = c.rgb;
			o.Alpha = c.a;
		}
		ENDCG
	}

	Fallback "Legacy Shaders/VertexLit"
}

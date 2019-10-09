// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/3DPlayerShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	_Color("Main Color",Color) = (1, 1, 1, 1)
	}
		SubShader
	{
		Tags{ "Queue" = "Geometry" }
		LOD 100
		/*ZTest Always Cull Off ZWrite Off
		Fog{ Mode Off }
		Cull Off
		*/
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

	sampler2D _MainTex;
	float4 _MainTex_ST;
	float4 _Color;

	v2f vert(appdata v)
	{
		v2f o;
		o.vertex = UnityObjectToClipPos(v.vertex);
		float2 uvTemp = TRANSFORM_TEX(v.uv, _MainTex);
		o.uv.x = uvTemp.x;
		o.uv.y = uvTemp.y;

		return o;
	}

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 col = tex2D(_MainTex, i.uv) * _Color;
	return col;
	/*
	fixed4 renderTex = tex2D(_MainTex,i.uv);


	//brightness
	fixed3 finalcolor = renderTex.rgb * 1.5;


	//saturation
	fixed luminance = 0.2125 * renderTex.r + 0.7154 * renderTex.g + 0.0721 * renderTex.b;
	fixed3 luminancecolor = fixed3(luminance, luminance, luminance);
	finalcolor = lerp(luminancecolor, finalcolor, 1.1);

	//contrast

	fixed3 avgcolor = fixed3(0.5, 0.5, 0.5);
	finalcolor = lerp(avgcolor, finalcolor, 1);


	return fixed4(finalcolor, renderTex.a);
	*/
	}
		ENDCG
	}
	}
}


// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Hidden/ScreenSpaceOutlineShader"
{
	Properties
	{
		_MainTex("Texture", 2D) = "white" {}
	_BlurTex("BlurTexture", 2D) = "white" {}
	_OutlineCol("OutlineColour", Color) = (0.0,0.0,1.0,1.0)
		[Toggle] _Solid("Solid Outline", Float) = 0
		_GradientStrengthModifier("Strength Modifier", Float) = 1.0
	}
		SubShader
	{
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
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	sampler2D _MainTex;

	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 col = tex2D(_MainTex, i.uv);
	if (col.r == 1.0 && col.g == 0.0 && col.b == 1.0) {
		//col = (1.0, 0.0, 0.0, 1.0);
	}
	else {
		col.r = 0.0;
		col.g = 0.0;
		col.b = 1.0;
	}
	return col;
	}
		ENDCG
	}


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
		o.vertex = UnityObjectToClipPos(v.vertex);
		o.uv = v.uv;
		return o;
	}

	sampler2D _MainTex;
	sampler2D _BlurTex;
	float _Solid;
	fixed4 _OutlineCol;
	float _GradientStrengthModifier;
	fixed4 frag(v2f i) : SV_Target
	{
		fixed4 col;
	fixed4 blurCol = tex2D(_BlurTex, i.uv);
	fixed4 unBlurCol = tex2D(_MainTex, i.uv);
	col = _OutlineCol;
	if (_Solid) {
		if (blurCol.r == 1.0) {
			col.a = 0.0;
		}
	}
	else {
		col.a *= 1.0 - blurCol.r;
	}

	col.a *= _GradientStrengthModifier;

	if (unBlurCol.r == 0.0 && unBlurCol.g == 0.0 && unBlurCol.b == 1.0) {
		col.a = 0.0;
	}
	return col;
	}
		ENDCG
	}
	}
}
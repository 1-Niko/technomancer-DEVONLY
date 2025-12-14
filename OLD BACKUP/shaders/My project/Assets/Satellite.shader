// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'


//from http://forum.unity3d.com/threads/68402-Making-a-2D-game-for-iPhone-iPad-and-need-better-performance

Shader "Futile/Background" //Unlit Transparent Vertex Colored Additive 
{
	Properties 
	{
		_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
        _SkyMask ("Sky Mask", 2D) = "red" {}
		
	//	_PalTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		
	//	_NoiseTex ("Base (RGB) Trans (A)", 2D) = "white" {}
		
		//    _RAIN ("Rain", Range (0,1.0)) = 0.5
		//_Color ("Main Color", Color) = (1,0,0,1.5)
		//_BlurAmount ("Blur Amount", Range(0,02)) = 0.0005
	}
	
	Category 
	{
		Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
		ZWrite Off
		//Alphatest Greater 0
		Blend SrcAlpha OneMinusSrcAlpha 
		Fog { Color(0,0,0,0) }
		Lighting Off
		Cull Off //we can turn backface culling off because we know nothing will be facing backwards

		BindChannels 
		{
			Bind "Vertex", vertex
			Bind "texcoord", texcoord 
			Bind "Color", color 
		}

		SubShader   
		{
	
		
			Pass 
			{
				//SetTexture [_MainTex] 
				//{
				//	Combine texture * primary
				//}
				
				
				
CGPROGRAM
#pragma target 3.0
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
// #include "_ShaderFix.cginc"
//#pragma profileoption NumTemps=64
//#pragma profileoption NumInstructionSlots=2048

//float4 _Color;
sampler2D _MainTex;
sampler2D _PalTex;
sampler2D _LevelTex;
sampler2D _SkyMask;
#if defined(SHADER_API_PSSL)
sampler2D _GrabTexture;
#else
sampler2D _GrabTexture : register(s0);
#endif

//float _BlurAmount;


uniform float _palette;
uniform float _RAIN;
uniform float _light = 0;
uniform float4 _spriteRect;
uniform float2 _screenSize;
uniform float _fogAmount;
uniform float _waterLevel;

struct v2f {
    float4  pos : SV_POSITION;
   float2  uv : TEXCOORD0;
    float2 scrPos : TEXCOORD1;
    float4 clr : COLOR;
};

float4 _MainTex_ST;

v2f vert (appdata_full v)
{
    v2f o;
    o.pos = UnityObjectToClipPos (v.vertex);
    o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
    o.scrPos = ComputeScreenPos(o.pos);
    o.clr = v.color;
    return o;
}

half4 frag (v2f i) : SV_Target
{
	float2 textCoord = float2(floor(i.scrPos.x*_screenSize.x)/_screenSize.x, floor(i.scrPos.y*_screenSize.y)/_screenSize.y);

	textCoord.x -= _spriteRect.x;
	textCoord.y -= _spriteRect.y;

	textCoord.x /= _spriteRect.z - _spriteRect.x;
	textCoord.y /= _spriteRect.w - _spriteRect.y;

	half4 skySample = tex2D(_SkyMask, textCoord);
	
	half4 c = tex2D(_LevelTex, textCoord);
	if(c.x != 1 && c.y != 1 && c.z != 1) return half4(0,0,0,0);

	c = tex2D(_GrabTexture, half2(i.scrPos.x, i.scrPos.y));
	if (c.x > 1.0/255.0 || c.y != 0.0 || c.z != 0.0) return half4(0,0,0,0);

	if (skySample.r == 0 && skySample.g == 0 && skySample.b == 0) { return float4(0,0,0,0); }

	return float4(i.clr.r * skySample.r, i.clr.g * skySample.g, i.clr.b * skySample.b, 1);
	
	// c = tex2D(_SkyMask, i.uv);
	// // c *= i.clr;
	// 
	// return half4(textCoord.rg, c.b,1);

}
ENDCG
				
				
				
			}
		} 
	}
}
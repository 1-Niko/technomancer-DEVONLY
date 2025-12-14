// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

	
// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'

//from http://forum.unity3d.com/threads/68402-Making-a-2D-game-for-iPhone-iPad-and-need-better-performance

Shader "Futile/DistantBkgObject" //Unlit Transparent Vertex Colored Additive 
{
	Properties 
		{
			_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
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
			//#include "_ShaderFix.cginc"

			//#pragma profileoption NumTemps=64
			//#pragma profileoption NumInstructionSlots=2048

			//float4 _Color;
			sampler2D _MainTex;
			sampler2D _LevelTex;
			sampler2D _NoiseTex;
			sampler2D _NoiseTex2;
			sampler2D _CloudsTex;
			sampler2D _PalTex;
			uniform float _fogAmount;
			uniform half4 _AboveCloudsAtmosphereColor;
			uniform half4 _MultiplyColor;
			//uniform float _waterPosition;

			#if defined(SHADER_API_PSSL)
			sampler2D _GrabTexture;
			#else
			sampler2D _GrabTexture : register(s0);
			#endif

			uniform float _RAIN;

			uniform float4 _spriteRect;
			uniform float2 _screenSize;


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
			
            float random (float2 uv)
            {
                return frac(sin(dot(uv,float2(12.9898,78.233)))*43758.5453123);
            }

            float Twinkle(float timer, float offset)
            {
                return (sin(2.0 * ((timer + offset) / 512.0)) + cos(3.14159265359 * ((timer + offset) / 512.0))) / 8.0;
            }

			half4 frag (v2f i) : SV_Target
			{
				half4 colour = tex2D(_MainTex, i.uv);
				
				if (colour.r == 0 && colour.g == 0 && colour.b == 0)
				{
					return half4(0,0,0,0);
				}
				if (i.clr.a > 0 && random(i.uv) + Twinkle(i.clr.b / 23, random(i.uv) * 1000) < colour.b * i.clr.a)
				{
					if (i.clr.r == 0 && i.clr.g == 0)
					{
						return half4(1,1,1,colour.b);
					}
					else if (i.clr.r == 0 && i.clr.g == 1)
					{
						return half4(0.99, 0.4, 0.01, colour.b + Twinkle((i.clr.b + random(i.uv) * 1000) * 2, random(i.uv) * 15200));
					}
					else if (i.clr.r == 1 && i.clr.g == 0)
					{
						return half4(0.15, 0.9, 0.9, colour.b + Twinkle((i.clr.b + random(i.uv) * 1000) * 2, random(i.uv) * 12500));
					}
					else if (i.clr.r == 1 && i.clr.g == 1)
					{
						return half4(i.clr.rg,1,0);
					}
				}
				else
				{
					return half4(0,0,0,0);
				}
				return half4(0,1,1,1);
			}

			ENDCG
				
				
				
			}
		} 
	}
}

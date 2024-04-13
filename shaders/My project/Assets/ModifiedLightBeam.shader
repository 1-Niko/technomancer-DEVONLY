// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

	
// Upgrade NOTE: replaced 'samplerRECT' with 'sampler2D'

//from http://forum.unity3d.com/threads/68402-Making-a-2D-game-for-iPhone-iPad-and-need-better-performance

Shader "Futile/CustomDepth" //Unlit Transparent Vertex Colored Additive 
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
			// #include "_ShaderFix.cginc"

			//#pragma profileoption NumTemps=64
			//#pragma profileoption NumInstructionSlots=2048

			//float4 _Color;
			sampler2D _MainTex;
			sampler2D _LevelTex;
			//sampler2D _NoiseTex;
			sampler2D _PalTex;
			uniform float _fogAmount;
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
				float4 color : COLOR;
			};

			float4 _MainTex_ST;

			v2f vert (appdata_full v)
			{
				v2f o;
				o.pos = UnityObjectToClipPos (v.vertex);
				o.uv = TRANSFORM_TEX (v.texcoord, _MainTex);
				o.scrPos = ComputeScreenPos(o.pos);
				o.color = v.color;
				return o;
			}
			
			float binStep(float N)
			{
				if (N >= 0.0)
					return 1.0;
				return 0.0;
			}
			
			float extractBit(float N, int position)
			{
				return binStep(N % (2.0 * pow(2.0, position)) - pow(2.0, position));
			}
			
			float calculateValue(float a, float b, float c, float d, float e)
			{
				return (a * pow(2.0, 4.0)) + (b * pow(2.0, 3.0)) + (c * pow(2.0, 2.0)) + (d * pow(2.0, 1.0)) + (e * pow(2.0, 0.0));
			}
			
			float3 decodeFloats(float N)
			{
				float a = calculateValue(extractBit(N, 14.0), extractBit(N, 13.0), extractBit(N, 12.0), extractBit(N, 11.0), extractBit(N, 10.0)) / 31.0;
				float b = calculateValue(extractBit(N, 9.0), extractBit(N, 8.0), extractBit(N, 7.0), extractBit(N, 6.0), extractBit(N, 5.0)) / 31.0;
				float c = calculateValue(extractBit(N, 4.0), extractBit(N, 3.0), extractBit(N, 2.0), extractBit(N, 1.0), extractBit(N, 0.0)) / 31.0;
				
				return float3(a, b, c);
			}
			
			float lerp(float a, float b, float t) {
				return a * (1.0 - t) + b * t;
			}
			
			float3 whiteLerp(float3 normal, float t, float g)
			{
				g = g * 3.984375;
				t = t * g;
				
				return float3(lerp(normal.r, 1, t), lerp(normal.g, 1, t), lerp(normal.b, 1, t));
			}

			half4 frag (v2f i) : SV_Target
			{

				float2 textCoord = float2(floor(i.scrPos.x*_screenSize.x)/_screenSize.x, floor(i.scrPos.y*_screenSize.y)/_screenSize.y);

				textCoord.x -= _spriteRect.x;
				textCoord.y -= _spriteRect.y;

				textCoord.x /= _spriteRect.z - _spriteRect.x;
				textCoord.y /= _spriteRect.w - _spriteRect.y;

				half4 texcol = tex2D(_LevelTex, textCoord);
				
				float3 v = decodeFloats(i.color.a * 32767);
				
				half dpth = 1.0 - v.x;

				half terrainDpth = (((int)round(texcol.x * 255)-1) % 30)/30.0;
				if(texcol.x == 1 && texcol.y == 1 && texcol.z == 1)
					terrainDpth = 1;

				if(terrainDpth > 6.0/30.0){
					float4 grabTexCol = tex2D(_GrabTexture, half2(i.scrPos.x, i.scrPos.y));
				if (grabTexCol.x > 1.0/255.0 || grabTexCol.y != 0.0 || grabTexCol.z != 0.0)
					terrainDpth = 6.0/30.0;
				}

				if(dpth > terrainDpth)
					return float4(0,0,0,0);
				
				float3 sample = tex2D(_MainTex, i.uv).rgb;
				return half4(whiteLerp(i.color.rgb, v.z, sample.g), sample.b * v.y);
				
				//tex2D(_MainTex, i.uv).w * v.y);
				// return half4(tex2D(_MainTex, i.uv).rgb, tex2D(_MainTex, i.uv).w * v.y);
			}
			ENDCG
			}
		} 
	}
}
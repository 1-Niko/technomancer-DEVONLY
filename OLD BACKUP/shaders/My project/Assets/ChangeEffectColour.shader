Shader "Slugpack/AlternateEffectColour"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _EffectMask ("Effect Mask", 2D) = "white" {}
		
        _RGB2HSL ("RGB to HSL", 2D) = "white" {}
        _HSL2RGB ("HSL to RGB", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _ShadowMask;
            sampler2D _EffectMask;
			sampler2D _LevelTex;
			sampler2D _NoiseTex;
			sampler2D _PalTex;
			sampler2D _GrabTexture;
			
			sampler2D _RGB2HSL;
			sampler2D _HSL2RGB;
			
			uniform fixed _rimFix;
			uniform float _RAIN;
			uniform float _WetTerrain;
			uniform float _waterLevel;
			uniform float _cloudsSpeed;
			uniform float _light = 0;
			uniform float _Grime;
			uniform float _SwarmRoom;
			uniform float _fogAmount;
			uniform float _darkness;
			uniform float _contrast;
			uniform float _saturation;
			uniform float _hue;
			uniform float _brightness;
			uniform float2 _screenOffset;
			uniform float2 _LevelTex_TexelSize;

            uniform float4 _camInRoomRect;
			uniform float2 _screenSize;
			uniform float4 _spriteRect;
			uniform half4 _AboveCloudsAtmosphereColor;
			uniform float4 _lightDirAndPixelSize;
			
            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float2 scrPos : TEXCOORD1;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
                float2 scrPos : TEXCOORD1;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.scrPos = ComputeScreenPos(o.vertex);
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
			
			float4 decodeFloats(float N)
			{
				float a = calculateValue(extractBit(N, 19.0), extractBit(N, 18.0), extractBit(N, 17.0), extractBit(N, 16.0), extractBit(N, 15.0)) / 31.0;
				float b = calculateValue(extractBit(N, 14.0), extractBit(N, 13.0), extractBit(N, 12.0), extractBit(N, 11.0), extractBit(N, 10.0)) / 31.0;
				float c = calculateValue(extractBit(N, 9.0), extractBit(N, 8.0), extractBit(N, 7.0), extractBit(N, 6.0), extractBit(N, 5.0)) / 31.0;
				float d = calculateValue(extractBit(N, 4.0), extractBit(N, 3.0), extractBit(N, 2.0), extractBit(N, 1.0), extractBit(N, 0.0)) / 31.0;
				
				return float4(a, b, c, d);
			}
			
			float3 sampleColour(float3 values, bool which)
			{
				if (which) // RGB to HSL
				{
					return tex2D(_RGB2HSL, float2(values.b, ((32.0 * values.r) + values.g) / 33.0));
				}
				else // HSL to RGB
				{
					return tex2D(_HSL2RGB, float2(values.b, ((32.0 * values.r) + values.g) / 33.0));
				}
			}
			
			float CustomClamp(float value, float minValue, float maxValue) {
				return max(minValue, min(value, maxValue));
			}
			
            fixed4 frag (Interpolators i) : SV_Target
            {
				float2 textCoord = float2(floor(i.scrPos.x*_screenSize.x)/_screenSize.x, floor(i.scrPos.y*_screenSize.y)/_screenSize.y);
				
				textCoord.x -= _spriteRect.x;
				textCoord.y -= _spriteRect.y;
				
				textCoord.x /= _spriteRect.z - _spriteRect.x;
				textCoord.y /= _spriteRect.w - _spriteRect.y;
				
				// displace doesn't work quite as expected
				// not removing it because it looks fine if the effect colour in question is set to black, but its use is not recommended
				float ugh = fmod(fmod(   round(tex2D(_LevelTex, float2(textCoord.x, textCoord.y)).x*255)   , 90)-1, 30)/300.0;
				float displace = tex2D(_NoiseTex, float2((textCoord.x * 1.5) - ugh + (_RAIN*0.01), (textCoord.y*0.25) - ugh + _RAIN * 0.05)   ).x;
				displace = clamp((sin((displace + textCoord.x + textCoord.y + _RAIN*0.1) * 3 * 3.14)-0.95)*20, 0, 1);
				
				// screenPos is equal to textCoord
				
				if (_WetTerrain < 0.5 || 1-textCoord.y > _waterLevel) 
					displace = 0;

				half4 texcol = tex2D(_LevelTex, float2(textCoord.x, textCoord.y+displace*0.001));
				
				// return float4(sampleColour(float3((texHSL.r + v.g) % 1.0, CustomClamp(texHSL.g + v.b,0,1), CustomClamp(texHSL.b + v.a,0,1)), false), 1);
				// return float4(sampleColour(float3(texcol.ga,1-v.b), false), 1);
				
				if (texcol.r == 1 && texcol.g == 1 && texcol.b == 1)
				{
					return float4(0,0,0,0);
				}
				
				if (texcol.b > 0)
				{
					if ((i.color.r == 0.5 || i.color.r == 1.0) && texcol.g == 1.0/255.0) // NOT effect colour A
						return float4(0,0,0,0);
					if ((i.color.r == 0.75 || i.color.r == 1.0) && texcol.g == 2.0/255.0) // NOT effect colour B
						return float4(0,0,0,0);
						
					float4 colour = tex2D(_EffectMask, float2(texcol.r, (floor(22.0 * i.color.g) / 22.0) + ((1 - texcol.b) / 22.0))); // - 0.00071022727
				
					half dpth = (((uint)round(texcol.x * 255)-1) % 30)/30.0;

					half terrainDpth = (((uint)round(texcol.x * 255)-1) % 30)/30.0;
					if(texcol.x == 1 && texcol.y == 1 && texcol.z == 1)
						terrainDpth = 1;

					if(terrainDpth > 6.0/30.0){
						float4 grabTexCol = tex2D(_GrabTexture, half2(i.scrPos.x, i.scrPos.y));
						if (grabTexCol.x > 1.0/255.0 || grabTexCol.y != 0.0 || grabTexCol.z != 0.0)
							terrainDpth = 6.0/30.0;
					}

					if(dpth > terrainDpth)
						return float4(0,0,0,0);
					
					// Extra colour alterations
					float4 v = decodeFloats(i.color.z);
				
					if (!(v.g == 0.0 && v.b == 0.0 && v.a == 0.0))
					{
						float3 texHSL = sampleColour(colour.rgb, true);
						
						colour.rgb = sampleColour(float3((texHSL.r + v.g) % 1.0, CustomClamp(texHSL.g + v.b,0,1), CustomClamp(texHSL.b + v.a,0,1)), false);
					}
					if (v.r != 0)
					{
						float4 sample = tex2D(_PalTex, float2((texcol.r + 0.5) % 30.0/32.0, (floor((1.0 / 30.0) * texcol.r) + 2.5)/8.0));
						colour = lerp(sample, colour, 1-v.r);
					}
					
					return half4(colour.rgb, 1); // tex2D(_MainTex, i.uv).w
				}
				return float4(0,0,0,0);
            }
            ENDCG
        }
    }
}

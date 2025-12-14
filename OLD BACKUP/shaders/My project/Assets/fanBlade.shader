Shader "Custom/Fans"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ShadowMask ("Shadow Mask", 2D) = "white" {}
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        //Alphatest Greater 0
        Blend SrcAlpha OneMinusSrcAlpha
        Lighting Off
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            // #include "_ShaderFix.cginc"
            
            sampler2D _MainTex;
            sampler2D _PalTex;
            sampler2D _ShadowMask;
            
            sampler2D _LevelTex;
            sampler2D _GrabTexture;
            
            uniform float4 _spriteRect;
            uniform float2 _screenSize;
            uniform float4 _camInRoomRect;

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 colour : COLOR;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 colour : COLOR;
                float2 scrPos : TEXCOORD1;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.colour = v.colour;
                o.scrPos = ComputeScreenPos(o.vertex);
                return o;
            }

			fixed4 CalculateColourOutput(float2 uv, float lighting_offset, float depth, float4 _spriteRect, float2 _screenSize, sampler2D _PalTex)
			{
				// Main texture color calculation
				float4 mainSample = tex2D(_MainTex, uv);
				float paletteSample = ((mainSample.b == 0 ? 1.0 :
					(mainSample.b >= 0.4 && mainSample.b <= 0.6) ? 0.0 :
					mainSample.b == 1 ? 2.0 : 6.0) + lighting_offset) / 8.0;

				// Custom depth and terrain depth calculations are not needed for the blur effect.
				// They are used to determine if the pixel should be discarded, which is not the case here.
				
				// Return colourOutput
				// depth / (32.0/30.0)
				return float4(tex2D(_PalTex, float2((1 - depth) / (32.0/30.0), paletteSample)).rgb, 1);
			}

			half4 frag (Interpolators i) : SV_Target
			{
				float2 textCoord = float2(floor(i.scrPos.x*_screenSize.x)/_screenSize.x, floor(i.scrPos.y*_screenSize.y)/_screenSize.y);

				textCoord.x -= _spriteRect.x;
				textCoord.y -= _spriteRect.y;

				textCoord.x /= _spriteRect.z - _spriteRect.x;
				textCoord.y /= _spriteRect.w - _spriteRect.y;

				half4 shadowSample = tex2D(_ShadowMask, textCoord);
				
				half4 texcol = tex2D(_LevelTex, textCoord);
				
				half dpth = 1.0-i.colour.w;////lerp(1.0-i.clr.w, 1.0, texcol.x);
				
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
				
				// return shadowSample;
				
				float lighting_offset = 0.0;
							
				// float4 shadowSample = tex2D(_ShadowMask, float2(_camInRoomRect.x + _camInRoomRect.z * i.scrPos.x, _camInRoomRect.y + _camInRoomRect.w * i.scrPos.y));
				float4 shadowDiscardSample = tex2D(_MainTex, i.uv);
				if (shadowSample.r != 0.0)
					lighting_offset = 3.0;

				float4 colour_A = CalculateColourOutput(i.uv, lighting_offset, i.colour.a, _spriteRect, _screenSize, _PalTex);
				
				if (i.colour.r == 1)
				{
					if (i.colour.g == 1)
						return float4(1,1,1, tex2D(_MainTex, i.uv).a);
					else
						return float4(1,1,1, 0);
				}
				else
					return float4(colour_A.rgb, tex2D(_MainTex, i.uv).a);
			}
            ENDCG
        }
    }
}
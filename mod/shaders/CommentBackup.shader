Shader "Slugpack/ShadowMask"
{
    Properties
    {
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_ShadowMask ("Shadow Mask", 2D) = "white" {}
		_RoomSize ("Room Size", Vector) = (0, 0, 0, 0) // room.RoomRect
		_SpritePosition ("Sprite Position", Vector) = (0, 0, 0, 0) // this.pos
		_SpriteSize ("Sprite Size", Vector) = (0, 0, 0, 0) // sprite.size * scale
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
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            sampler2D _ShadowMask;

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }

			fixed4 frag (Interpolators i) : SV_Target
			{
				return float4(0,0,0,0);
			
				// float4 colour = tex2D(_LevelTex, float2(i.uv.x, i.uv.y) + float2(_TrainOffset.x * 2, _TrainOffset.y * 3.5) / _screenSize);
				// if (all(colour.rgb == 1))
				// {
				// 	return float4(0,0,0,0);
				// }
				// return float4(colour.r, colour.g, colour.b, 1);
			
				// float2 roomSize = (1 / _camInRoomRect.zw) * _screenSize.xy;
				// 
				// Vector bounds = ((roomSize.x - _TrainOffset.x - (_SpriteSize.x / 2)) / roomSize.x, (roomSize.x - _TrainOffset.x + (_SpriteSize.x / 2)) / roomSize.x, (roomSize.y - _TrainOffset.y - (_SpriteSize.y / 2)) / roomSize.y, (roomSize.y - _TrainOffset.y + (_SpriteSize.y / 2)) / roomSize.y);
				// 
				// if (bounds.x / 2 < i.uv.x < bounds.y / 2 && bounds.z / 2 < i.uv.y < bounds.w / 2)
				// {
				//     return float4(0.8,0.8,1,1);
				// }
				// return float4(0,1,0,1);

				// return float4(i.uv.x, i.uv.y, 0, 1);
	
				// float4 trainColor = tex2D(_MainTex, i.uv);
				// 
				// float2 invScreenSize = 1.0 / _screenSize.xy;
				// float maskColor = tex2D(_ShadowMask, i.uv + _TrainOffset.xy * invScreenSize).r; // You'll just want a stock float because you aren't going to really be using any color from this mask. In unity it may be a good idea to set its format to R8 in the texture import settings for this reason, save a smidge of VRAM :P
				// 
				// maskColor = saturate(maskColor + 0.1); // This is a bit arbitrary, you could achieve the same by making the texture gray instead of black.
				// trainColor.rgb *= maskColor;
				// return trainColor;
			
				//return fixed4(_camInRoomRect.xy, 0, 1);
				
				// if (i.uv.x < 0.5 || i.uv.y > 0.5)
				// {
				//     return float4(0.05, 0.05, 0.05, 1);
				// }
				// return tex2D(_MainTex, i.scrPos);
				
				// float4 colour = tex2D(_MainTex, i.uv);
				// if (colour.r == 1 && colour.g == 0 && colour.b == 0)
				// {
				// 	return tex2D(_PalTex, float2(0.0, 0.3125));
				// }
				// else if (colour.r == 0 && colour.g == 1 && colour.b == 0)
				// {
				// 	return tex2D(_PalTex, float2(0.0, 0.25));
				// }
				// else if (colour.r == 0 && colour.g == 0 && colour.b == 1)
				// {
				// 	return tex2D(_PalTex, float2(0.0, 0.1875));
				// }
			}
            ENDCG
        }
    }
}
Shader "Slugpack/ShadowMask"
{
    Properties
    {
		_MainTex ("Sprite Texture", 2D) = "white" {}
		_ShadowMask ("Shadow Mask", 2D) = "white" {}
		_RoomSize ("Room Size", Vector) = (0, 0, 0, 0) // room.RoomRect
		_SpritePosition ("Sprite Position", Vector) = (0, 0, 0, 0) // this.pos
		_SpriteSize ("Sprite Size", Vector) = (0, 0, 0, 0) // sprite.size
		_SpriteScale ("Sprite Scale", Vector) = (0, 0, 0, 0) // sprite.scale
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
			#include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _ShadowMask;
			
            sampler2D _LevelTex;
			
			Vector _RoomSize;
			Vector _SpritePosition;
			Vector _SpriteSize;
			Vector _SpriteScale;
			Vector _screenSize;
			Vector _camInRoomRect;
			
			uniform float4 _spriteRect;
			
			// Functions
			
			float2 Normalize(int corner_index, float2 sprite_position, float2 sprite_size, float2 sprite_scale, float2 room_size)
			{
				switch (corner_index)
				{
					case 0:
						return float2((sprite_position.x - ((sprite_size.x / 2) * sprite_scale.x)) / room_size.x, (sprite_position.y + ((sprite_size.y / 2) * sprite_scale.y)) / room_size.y);
						break;
					case 1:
						return float2((sprite_position.x + ((sprite_size.x / 2) * sprite_scale.x)) / room_size.x, (sprite_position.y + ((sprite_size.y / 2) * sprite_scale.y)) / room_size.y);
						break;
					case 2:
						return float2((sprite_position.x + ((sprite_size.x / 2) * sprite_scale.x)) / room_size.x, (sprite_position.y - ((sprite_size.y / 2) * sprite_scale.y)) / room_size.y);
						break;
					case 3:
						return float2((sprite_position.x - ((sprite_size.x / 2) * sprite_scale.x)) / room_size.x, (sprite_position.y - ((sprite_size.y / 2) * sprite_scale.y)) / room_size.y);
						break;
				}
			}

			float map(bool horizontal, float N, float2 sprite_position, float2 sprite_size, float2 sprite_scale, float2 room_size)
			{
				if (horizontal)
				{
					float A = Normalize(0, sprite_position, sprite_size, sprite_scale, room_size).x;
					float B = Normalize(1, sprite_position, sprite_size, sprite_scale, room_size).x;
					return (B - A) * N + B;
				}
				else
				{
					float A = Normalize(1, sprite_position, sprite_size, sprite_scale, room_size).y;
					float B = Normalize(2, sprite_position, sprite_size, sprite_scale, room_size).y;
					return (B - A) * N + B;
				}
			}

			float2 transpose(float2 uv, float2 sprite_position, float2 sprite_size, float2 sprite_scale, float2 room_size)
			{
				return float2(map(true, uv.x, sprite_position, sprite_size, sprite_scale, room_size), map(false, uv.y, sprite_position, sprite_size, sprite_scale, room_size));
			}

			// Shader

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

			fixed4 frag (Interpolators i) : SV_Target
			{
				return tex2D(_ShadowMask, float2(_camInRoomRect.x + _camInRoomRect.z * i.scrPos.x, _camInRoomRect.y + _camInRoomRect.w * i.scrPos.y));

				//return tex2D(_ShadowMask, textCoord);
				
				//return tex2D(_ShadowMask, transpose(i.uv, _SpritePosition.xy, _SpriteSize.xy, _SpriteScale.xy, _RoomSize.xy));
			}
            ENDCG
        }
    }
}
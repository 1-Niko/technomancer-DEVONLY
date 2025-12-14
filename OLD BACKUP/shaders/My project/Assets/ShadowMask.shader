Shader "Slugpack/ShadowMask"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _ShadowMask ("Shadow Mask", 2D) = "white" {}
        _Mode ("Lighting or Shading", Float) = 0
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

			float _Mode;

            uniform float4 _camInRoomRect;
            
            uniform float4 _spriteRect;
            uniform float2 _screenSize;

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
				float2 textCoord = float2(floor(i.scrPos.x*_screenSize.x)/_screenSize.x, floor(i.scrPos.y*_screenSize.y)/_screenSize.y);

				textCoord.x -= _spriteRect.x;
				textCoord.y -= _spriteRect.y;

				textCoord.x /= _spriteRect.z - _spriteRect.x;
				textCoord.y /= _spriteRect.w - _spriteRect.y;

				half4 colour = tex2D(_ShadowMask, textCoord);
				
                if (_Mode == 0) // Window Lighting
				{
					if (colour.r > 0.5)
					{
						return tex2D(_MainTex, i.uv);
					}
					return float4(0,0,0,0);	
				}
				else if (_Mode == 1) // Darkness Shading
				{
					float4 sampled_colour = tex2D(_MainTex, i.uv);
					return float4(sampled_colour.rgb, colour.g);
				}
				return float4(1,0.4117,0.7058,1); // If this colour is ever outputted then something has gone horribly wrong	
            }
            ENDCG
        }
    }
}

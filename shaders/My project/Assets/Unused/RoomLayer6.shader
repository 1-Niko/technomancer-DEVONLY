Shader "Slugpack/RoomLayer0"
{
    Properties
    {
        _InputColorA ("_InputColorA", Color) = (0.09, 0.4, 1, 1)
		_MainTex ("Sprite Texture", 2D) = "white" {}
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
			sampler2D _PalTex;
			sampler2D _LevelTex;

            float4 _InputColorA;
			uniform float4 _spriteRect;
			uniform float2 _screenSize;

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
				
				// float2 scrPos : TEXCOORD1;
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
				float4 colour = tex2D(_MainTex, i.uv);
				if (colour.r == 1 && colour.g == 0 && colour.b == 0)
				{
					return tex2D(_PalTex, float2(0.1875, 0.3125));
				}
				else if (colour.r == 0 && colour.g == 1 && colour.b == 0)
				{
					return tex2D(_PalTex, float2(0.1875, 0.25));
				}
				else if (colour.r == 0 && colour.g == 0 && colour.b == 1)
				{
					return tex2D(_PalTex, float2(0.1875, 0.1875));
				}
				return colour;
			}
            ENDCG
        }
    }
}
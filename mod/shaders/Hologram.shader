Shader "Custom/Radial"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
		_Offset ("Offset", Float) = 0
		_RandomOffset ("Randomized Offset", Float) = 0
		_Radial ("Is Radial", Int) = 1
		
		_ColourA ("ColourA", Vector) = (1, 1, 1, 1)
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
			
			float _Offset;
			float _RandomOffset;
			
			bool _Radial;

			Vector _ColourA;

            struct MeshData
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Interpolators
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float random(float2 uv)
            {
                return frac(sin(dot(uv,float2(12.9898, 78.233))) * 43758.5453123);
            }

			float radialSine(float2 uv, float offset)
			{
				return sin(3.141 * 59 * sqrt(((uv.x - 0.5) * (uv.x - 0.5)) + ((uv.y - 0.5) * (uv.y - 0.5))) - offset);
			}
			
			float scanline(float2 uv, float offset)
			{
				return sin(17 * 3.141 * (uv.y + -5 * uv.x - offset));
			}
			
			fixed4 frag (Interpolators i) : SV_Target
			{
				bool isColour = false;
				
				float4 colour = tex2D(_MainTex, i.uv);
				float4 sample = tex2D(_MainTex, i.uv);
				if (_Radial == 0 && radialSine(float2(colour.r, colour.g), _Offset) + random(float2(i.uv.x,i.uv.y + _RandomOffset)) < 0)
				{ isColour = true; }
				else if (_Radial == 1 && scanline(float2(colour.r, colour.g), _Offset / 67) + random(float2(i.uv.x,i.uv.y + _RandomOffset)) < 0)
				{ isColour = true; }
				
				if (isColour)
				{
					if (sample.b == 1)
					{
						return _ColourA;
					}
				}
				
				return float4(0,0,0,0);
			}
            ENDCG
        }
    }
}
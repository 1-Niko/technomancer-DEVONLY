Shader "Slugpack/DistancePoints"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
		_PointA ("Point A", Vector) = (0,0,0,0)
		_PointB ("Point B", Vector) = (0,0,0,0)
		
		_Colour ("Colour", Vector) = (0,0,0,0)
		
		_Width ("Width", Float) = 0
		_Height ("Height", Float) = 0
		
		_Size ("Size", Float) = 0
		_Cutoff ("Cutoff", Float) = 0
		
		_Offset ("Offset", Float) = 0
		_RandomOffset ("Randomized Offset", Float) = 0
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

            sampler2D _MainTex;
			
			Vector _PointA;
			Vector _PointB;
			
			Vector _Colour;

			float _Height;
			float _Width;
			
			float _Size;
			float _Cutoff;
			
			float _Offset;
			float _RandomOffset;

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
			
			float2 lerp(float2 pointA, float2 pointB, float t)
			{
				return float2((1 - t) * pointA.x + t * pointB.x, (1 - t) * pointA.y + t * pointB.y);
			}

			float modifiedDistance(float2 uv, float2 position, float2 scale)
			{
				float xSide = (uv.x - position.x) / scale.x;
				float ySide = (uv.y - position.y) / scale.y;
				return sqrt(xSide * xSide + ySide * ySide);
			}

			bool withinTolerance(float N, float expected, float tolerance)
			{
				return (expected / 255 - tolerance) <= N && N <= (expected / 255 + tolerance);
			}

            float random(float2 uv)
            {
                return frac(sin(dot(uv,float2(12.9898, 78.233))) * 43758.5453123);
            }
			
			float scanline(float2 uv, float offset)
			{
				return sin(17 * 3.141 * (uv.y + -5 * uv.x - offset));
			}

            fixed4 frag (Interpolators i) : SV_Target
            {	
				float4 colour = tex2D(_MainTex, i.uv);
				
				uint point_count = 600;
				for (uint j = 0; j < point_count; j++) // The point loop
				{
					float2 lerpAB = lerp(_PointA.yx, _PointB.yx, float(j) / point_count);
					if (lerpAB.x > _Cutoff)
					{
						if (modifiedDistance(float2(colour.r, colour.g), lerpAB, float2(_Width, _Height)) < _Size && scanline(float2(colour.r, colour.g), _Offset / 67) + random(float2(i.uv.x,i.uv.y + _RandomOffset)) < -0.8)
						{
							return float4(_Colour.xyz,1);
						}
					}
				}
				return float4(0,0,0,0);
            }
            ENDCG
        }
    }
}

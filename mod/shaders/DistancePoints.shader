Shader "Slugpack/DistancePoints"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
		_PointA ("Point A", Vector) = (0,0,0,0)
		_PointB ("Point B", Vector) = (0,0,0,0)
		_PointC ("Point C", Vector) = (0,0,0,0)
		
		_Width ("Width", Float) = 0
		_Height ("Height", Float) = 0
		
		_Size ("Size", Float) = 0
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
			Vector _PointC;

			float _Height;
			float _Width;
			
			float _Size;

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

			float2 localUV(float4 colour)
			{
				float2 local = float2(colour.r / 4, colour.g / 4);
				float tolerance = 0.003;
				if (colour.b == 0)
					return local;
				if (withinTolerance(colour.b, 17, tolerance))
					return float2(local.x + 0.00, local.y + 0.25);
				if (withinTolerance(colour.b, 34, tolerance))
					return float2(local.x + 0.00, local.y + 0.50);
				if (withinTolerance(colour.b, 51, tolerance))
					return float2(local.x + 0.00, local.y + 0.75);
				if (withinTolerance(colour.b, 68, tolerance))
					return float2(local.x + 0.25, local.y + 0.00);
				if (withinTolerance(colour.b, 85, tolerance))
					return float2(local.x + 0.25, local.y + 0.25);
				if (withinTolerance(colour.b, 102, tolerance))
					return float2(local.x + 0.25, local.y + 0.50);
				if (withinTolerance(colour.b, 119, tolerance))
					return float2(local.x + 0.25, local.y + 0.75);
				if (withinTolerance(colour.b, 136, tolerance))
					return float2(local.x + 0.50, local.y + 0.00);
				if (withinTolerance(colour.b, 153, tolerance))
					return float2(local.x + 0.50, local.y + 0.25);
				if (withinTolerance(colour.b, 170, tolerance))
					return float2(local.x + 0.50, local.y + 0.50);
				if (withinTolerance(colour.b, 187, tolerance))
					return float2(local.x + 0.50, local.y + 0.75);
				if (withinTolerance(colour.b, 204, tolerance))
					return float2(local.x + 0.75, local.y + 0.00);
				if (withinTolerance(colour.b, 221, tolerance))
					return float2(local.x + 0.75, local.y + 0.25);
				if (withinTolerance(colour.b, 238, tolerance))
					return float2(local.x + 0.75, local.y + 0.50);
				if (colour.b == 1)
					return float2(local.x + 0.75, local.y + 0.75);
				
				return float2(0, 0);
			}

            fixed4 frag (Interpolators i) : SV_Target
            {	
				float4 colour = tex2D(_MainTex, i.uv);
				
				uint point_count = 1500;
				for (uint j = 0; j < point_count; j++) // The point loop
				{
					float2 lerpAB = lerp(_PointA.yx, _PointB.yx, float(j) / point_count);
					float2 lerpBC = lerp(_PointB.yx, _PointC.yx, float(j) / point_count);
					float2 lerpABC = lerp(lerpAB, lerpBC, float(j) / point_count);
					
					if (modifiedDistance(localUV(colour), lerpABC, float2(_Width, _Height)) < _Size)
					{
						return float4(1,1,1,1);
					}
				}
				return float4(0,0,0,0);
				
				// float dist1 = distance(float2(colour.r, colour.g), _PointA.yx);
				// float dist2 = distance(float2(colour.r, colour.g), _PointB.yx);
				// float dist3 = distance(float2(colour.r, colour.g), _PointC.yx);
				// 
				// float minDist = min(min(dist1, dist2), dist3);
				
				// if (minDist < 0.15)
				// 	   return float4(1,1,1,1);
				// return float4(0,0,0,0);
            }
            ENDCG
        }
    }
}

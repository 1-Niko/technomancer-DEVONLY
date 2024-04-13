Shader "Slugpack/DistancePoints"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
		_RandomOffset ("Randomized Offset", Float) = 0
		
		// _Colour ("Colour", Vector) = (0,0,0,0)
		
		_PlayerPosition ("Player Position", Vector) = (0,0,0,0)
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
			
			float _RandomOffset;
			Vector _PlayerPosition;
			
			// Vector _Colour;

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
                float2 scrPos : TEXCOORD1;
            };

            Interpolators vert (MeshData v)
            {
                Interpolators o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.scrPos = ComputeScreenPos(o.vertex);
                o.color = v.color;
                return o;
            }

            float random(float2 uv)
            {
                return frac(sin(dot(uv,float2(12.9898, 78.233))) * 43758.5453123);
            }

			float warped_distance(float2 point0, float2 point1)
			{
				return 3.1415 * sqrt((4 * (point0.x - point1.x) * (point0.x - point1.x)) + ((point0.y - point1.y) * (point0.y - point1.y)));
			}

			bool within_range(float val, float min, float max)
			{
				return ((val > min) && (max >= val));
			}

			float return_values(int sampleCol, int _0, int _1, int _2, int _3, int _4, int _5, int _6, int _7)
			{
				if (sampleCol == 0) {return _0;}
				else if (sampleCol == 1) {return _1;}
				else if (sampleCol == 2) {return _2;}
				else if (sampleCol == 3) {return _3;}
				else if (sampleCol == 4) {return _4;}
				else if (sampleCol == 5) {return _5;}
				else if (sampleCol == 6) {return _6;}
				else if (sampleCol == 7) {return _7;}
				return 0;
			}

			float determine_activation(float sampleCol, float alpha)
			{
				// settings are currently across whole alpha, will have to change later for greater control
				if (within_range(alpha, 0.0, 0.125))       {return return_values(sampleCol, 1, 0, 0, 0, 0, 0, 0, 0);}
				else if (within_range(alpha, 0.125, 0.25)) {return return_values(sampleCol, 0, 1, 0, 0, 0, 0, 0, 0);}
				else if (within_range(alpha, 0.25, 0.375)) {return return_values(sampleCol, 1, 0, 1, 0, 0, 0, 0, 0);}
				else if (within_range(alpha, 0.375, 0.5))  {return return_values(sampleCol, 0, 1, 0, 1, 0, 0, 0, 0);}
				else if (within_range(alpha, 0.5, 0.625))  {return return_values(sampleCol, 1, 0, 1, 0, 1, 0, 0, 0);}
				else if (within_range(alpha, 0.625, 0.75)) {return return_values(sampleCol, 0, 1, 0, 1, 0, 1, 0, 0);}
				else if (within_range(alpha, 0.75, 0.875)) {return return_values(sampleCol, 1, 0, 1, 0, 1, 0, 1, 0);}
				else if (within_range(alpha, 0.875, 1.0))  {return return_values(sampleCol, 0, 1, 0, 1, 0, 1, 0, 1);}
				return 0;
			}

			bool val_to_bool(float N)
			{
				if (N == 1)
				{
					return true;
				}
				return false;
			}

			float DEBUG(float N)
			{
				return ((N * 256) - 121) / (136 - 121);
			}
			
            fixed4 frag (Interpolators i) : SV_Target
            {	
				float4 sample = tex2D(_MainTex, i.uv);
			
				float position = sample.g;
				
				float blue = round(sample.b * 255) - 1;
				
				for (uint j = 0; j < 3; j++)
				{
					if (blue >= 8)
					{
						position += 1;
						blue -= 8;
					}
				}
				
				if (random(sample.rg + float2(i.color.b, position / 4)) < 0.5)
				{
					return float4(1,1,1, (sin(256 * (warped_distance(_PlayerPosition.xy, i.scrPos)) - _RandomOffset) + random(float2(sample.r + _RandomOffset, sample.g + _RandomOffset))-0.5 > -0.65 && val_to_bool(determine_activation(blue, i.color.a))) ? 1 : 0);
				}
				return float4(0,0,0,0);//tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}

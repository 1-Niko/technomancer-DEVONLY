Shader "Slugpack/DistancePoints"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
		_RandomOffset ("Randomized Offset", Float) = 0
		
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

            fixed4 frag (Interpolators i) : SV_Target
            {	
				float4 sample = tex2D(_MainTex, i.uv);
				bool showColour = (sin(256 * (warped_distance(_PlayerPosition.xy, i.scrPos)) - _RandomOffset) + random(float2(sample.r + _RandomOffset, sample.g + _RandomOffset))-0.5 > 0);
				if (i.color.b != 0)
				{
					return float4(1,1,1, (showColour) ? sample.b : sample.b / 2);
				}
				else if (sample.b == 1)
				{
					return float4(1,1,1, (showColour) ? 1 : 0);
				}
				return float4(0,0,0,0);
            }
            ENDCG
        }
    }
}

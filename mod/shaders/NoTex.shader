Shader "Futile/Red"
{
    Properties
    {
        _InputColorA ("_InputColorA", Color) = (0.09, 0.4, 1, 1)
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

            float4 _InputColorA;

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
				if (i.uv.x > 0.5 && i.uv.y > 0.5)
				{
					return float4(247 / 255, 0, 214 / 255, 1);
				}
				return float4(0.1, 0.1, 0.1, 1);
			}
            ENDCG
        }
    }
}
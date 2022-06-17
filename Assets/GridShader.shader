Shader "Unlit/GridShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_GridColor ("Grid Color", Color) = (0, 0, 0, 1)
		_GridColorL ("Grid Color L", Color) = (0, 0, 0, 1)
		_Grid ("Grid", int) = 16
		_GridL ("Grid L", int) = 2
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			fixed4 _GridColor;
			fixed4 _GridColorL;
			float _Grid;
			float _GridL;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				float2 s = floor(1.001 - i.uv);
				float ss = 1 - min(s.x + s.y, 1);

				float2 p = floor(1.05 - frac(i.uv * _Grid));
				float g = min(p.x + p.y, 1);

				float2 p2 = floor(1.01 - frac(i.uv * _GridL));
				float g2 = min(p2.x + p2.y, 1);

                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
				col = lerp(col, _GridColor, g);
				col = lerp(col, _GridColorL, g2);
				return col;
            }
            ENDCG
        }
    }
}

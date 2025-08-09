Shader "Unlit/slidingTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RotationSpeed ("Rotation Speed", Float) = 1.0
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
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _RotationSpeed;

            // Function to rotate UV coordinates
            float2 rotateUV(float2 uv, float rotation, float2 pivot)
            {
                float s = sin(rotation);
                float c = cos(rotation);
                
                // Translate to pivot
                uv -= pivot;
                
                // Rotate
                float2x2 rotMatrix = float2x2(c, -s, s, c);
                uv = mul(rotMatrix, uv);
                
                // Translate back
                uv += pivot;
                
                return uv;
            }

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate rotation angle based on time
                float rotationAngle = _Time.y * _RotationSpeed;
                
                // Get screen-space UVs (normalized)
                float2 screenUV = i.screenPos.xy / i.screenPos.w;
                
                // Rotate UVs around center (0.5, 0.5)
                float2 rotatedUV = rotateUV(screenUV, rotationAngle, float2(0.5, 0.5));
                
                // Sample texture with rotated UVs
                fixed4 col = tex2D(_MainTex, rotatedUV);
                return col;
            }
            ENDCG
        }
    }
}
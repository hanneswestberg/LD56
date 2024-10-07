Shader "Unlit/GradientShader"
{
    Properties
    {
        _ColorStart("Start Color", Color) = (1, 1, 1, 1)
        _ColorEnd("End Color", Color) = (1, 1, 1, 1)
        _MainTex("Texture", 2D) = "white" {}
        _GradientSpeed("Gradient Speed", Range(0.1, 5.0)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue" = "Transparent" }
        LOD 100

        Pass
        {
            ZWrite Off
            ZTest LEqual

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 _ColorStart;
            fixed4 _ColorEnd;
            float _GradientSpeed;
            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the texture
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Apply gradient speed using a power function
                float gradientFactor = pow(i.uv.y, _GradientSpeed);
                fixed4 gradientColor = lerp(_ColorStart, _ColorEnd, gradientFactor);

                // Combine the texture color with the gradient color
                fixed4 finalColor = texColor * gradientColor;
                return finalColor;
            }
            ENDCG
        }
    }
}

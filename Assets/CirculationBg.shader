Shader "Custom/CirculationBg"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _ScrollXSpeed("X Scroll Speed", Range(-10,10)) = 2
        _ScrollYSpeed("Y Scroll Speed", Range(-10,10)) = 0
    }
        SubShader
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            ColorMask RGB
            Tags { "RenderType" = "Opaque" }
            LOD 100
            Pass
            {
                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #include "UnityCG.cginc"
                struct appdata
                {
                    float4 vertex: POSITION;
                    float2 uv: TEXCOORD0;
                };
                struct v2f  
                {
                    float2 uv: TEXCOORD0;
                    float4 vertex: SV_POSITION;
                };
                sampler2D _MainTex;
                float4 _MainTex_ST;
                float _ScrollXSpeed;
                float _ScrollYSpeed;
                v2f vert(appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                    return o;
                }
                fixed4 frag(v2f i): SV_Target
                {
                    float2 uv = i.uv;
                    float xNew = frac(_ScrollXSpeed * _Time);
                    float yNew = frac(_ScrollYSpeed * _Time);
                    uv += float2(xNew,yNew);
                    float4 col = tex2D(_MainTex, uv);
                    return col;
                }
                ENDCG
            }
        }
        Fallback "VertexLit"
}
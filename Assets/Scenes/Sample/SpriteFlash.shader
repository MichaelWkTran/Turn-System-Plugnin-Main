Shader "Unlit/SpriteFlash"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FlashColour("Flash Colour", Color) = (1, 1, 1, 1)
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 100

            HLSLINCLUDE
                CBUFFER_START
                    float4 _FlashColour;
                CBUFFER_END

                TEXTURE2D(_MainTex);
                SAMPLER(sampler_MainTex);

                struct VertexInput()
                {
                    float4 position : POSITION;
                    float4 uv : TEXCOORD0;
                };

                struct VertexOutput()
                {
                    float4 position : SV_POSITION;
                    float4 uv : TEXCOORD0;
                };

            ENDHLSL

        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment

            VertexOutput Vertex(VertexInput _input)
            {
                VertexOutput output;
                output.position = _vertexInput.position;
                output.uv = _vertexInput.uv;

                return output;
            }

            float4 Fragment(VertexOutput _input)
            {
                VertexOutput output;
                output.position = _vertexInput.position;
                output.uv = _vertexInput.uv;

                return output;
            }

            ENDHLSL

            //CGPROGRAM
            //#pragma vertex vert
            //#pragma fragment frag
            //#pragma multi_compile_fog
            //
            //#include "UnityCG.cginc"
            //
            //struct appdata
            //{
            //    float4 vertex : POSITION;
            //    float2 uv : TEXCOORD0;
            //};
            //
            //struct v2f
            //{
            //    float2 uv : TEXCOORD0;
            //    UNITY_FOG_COORDS(1)
            //    float4 vertex : SV_POSITION;
            //};
            //
            //sampler2D _MainTex;
            //float4 _MainTex_ST;
            //
            //v2f vert (appdata v)
            //{
            //    v2f o;
            //    o.vertex = UnityObjectToClipPos(v.vertex);
            //    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            //    UNITY_TRANSFER_FOG(o,o.vertex);
            //    return o;
            //}
            //
            //fixed4 frag (v2f i) : SV_Target
            //{
            //    fixed4 col = tex2D(_MainTex, i.uv);
            //    return col;
            //}
            //ENDCG
        }
    }
}

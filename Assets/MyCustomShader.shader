Shader "MyCustomShader"
{
    Properties
    {
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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
                float3 ray_pos : TEXCOORD1;
                float3 ray_dir : TEXCOORD2;
            };
            
            uniform float4x4 cam_frustum;
            uniform float4 cam_position;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                int index = v.uv.x *2 + v.uv.y;
                o.ray_pos = cam_frustum[index];
                o.ray_dir = normalize(o.ray_pos - cam_position); //_WorldSpaceCameraPos
                
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float4 color = float4(i.ray_dir, 1);                
                return color;
            }
            ENDCG
        }
    }
}

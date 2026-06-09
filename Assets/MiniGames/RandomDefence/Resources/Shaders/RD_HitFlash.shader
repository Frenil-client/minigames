// ──────────────────────────────────────────────────────────────────────────────
//  RD_HitFlash.shader
//  스프라이트 원본 색상에 관계없이 히트 플래시 색상으로 RGB를 덮어씁니다.
//  알파(투명 영역)는 유지되므로 스프라이트 외곽선이 그대로 유지됩니다.
//
//  사용법:
//   1. 이 셰이더를 사용하는 Material(RD_HitFlash_Mat)을 만들고
//      몬스터 프리팹의 각 SpriteRenderer에 할당합니다.
//   2. RD_MonsterBase가 _HitFlashAmount를 MaterialPropertyBlock으로 제어합니다.
//      (_HitFlashAmount = 0 → 정상 / 1 → 완전히 플래시 색상으로 덮임)
// ──────────────────────────────────────────────────────────────────────────────
Shader "Custom/RD_HitFlash"
{
    Properties
    {
        [PerRendererData] _MainTex       ("Sprite Texture",         2D)           = "white" {}
        _Color                           ("Tint",                   Color)        = (1,1,1,1)
        _HitFlashColor                   ("Hit Flash Color",        Color)        = (1,0,0,1)
        [PerRendererData] _HitFlashAmount("Hit Flash Amount",       Range(0,1))   = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"             = "Transparent"
            "IgnoreProjector"   = "True"
            "RenderType"        = "Transparent"
            "PreviewType"       = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull     Off
        Lighting Off
        ZWrite   Off
        Blend    SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 color  : COLOR;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4    _Color;
            fixed4    _HitFlashColor;
            float     _HitFlashAmount;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                o.color  = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * i.color;

                // 알파(스프라이트 외곽)는 유지, RGB만 플래시 색상으로 덮음
                col.rgb = lerp(col.rgb, _HitFlashColor.rgb, _HitFlashAmount);

                return col;
            }
            ENDCG
        }
    }
}

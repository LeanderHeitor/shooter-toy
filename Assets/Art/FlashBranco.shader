// FlashBranco.shader
// Pinta a silhueta do sprite de uma cor chapada, mantendo a transparencia.
//
// Por que isto existe: o "color" do SpriteRenderer MULTIPLICA a cor do desenho.
// Multiplicar por branco nao muda nada, entao nao da pra clarear um sprite por ali.
// Este material troca a cor de todos os pixels visiveis por branco, que e o flash
// de quando o inimigo leva o tiro. O EnemyDeath troca o material por 0,06s e devolve.
Shader "ShooterToy/FlashBranco"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Cor do flash", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "RenderType" = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        // Mesma mistura que o sprite normal da Unity usa (alfa pre-multiplicado).
        Blend One OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct entrada
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                fixed4 cor    : COLOR;
            };

            struct saida
            {
                float4 pos : SV_POSITION;
                float2 uv  : TEXCOORD0;
                fixed4 cor : COLOR;
            };

            sampler2D _MainTex;
            fixed4 _Color;

            saida vert(entrada v)
            {
                saida o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.cor = v.cor;
                return o;
            }

            fixed4 frag(saida i) : SV_Target
            {
                // So o alfa do desenho importa: a cor vem toda do _Color.
                fixed a = tex2D(_MainTex, i.uv).a * i.cor.a * _Color.a;
                return fixed4(_Color.rgb * a, a);
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}

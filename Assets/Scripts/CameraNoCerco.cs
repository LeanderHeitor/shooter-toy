using UnityEngine;
using Unity.Cinemachine;

// CameraNoCerco.cs
// Faz a camera parar junto com as paredes do cerco.
//
// Sem isto a camera segue o jogador ate encostar na parede e continua andando, e a
// tela mostra o mundo do lado de fora - onde nao tem inimigo, nem moeda, nem nada.
// A parede vira um risco vermelho no meio da tela em vez de uma borda. Com a camera
// parando, o limite do cerco e o limite do que o jogador enxerga: ele SENTE a caixa.
//
// O Cinemachine ja tem um componente para isso, o Confiner2D, mas ele precisa de um
// colisor com o formato da area, reconstruido a cada horda. O cerco ja publica os
// dois limites como numeros, entao aqui basta uma conta com eles.
//
// Vai no mesmo objeto da CinemachineCamera (o CamVirtual).
[DisallowMultipleComponent]
public class CameraNoCerco : CinemachineExtension
{
    // Quanto tempo a camera leva para voltar ao jogador quando o cerco abre.
    //
    // Enquanto o cerco esta fechado, a camera pode estar presa na parede com o
    // jogador longe do centro da tela. Quando a horda acaba, a trava some - e sem
    // esta suavizacao a camera pulava de uma vez para cima do jogador, ate nove
    // unidades num quadro so. Parecia que o jogador tinha sido teleportado.
    public float tempoParaSoltar = 0.35f;

    // O mesmo problema no sentido contrario: quando a horda comeca, a camera estava
    // adiantada na frente do jogador (o look-ahead) e a trava a puxava de uma vez para
    // o meio do cerco. O cenario inteiro saltava junto, e parecia teleporte de novo.
    // Agora ela desliza ate a posicao presa durante este tempo, e so depois a trava
    // fica exata. Nesse instante as paredes acabaram de surgir, entao ver um pedaco
    // do lado de fora por meio segundo nao engana ninguem.
    public float tempoParaPrender = 0.5f;

    // Para perceber o quadro em que o cerco fechou, quando foi, e onde a camera
    // estava naquele instante.
    private bool estavaFechado = false;
    private float fechouEm = -1f;
    private float correcaoAoFechar = 0f;

    // Quanto a camera esta sendo empurrada em x neste momento, e a velocidade com
    // que esse empurrao esta sumindo (o SmoothDamp precisa guardar as duas).
    private float correcao = 0f;
    private float velocidadeDaCorrecao = 0f;

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        // Depois do Body, que e quem decidiu para onde a camera vai seguindo o
        // jogador, e antes do Noise: o tremor da granada tem que continuar tremendo
        // mesmo com a camera encostada na parede.
        if (stage != CinemachineCore.Stage.Body) return;

        Vector3 posicao = state.RawPosition;

        if (Cerco.fechado)
        {
            // Metade da largura que a camera enxerga, em unidades de mundo.
            float meiaLargura = state.Lens.OrthographicSize * state.Lens.Aspect;

            float minimo = Cerco.limiteEsquerdo + meiaLargura;
            float maximo = Cerco.limiteDireito - meiaLargura;

            // Na Horda de Resistencia o cerco e mais estreito que a tela. Nao existe
            // posicao que esconda as duas paredes, entao a camera fica no meio e
            // mostra as duas: e justamente o aperto que aquela horda quer que o
            // jogador veja.
            float presa = (minimo > maximo)
                ? (Cerco.limiteEsquerdo + Cerco.limiteDireito) * 0.5f
                : Mathf.Clamp(posicao.x, minimo, maximo);

            // O cerco acabou de fechar: guarda de onde a camera parte.
            if (estavaFechado == false)
            {
                fechouEm = Time.time;
                correcaoAoFechar = correcao;
            }

            float alvo = presa - posicao.x;
            float andamento = (tempoParaPrender > 0f) ? (Time.time - fechouEm) / tempoParaPrender : 1f;

            // Mistura entre a correcao de antes e a trava, indo de 0 a 1 no tempo
            // pedido. Nao da para usar um SmoothDamp aqui: o jogador continua andando,
            // o alvo foge, e quando a janela acabasse sobraria um degrau - o mesmo
            // salto de antes, so que meio segundo depois. A mistura chega em 1
            // exatamente no fim, entao a passagem para a trava exata nao aparece.
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(andamento));
            correcao = Mathf.Lerp(correcaoAoFechar, alvo, t);
            velocidadeDaCorrecao = 0f;
        }
        else if (deltaTime < 0f)
        {
            // Corte de camera (cena recarregando, por exemplo): nada para suavizar.
            correcao = 0f;
            velocidadeDaCorrecao = 0f;
        }
        else
        {
            // Cerco aberto: o empurrao vai sumindo aos poucos, e a camera desliza
            // de volta ate o jogador em vez de saltar.
            correcao = Mathf.SmoothDamp(correcao, 0f, ref velocidadeDaCorrecao,
                                        tempoParaSoltar, Mathf.Infinity, deltaTime);
        }

        estavaFechado = Cerco.fechado;

        posicao.x = posicao.x + correcao;
        state.RawPosition = posicao;
    }
}

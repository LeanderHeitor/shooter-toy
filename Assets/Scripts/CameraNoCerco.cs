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

            // Com o cerco fechado a trava e exata: a camera nunca mostra o lado de
            // fora da parede, nem por um quadro.
            correcao = presa - posicao.x;
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

        posicao.x = posicao.x + correcao;
        state.RawPosition = posicao;
    }
}

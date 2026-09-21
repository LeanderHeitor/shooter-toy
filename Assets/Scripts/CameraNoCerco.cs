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
    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        // Depois do Body, que e quem decidiu para onde a camera vai seguindo o
        // jogador, e antes do Noise: o tremor da granada tem que continuar tremendo
        // mesmo com a camera encostada na parede.
        if (stage != CinemachineCore.Stage.Body) return;
        if (Cerco.fechado == false) return;

        // Metade da largura que a camera enxerga, em unidades de mundo.
        float meiaLargura = state.Lens.OrthographicSize * state.Lens.Aspect;

        float minimo = Cerco.limiteEsquerdo + meiaLargura;
        float maximo = Cerco.limiteDireito - meiaLargura;

        Vector3 posicao = state.RawPosition;

        // Na Horda de Resistencia o cerco e mais estreito que a tela. Nao existe
        // posicao que esconda as duas paredes, entao a camera fica no meio e mostra
        // as duas: e justamente o aperto que aquela horda quer que o jogador veja.
        if (minimo > maximo) posicao.x = (Cerco.limiteEsquerdo + Cerco.limiteDireito) * 0.5f;
        else posicao.x = Mathf.Clamp(posicao.x, minimo, maximo);

        state.RawPosition = posicao;
    }
}

using UnityEngine;

// GroundScroller.cs
// O chao e um colisor invisivel na linha do mato. Como o mundo nao acaba,
// em vez de fabricar pedaco de chao eu simplesmente mantenho esse colisor
// sempre embaixo da camera. Pro jogador da no mesmo: o chao nunca termina.
//
// A ordem de execucao alta existe porque este script LE a posicao da camera, e quem
// ESCREVE nela e o Cinemachine, tambem no LateUpdate. Entre dois LateUpdate sem
// ordem declarada a Unity nao promete quem vem primeiro, e se este vier antes ele
// usa a posicao do quadro passado: o chao fica sempre um quadro atrasado em relacao
// a tela, que e tremor.
[DefaultExecutionOrder(1000)]
public class GroundScroller : MonoBehaviour
{
    private Transform cam;

    void Start()
    {
        if (Camera.main != null) cam = Camera.main.transform;
    }

    void LateUpdate()
    {
        if (cam == null) return;
        transform.position = new Vector3(cam.position.x, transform.position.y, transform.position.z);
    }
}

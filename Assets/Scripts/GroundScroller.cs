using UnityEngine;

// GroundScroller.cs
// O chao e um colisor invisivel na linha do mato. Como o mundo nao acaba,
// em vez de fabricar pedaco de chao eu simplesmente mantenho esse colisor
// sempre embaixo da camera. Pro jogador da no mesmo: o chao nunca termina.
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

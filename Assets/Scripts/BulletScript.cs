using UnityEngine;

// BulletScript.cs
// A bala anda numa direcao e se destroi ao sair da tela.
// E a mesma ideia do PipeMoveScript do Flappy Bird (mover + deadZone pra sumir).
public class BulletScript : MonoBehaviour
{
    // Velocidade da bala (unidades por segundo).
    public float moveSpeed = 14f;

    // Direcao: 1 = direita, -1 = esquerda. Quem define isso e o PlayerScript ao atirar.
    public float direction = 1f;

    // Se a bala passar disso pros lados, ela some (pra nao ficar viva pra sempre).
    public float deadZoneX = 30f;

    void Update()
    {
        // Anda pra frente na direcao escolhida (direita ou esquerda).
        transform.position = transform.position + (Vector3.right * direction * moveSpeed * Time.deltaTime);

        // Saiu da tela (muito pra esquerda ou muito pra direita)? Se destroi.
        if (transform.position.x > deadZoneX || transform.position.x < -deadZoneX)
        {
            Destroy(gameObject);
        }
    }
}

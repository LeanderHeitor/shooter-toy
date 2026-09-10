using UnityEngine;

// BulletScript.cs
// A bala anda numa direcao, mata quem ela acerta e some quando fica longe demais.
// O mesmo script serve pra bala do player e pra bala do fuzileiro: muda so o "isBalaInimiga".
public class BulletScript : MonoBehaviour
{
    // Velocidade da bala (unidades por segundo).
    public float moveSpeed = 14f;

    // Direcao: 1 = direita, -1 = esquerda. Quem define isso e quem atira.
    public float direction = 1f;

    // false = bala do player (mata inimigo). true = bala do inimigo (mata o player).
    public bool isBalaInimiga = false;

    // Depois de andar isto tudo sem acertar ninguem, a bala se destroi sozinha.
    public float alcance = 25f;

    // Onde a bala nasceu, pra medir o alcance (o mundo e infinito, entao nao da pra usar x fixo).
    private float origemX;

    void Start()
    {
        origemX = transform.position.x;
    }

    void Update()
    {
        // Anda pra frente na direcao escolhida (direita ou esquerda).
        transform.position += Vector3.right * direction * moveSpeed * Time.deltaTime;

        // Andou demais? Se destroi, pra nao ficar viva pra sempre.
        if (Mathf.Abs(transform.position.x - origemX) > alcance)
        {
            Destroy(gameObject);
        }
    }

    // O colisor da bala e um gatilho: acerta o alvo sem empurrar as coisas.
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isBalaInimiga)
        {
            // Bala do inimigo: so mata o player.
            PlayerScript player = other.GetComponent<PlayerScript>();
            if (player == null) return;
            player.Morrer();
        }
        else
        {
            // Bala do player: so mata inimigo (os dois tipos tem o EnemyDeath).
            EnemyDeath inimigo = other.GetComponent<EnemyDeath>();
            if (inimigo == null) return;
            inimigo.Morrer();
        }
        Destroy(gameObject);
    }
}

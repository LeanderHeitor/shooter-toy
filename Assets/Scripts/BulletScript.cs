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

    // Subida por segundo. Zero na pistola; e o que abre o leque da shotgun, com um
    // chumbo subindo, um reto e um descendo.
    public float velocidadeVertical = 0f;

    // Maior que zero, a bala e um rocket: no primeiro acerto ela explode e mata todo
    // inimigo dentro deste raio, e nao so quem ela tocou.
    public float raioDaExplosao = 0f;

    // Onde a bala nasceu, pra medir o alcance (o mundo e infinito, entao nao da pra usar x fixo).
    private float origemX;

    void Start()
    {
        origemX = transform.position.x;
    }

    void Update()
    {
        // Anda pra frente na direcao escolhida (direita ou esquerda).
        transform.position += ((Vector3.right * direction * moveSpeed) + (Vector3.up * velocidadeVertical)) * Time.deltaTime;

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
            // Dois chumbos entrando no mesmo inimigo no mesmo quadro: o segundo
            // passa reto em vez de sumir num corpo que ja esta caindo.
            if (inimigo.isMorto) return;

            if (raioDaExplosao > 0f) Explodir();
            else inimigo.Morrer();
        }
        Destroy(gameObject);
    }

    // Mata todo mundo em volta do ponto do acerto. Diferente da granada, paga moeda
    // normalmente: o rocket ja foi pago na loja, tiro por tiro.
    void Explodir()
    {
        Vector3 centro = transform.position;

        GameObject[] vivos = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < vivos.Length; i++)
        {
            // So a distancia no chao: um inimigo pulando em cima do outro continua
            // "perto", e medir em duas dimensoes deixaria o raio injusto nessa hora.
            if (Mathf.Abs(vivos[i].transform.position.x - centro.x) > raioDaExplosao) continue;

            EnemyDeath ed = vivos[i].GetComponent<EnemyDeath>();
            if (ed != null) ed.Morrer();
        }

        Explosao.Criar(centro, raioDaExplosao);
    }
}

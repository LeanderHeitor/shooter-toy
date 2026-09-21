using UnityEngine;

// EnemyKnife.cs
// Rebelde da faca: corre reto na direcao do player e mata no encostao.
// O golpe nao tem animacao de proposito: encostar e golpear sao o mesmo instante.
public class EnemyKnife : MonoBehaviour
{
    public float moveSpeed = 3.2f;
    public EnemyDeath morte;   // arraste o EnemyDeath do proprio inimigo

    // Perto o bastante pra ele parar de se ajustar. Sem esta folga o rebelde
    // ultrapassa o player por um triz num quadro e volta no seguinte, e o espelho do
    // desenho troca de lado a cada quadro - e o que fazia ele parecer girando em cima
    // do corpo. Nao e correcao visual: ele realmente estava indo e voltando.
    public float distanciaQueBasta = 0.15f;

    private Transform alvo;
    private PlayerScript alvoScript;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (morte == null) morte = GetComponent<EnemyDeath>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            alvo = player.transform;
            alvoScript = player.GetComponent<PlayerScript>();
        }
    }

    void Update()
    {
        if (morte != null && morte.isMorto == true) return;
        if (alvo == null) return;

        // Player morto e alvo encerrado: ele para onde esta. Perseguir cadaver nao
        // acrescenta nada e ainda atrapalha a leitura da tela de morte.
        if (alvoScript != null && alvoScript.isMorto == true) return;

        float distancia = alvo.position.x - transform.position.x;
        if (Mathf.Abs(distancia) <= distanciaQueBasta) return;

        // Anda sempre pro lado onde o player esta.
        float direcao = (distancia > 0f) ? 1f : -1f;
        transform.position = transform.position + (Vector3.right * direcao * moveSpeed * Time.deltaTime);

        // As folhas dos rebeldes foram recortadas olhando pra ESQUERDA (ao contrario do Tarma),
        // entao espelha quando ele anda pra direita.
        if (spriteRenderer != null) spriteRenderer.flipX = (direcao > 0f);
    }

    // Encostou no player? Mata. Uso o Stay tambem caso o player apareca ja colado nele.
    private void OnTriggerEnter2D(Collider2D other) { Encostou(other); }
    private void OnTriggerStay2D(Collider2D other)  { Encostou(other); }

    void Encostou(Collider2D other)
    {
        if (morte != null && morte.isMorto == true) return;

        PlayerScript player = other.GetComponent<PlayerScript>();
        if (player != null) player.Morrer();
    }
}

using UnityEngine;

// EnemyKnife.cs
// Rebelde da faca: corre reto na direcao do player e mata no encostao.
// O golpe nao tem animacao de proposito: encostar e golpear sao o mesmo instante.
public class EnemyKnife : MonoBehaviour
{
    public float moveSpeed = 3.2f;
    public EnemyDeath morte;   // arraste o EnemyDeath do proprio inimigo

    private Transform alvo;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (morte == null) morte = GetComponent<EnemyDeath>();

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) alvo = player.transform;
    }

    void Update()
    {
        if (morte != null && morte.isMorto == true) return;
        if (alvo == null) return;

        // Anda sempre pro lado onde o player esta.
        float direcao = (alvo.position.x > transform.position.x) ? 1f : -1f;
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

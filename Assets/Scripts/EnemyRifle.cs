using UnityEngine;

// EnemyRifle.cs
// Rebelde do fuzil: entra andando, para quando chega perto o bastante e fica atirando.
public class EnemyRifle : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float distanciaDeTiro = 6f;   // para de andar quando fica a esta distancia do player
    public float tirosPorSegundo = 0.5f; // um tiro a cada 2s, sem espera extra antes do primeiro

    public GameObject enemyBulletPrefab;  // arraste o prefab EnemyBullet
    public Transform firePoint;           // ponto de onde a bala sai (filho do inimigo)
    public EnemyDeath morte;              // arraste o EnemyDeath do proprio inimigo

    private Transform alvo;
    private SpriteRenderer spriteRenderer;
    private float proximoTiro = 0f;

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

        // Sempre encara o player.
        float direcao = (alvo.position.x > transform.position.x) ? 1f : -1f;
        // O desenho do rebelde olha pra ESQUERDA, entao o espelho e ao contrario do Tarma.
        if (spriteRenderer != null) spriteRenderer.flipX = direcao > 0f;

        // O firePoint acompanha o lado pra bala sair da arma certa.
        if (firePoint != null)
        {
            firePoint.localPosition = new Vector3(direcao * 0.7f, firePoint.localPosition.y, 0f);
        }

        float distancia = Mathf.Abs(alvo.position.x - transform.position.x);
        bool isAtirando = (distancia <= distanciaDeTiro);

        if (isAtirando == false)
        {
            // Ainda longe: anda em direcao ao player.
            transform.position += Vector3.right * direcao * moveSpeed * Time.deltaTime;
        }
        else if (Time.time >= proximoTiro)
        {
            Atirar(direcao);
            proximoTiro = Time.time + (1f / tirosPorSegundo);
        }

        if (morte != null && morte.animator != null)
        {
            morte.animator.SetBool("isAtirando", isAtirando);
        }
    }

    void Atirar(float direcao)
    {
        if (enemyBulletPrefab == null || firePoint == null) return;

        // Toca uma vez por bala, sem atrasar o disparo para esperar a animacao.
        if (morte != null && morte.animator != null) morte.animator.Play("Shoot", 0, 0f);

        GameObject bala = Instantiate(enemyBulletPrefab, firePoint.position, Quaternion.identity);
        BulletScript bs = bala.GetComponent<BulletScript>();
        if (bs != null)
        {
            bs.isBalaInimiga = true;
            bs.direction = direcao;
        }
    }
}

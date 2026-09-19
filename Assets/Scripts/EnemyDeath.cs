using UnityEngine;

// EnemyDeath.cs
// Pedacinho de morte que os DOIS inimigos usam (faca e fuzil): eles morrem com um tiro so.
// Fica separado porque a bala precisa de um jeito unico de matar qualquer inimigo.
public class EnemyDeath : MonoBehaviour
{
    public Animator animator;          // arraste o Animator do inimigo
    public Collider2D meuColisor;      // arraste o colisor, pra desligar quando morrer
    public float tempoAteSumir = 1.2f; // deixa a animacao de morte terminar antes de apagar

    // ----- FLASH BRANCO -----
    // Como o inimigo morre com um tiro so, este flash nao e aviso de dano: e o impacto
    // do abate, e o unico retorno visual que o jogo da quando voce acerta.
    // Ele tem que ser curto pra nao esconder a animacao de morte, que comeca no mesmo quadro.
    public float tempoDoFlash = 0.06f;

    // ----- MOEDA -----
    // Nem todo abate paga: matar nao e o mesmo que ser pago. Os 75% existem para que
    // o jogador nao consiga contar com o dinheiro do proximo tiro antes de atirar.
    // E campo ajustavel de proposito: se em playtest a economia ficar sovina, este e
    // o primeiro numero a subir.
    public float chanceDeSoltar = 0.75f;

    // Quantas moedas caem quando cai. O rebelde do fuzil solta mais que o da faca
    // por ser o mais perigoso dos dois - quem arrisca mais, recebe mais.
    public int moedasQueSolta = 1;

    // Os scripts de comportamento (EnemyKnife / EnemyRifle) olham isto pra parar de andar.
    public bool isMorto = false;

    private SpriteRenderer spriteRenderer;
    private Material materialNormal;
    private static Material materialDeFlash;   // vem de Assets/Resources, um pra todos
    private static GameObject moedaPrefab;     // idem: evita arrastar nos dois prefabs

    void Start()
    {
        if (animator == null) animator = GetComponent<Animator>();
        if (meuColisor == null) meuColisor = GetComponent<Collider2D>();

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) materialNormal = spriteRenderer.sharedMaterial;

        // Resources.Load acha o material sem precisar arrastar nada no Inspector,
        // entao os prefabs dos dois inimigos continuam como estavam.
        if (materialDeFlash == null) materialDeFlash = Resources.Load<Material>("FlashBranco");
    }

    void RestaurarMaterial()
    {
        if (spriteRenderer != null && materialNormal != null)
        {
            spriteRenderer.sharedMaterial = materialNormal;
        }
    }

    public void Morrer()
    {
        if (isMorto == true) return;
        isMorto = true;
        gameObject.tag = "Untagged"; // libera a vaga de vivo no Spawner durante a animacao

        if (animator != null) animator.SetBool("isMorto", true);
        if (meuColisor != null) meuColisor.enabled = false;

        Piscar();
        GameManager.ContarAbate();
        SoltarMoedas();

        Destroy(gameObject, tempoAteSumir);
    }

    // As moedas nascem onde o inimigo caiu, nao no jogador: e a ida ate elas que
    // custa alguma coisa, e e essa ida que o cerco transforma em risco.
    void SoltarMoedas()
    {
        if (moedaPrefab == null) moedaPrefab = Resources.Load<GameObject>("Moeda");
        if (moedaPrefab == null) return;

        if (Random.value > chanceDeSoltar) return;

        // Bonus de sequencia: a cada 5 abates encadeados, uma moeda a mais por abate.
        // E o que paga o jogador por continuar avancando - a sequencia morre sozinha
        // se ele parar ou recuar, entao o bonus premia exatamente o comportamento
        // que o jogo quer.
        int quantas = moedasQueSolta + (GameManager.sequencia / 5);

        // O transform do inimigo fica nos PES dele e o desenho da moeda e centrado,
        // entao sem esta subida ela nasceria metade enterrada no chao.
        Vector3 onde = transform.position + new Vector3(0f, 0.25f, 0f);

        for (int i = 0; i < quantas; i++)
        {
            Instantiate(moedaPrefab, onde, Quaternion.identity);
        }
    }

    // Troca o material por um que pinta a silhueta inteira de branco.
    // Uso sharedMaterial (e nao material) pra nao criar uma copia por inimigo.
    void Piscar()
    {
        if (spriteRenderer == null || materialDeFlash == null) return;

        spriteRenderer.sharedMaterial = materialDeFlash;
        Invoke(nameof(RestaurarMaterial), tempoDoFlash);
    }
}

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

    // De onde o sangue sai, medido a partir dos pes do rebelde.
    public float alturaDoPeito = 0.7f;

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
        Espirrar();
        GameManager.ContarAbate();
        SoltarMoedas();

        Destroy(gameObject, tempoAteSumir);
    }

    // As moedas nascem onde o inimigo caiu, nao no jogador: e a ida ate elas que
    // custa alguma coisa, e e essa ida que o cerco transforma em risco.
    void SoltarMoedas()
    {
        if (Random.value > chanceDeSoltar) return;

        // Quanto este inimigo vale, mais o que a sequencia esta pagando por abate.
        // A regra do bonus mora no GameManager, que e quem define a sequencia: aqui
        // o assunto e a morte de um rebelde, nao a economia do jogo.
        int quantas = moedasQueSolta + GameManager.BonusDeSequencia();

        // O transform do inimigo fica nos PES dele e o desenho da moeda e centrado,
        // entao sem esta subida ela nasceria metade enterrada no chao.
        Moeda.Soltar(transform.position + new Vector3(0f, 0.25f, 0f), quantas);
    }

    // O sangue espirra para longe do jogador, que e de onde o tiro veio.
    // Sai da altura do peito: o transform fica nos pes, e sangue saindo do chao
    // pareceria respingo de poca, nao de tiro.
    void Espirrar()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        bool paraDireita = (player == null) || (transform.position.x >= player.transform.position.x);

        Sangue.Soltar(transform.position + new Vector3(0f, alturaDoPeito, 0f), paraDireita);
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

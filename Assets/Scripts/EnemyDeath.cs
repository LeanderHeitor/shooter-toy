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
        GameManager.ContarAbate();

        Destroy(gameObject, tempoAteSumir);
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

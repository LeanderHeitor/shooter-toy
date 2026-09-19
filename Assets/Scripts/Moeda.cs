using UnityEngine;

// Moeda.cs
// A moeda largada por um inimigo morto.
//
// Ela NAO e um credito que cai na conta do jogador: e um objeto no chao, com prazo
// de validade. Quem quiser o dinheiro precisa ir buscar, e ir buscar quase sempre
// significa andar para dentro do tiroteio em vez de ficar atirando de longe.
// E por isso que o prazo existe - sem ele daria para limpar a horda de longe e
// recolher tudo com calma depois, e a moeda deixaria de custar coragem.
public class Moeda : MonoBehaviour
{
    public int valor = 1;

    // Quanto tempo ela fica no chao antes de sumir sozinha.
    public float tempoDeVida = 7f;

    // Comeca a piscar quando faltar isto, para o jogador conseguir decidir se ainda
    // da tempo de correr ate ela. Sem o aviso, sumir vira surpresa em vez de escolha.
    public float avisoAntesDeSumir = 2.5f;

    // Espalhamento no nascimento, para duas moedas do mesmo inimigo nao ficarem uma
    // exatamente em cima da outra.
    public float espalhamentoHorizontal = 0.6f;

    // So tem efeito se o prefab tiver Rigidbody2D. Hoje ele NAO tem: moeda com
    // fisica precisaria de camada de colisao propria para nao empurrar o jogador,
    // e ela nasce ja no chao (o transform do inimigo fica nos pes dele). O campo
    // fica aqui para o dia em que o pulinho valer o trabalho.
    public float impulsoAoNascer = 3.5f;

    private float nasceuEm;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        nasceuEm = Time.time;
        spriteRenderer = GetComponent<SpriteRenderer>();

        float lado = Random.Range(-espalhamentoHorizontal, espalhamentoHorizontal);

        Rigidbody2D corpo = GetComponent<Rigidbody2D>();
        if (corpo != null)
        {
            corpo.linearVelocity = new Vector2(lado, impulsoAoNascer);
        }
        else
        {
            // Sem fisica o espalhamento e feito na mao, uma vez so.
            transform.position = transform.position + new Vector3(lado, 0f, 0f);
        }
    }

    void Update()
    {
        float idade = Time.time - nasceuEm;

        if (idade >= tempoDeVida)
        {
            Destroy(gameObject);
            return;
        }

        if (spriteRenderer == null) return;

        // O pisca acelera conforme o prazo acaba: a pressa e informacao, nao enfeite.
        float quantoFalta = tempoDeVida - idade;
        if (quantoFalta <= avisoAntesDeSumir)
        {
            float frequencia = Mathf.Lerp(22f, 6f, quantoFalta / avisoAntesDeSumir);
            spriteRenderer.enabled = Mathf.Sin(Time.time * frequencia) > -0.3f;
        }
        else
        {
            spriteRenderer.enabled = true;
        }
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player") == false) return;

        GameManager.GanharMoedas(valor);
        Destroy(gameObject);
    }
}

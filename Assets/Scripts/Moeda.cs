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
    // exatamente em cima da outra. Quem le este campo e o Soltar, no prefab.
    public float espalhamentoHorizontal = 0.6f;

    // Distancia entre o centro de duas moedas do mesmo abate. A moeda mede cerca de
    // 0,47 de largura, entao com 0,6 sobra um vao visivel entre elas.
    private const float espacoEntreMoedas = 0.6f;

    // Os quadros da moeda girando (Art/Characters/Coin_Spin). E um giro so, sem
    // estados nem transicoes, entao um Animator com controller e clip seria tres
    // arquivos para fazer o que um vetor e uma divisao fazem aqui. O giro nao e
    // enfeite: e o brilho que faz o olho achar a moeda no meio do tiroteio.
    public Sprite[] quadros;
    public float quadrosPorSegundo = 12f;

    // O prefab, carregado uma vez por sessao. Fica aqui, e nao em quem mata: saber
    // onde mora o arquivo da moeda e assunto da moeda. Quando existir outra fonte de
    // dinheiro no jogo (bau, troco da loja), ela chama Soltar e nao repete nada.
    private static GameObject prefab;

    // O pulo ao nascer. Moeda que ja nasce no chao parece brotar dele; subindo e
    // caindo, ela sai do rebelde, e o olho acompanha o voo ate onde ela parou.
    public float impulsoParaCima = 6f;
    public float gravidade = 25f;

    private float nasceuEm;
    private SpriteRenderer spriteRenderer;
    private bool visivel = true;

    // Estado do voo. Uma moeda criada fora do Soltar (arrastada na cena, por
    // exemplo) nunca recebe o Pular, entao comeca como se ja estivesse no chao.
    private bool noChao = true;
    private float chaoY;
    private float velocidadeX;
    private float velocidadeY;

    // Larga "quantas" moedas em volta de um ponto. Ponto unico de nascimento de
    // dinheiro no chao.
    public static void Soltar(Vector3 onde, int quantas)
    {
        if (quantas <= 0) return;

        if (prefab == null) prefab = Resources.Load<GameObject>("Moeda");
        if (prefab == null) return;

        Moeda modelo = prefab.GetComponent<Moeda>();
        float espalhamento = (modelo != null) ? modelo.espalhamentoHorizontal : 0f;

        // As moedas nascem em fila, uma ao lado da outra, centradas em "onde". Com
        // cada uma sorteando a propria posicao, duas caiam uma em cima da outra quase
        // metade das vezes, e o jogador via uma moeda so onde havia duas: o rebelde
        // do fuzil parecia pagar o mesmo que o da faca.
        float meioDaFila = (quantas - 1) * 0.5f;

        for (int i = 0; i < quantas; i++)
        {
            // O sorteio continua, mas pequeno (um decimo do espalhamento): so para a
            // fila nao sair perfeita demais. No pior caso duas vizinhas ficam a 0,48
            // de distancia, e a moeda mede 0,47 - nunca se cobrem.
            float lado = ((i - meioDaFila) * espacoEntreMoedas)
                       + Random.Range(-espalhamento, espalhamento) * 0.1f;

            // A fila de 15 moedas do tanque tem 9 unidades: com ele encostado na
            // parede, metade caia do lado de fora, onde o jogador nao alcanca.
            lado = Cerco.ManterDentro(onde.x + lado) - onde.x;

            // Todas nascem no mesmo ponto, no corpo do rebelde, e o pulo e que as
            // leva ate o lugar delas na fila.
            GameObject go = Instantiate(prefab, onde, Quaternion.identity);
            Moeda moeda = go.GetComponent<Moeda>();
            if (moeda != null) moeda.Pular(lado);
        }
    }

    // Joga a moeda para cima de forma que ela caia "lado" unidades para o lado, na
    // mesma altura de onde saiu.
    //
    // A conta vem do lancamento vertical: subindo a "impulsoParaCima" contra a
    // "gravidade", ela leva 2 * impulso / gravidade segundos para voltar a altura de
    // saida. Dividindo o deslocamento por esse tempo, a velocidade para o lado sai
    // exata, e a moeda para no lugar da fila sem precisar de colisao com o chao.
    void Pular(float lado)
    {
        chaoY = transform.position.y;
        velocidadeY = impulsoParaCima;

        float tempoNoAr = (2f * impulsoParaCima) / gravidade;
        velocidadeX = lado / tempoNoAr;

        noChao = false;
    }

    void Start()
    {
        nasceuEm = Time.time;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // O voo e feito na mao, e nao com Rigidbody2D: a moeda com fisica empurraria o
    // jogador e precisaria de uma camada de colisao so para ela. Aqui sao tres
    // linhas, e ela para exatamente onde a fila mandou.
    void Voar()
    {
        velocidadeY = velocidadeY - (gravidade * Time.deltaTime);

        Vector3 p = transform.position;
        p.x = p.x + (velocidadeX * Time.deltaTime);
        p.y = p.y + (velocidadeY * Time.deltaTime);

        if (velocidadeY < 0f && p.y <= chaoY)
        {
            p.y = chaoY;
            noChao = true;
        }

        transform.position = p;
    }

    void Update()
    {
        // Fora da partida o Time.time nao anda, entao idade, prazo e pisca dariam
        // exatamente o mesmo resultado quadro apos quadro. Na tela de fim de jogo,
        // que fica parada ate o jogador apertar uma tecla, isso seria trabalho
        // infinito com resultado sempre igual.
        if (GameManager.estado != Estado.Jogando) return;

        if (noChao == false) Voar();

        float idade = Time.time - nasceuEm;

        if (idade >= tempoDeVida)
        {
            Destroy(gameObject);
            return;
        }

        if (spriteRenderer == null) return;

        // A idade escolhe o quadro, entao duas moedas que cairam em momentos
        // diferentes giram fora de fase, e o chao nao pisca inteiro no mesmo ritmo.
        if (quadros != null && quadros.Length > 0)
        {
            int quadro = (int)(idade * quadrosPorSegundo) % quadros.Length;
            spriteRenderer.sprite = quadros[quadro];
        }

        // O pisca acelera conforme o prazo acaba: a pressa e informacao, nao enfeite.
        float quantoFalta = tempoDeVida - idade;
        bool deveAparecer = true;

        if (quantoFalta <= avisoAntesDeSumir)
        {
            float frequencia = Mathf.Lerp(22f, 6f, quantoFalta / avisoAntesDeSumir);
            deveAparecer = Mathf.Sin(Time.time * frequencia) > -0.3f;
        }

        // Escrever em "enabled" atravessa a ponte para o motor toda vez, mesmo quando
        // o valor nao muda. Com uma duzia de moedas no chao, e o mesmo valor sendo
        // reescrito sessenta vezes por segundo em cada uma delas.
        if (deveAparecer != visivel)
        {
            visivel = deveAparecer;
            spriteRenderer.enabled = deveAparecer;
        }
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player") == false) return;

        GameManager.GanharMoedas(valor);
        Destroy(gameObject);
    }
}
